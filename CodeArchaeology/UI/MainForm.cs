namespace CodeArchaeology.UI;

public partial class MainForm : Form
{
    private string _lastFolderPath = string.Empty;
    private string _currentSearch  = string.Empty;
    private string _focusNodeId    = string.Empty;
    private Models.AnalysisResult? _analysisResult;
    private Microsoft.Msagl.GraphViewerGdi.GViewer? _gViewer;

    public MainForm()
    {
        InitializeComponent();
        Shown += (_, _) =>
        {
            splitOuter.Panel1MinSize = 120;
            splitOuter.Panel2MinSize = 400;
            splitOuter.SplitterDistance = 190;

            splitInner.Panel1MinSize = 300;
            splitInner.Panel2MinSize = 180;
            splitInner.SplitterDistance = Math.Max(300, splitInner.Width - 230);

            splitLeft.Panel1MinSize = 80;
            splitLeft.Panel2MinSize = 60;
            splitLeft.SplitterDistance = splitLeft.Height / 2;

            splitRight.Panel1MinSize = 80;
            splitRight.Panel2MinSize = 60;
            splitRight.SplitterDistance = splitRight.Height / 2;
        };
    }

    private void btnOpenFolder_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog();
        if (!string.IsNullOrEmpty(_lastFolderPath))
            dialog.InitialDirectory = _lastFolderPath;

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _lastFolderPath = dialog.SelectedPath;
            _ = RunAnalysisAsync(_lastFolderPath);
        }
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_lastFolderPath))
        {
            SetStatus("먼저 폴더를 선택해 주세요.");
            return;
        }
        _ = RunAnalysisAsync(_lastFolderPath);
    }

    private void btnExportPng_Click(object sender, EventArgs e)
    {
        if (_gViewer == null)
        {
            SetStatus("내보낼 그래프가 없습니다. 먼저 폴더를 열어주세요.");
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title            = "그래프를 PNG로 저장",
            Filter           = "PNG 이미지 (*.png)|*.png",
            DefaultExt       = "png",
            FileName         = $"graph_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        // DrawToBitmap은 오버레이 컨트롤(범례 등)을 캡처하지 못함
        // → 실제 화면 픽셀을 직접 복사
        var screenRect = pnlGraph.RectangleToScreen(new Rectangle(Point.Empty, pnlGraph.Size));
        using var bitmap = new System.Drawing.Bitmap(screenRect.Width, screenRect.Height);
        using (var g = System.Drawing.Graphics.FromImage(bitmap))
            g.CopyFromScreen(screenRect.Location, Point.Empty, screenRect.Size);
        bitmap.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);

        SetStatus($"저장 완료: {dialog.FileName}");
    }

    private async Task RunAnalysisAsync(string folderPath)
    {
        SetStatus($"분석 중... ({Path.GetFileName(folderPath)})");
        Cursor = Cursors.WaitCursor;

        try
        {
            var (result, files) = await Task.Run(() =>
            {
                var csFiles = Analysis.FolderScanner.GetCsFiles(folderPath);
                var analyzer = new Analysis.RoslynAnalyzer();
                return (analyzer.Analyze(csFiles), csFiles);
            });

            _analysisResult = result;

            PopulateNamespaceFilter();
            PopulateErrorLog();
            RebuildGraph(_analysisResult);

            var classCount = result.Nodes.Count(n => n.Kind == Models.TypeKind.Class);
            var interfaceCount = result.Nodes.Count(n => n.Kind == Models.TypeKind.Interface);
            SetStatus($"분석 완료 — 클래스: {classCount}개 | 인터페이스: {interfaceCount}개 | .cs 파일: {files.Count}개");
            lblError.Text = result.Errors.Count > 0 ? $"⚠ 에러: {result.Errors.Count}개" : string.Empty;
            lblFolderPath.Text = folderPath;
        }
        catch (Exception ex)
        {
            SetStatus($"분석 실패: {ex.Message}");
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    // ── Pan Mode ─────────────────────────────────────────────────────────

    private bool  _codeSmellMode   = false;
    private bool  _panToggled      = false;
    private bool  _spaceHeld       = false;
    private Point _mouseDownPoint  = Point.Empty;
    private bool  _wasDragged      = false;

    // 영향 분석 상태
    private string          _impactRootId = string.Empty;
    private HashSet<string> _impactSet    = new();

    private void btnCodeSmell_Click(object? sender, EventArgs e)
    {
        _codeSmellMode = !_codeSmellMode;
        UpdateCodeSmellButton();
        RebuildGraphFiltered();
    }

    private void UpdateCodeSmellButton()
    {
        btnCodeSmell.BackColor = _codeSmellMode
            ? Color.FromArgb(100, 70, 10)
            : Color.FromArgb(55, 55, 60);
        btnCodeSmell.Text = _codeSmellMode ? "📊 코드 스멜 해제" : "📊 코드 스멜";
    }

    private void btnPanMode_Click(object? sender, EventArgs e)
    {
        _panToggled = !_panToggled;
        ApplyPanMode(_panToggled);
        UpdatePanButton();
    }

    private void ApplyPanMode(bool pan)
    {
        if (_gViewer == null) return;

        // MSAGL 1.1.6 — PanButton/SelectButton이 public API가 아니므로 리플렉션으로 접근
        var type = _gViewer.GetType();
        SetButtonChecked(type, _gViewer, "panButton",    pan);
        SetButtonChecked(type, _gViewer, "selectButton", !pan);

        _gViewer.Cursor = pan ? Cursors.Hand : Cursors.Default;
    }

    private static void SetButtonChecked(Type viewerType, object viewer, string fieldName, bool value)
    {
        var field = viewerType.GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field?.GetValue(viewer) is ToolStripButton btn)
            btn.Checked = value;
    }

    private void UpdatePanButton()
    {
        var active = _panToggled || _spaceHeld;
        btnPanMode.BackColor = active
            ? Color.FromArgb(0, 100, 180)
            : Color.FromArgb(50, 50, 54);
        btnPanMode.ForeColor = active
            ? Color.White
            : Color.FromArgb(180, 180, 190);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Space && !_spaceHeld && _gViewer != null)
        {
            _spaceHeld = true;
            ApplyPanMode(true);
            UpdatePanButton();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space && _spaceHeld)
        {
            _spaceHeld = false;
            ApplyPanMode(_panToggled);
            UpdatePanButton();
        }
        base.OnKeyUp(e);
    }

    // ── Search ───────────────────────────────────────────────────────────

    private void txtSearch_TextChanged(object? sender, EventArgs e)
    {
        _currentSearch = txtSearch.Text;
        RebuildGraphFiltered();
    }

    // ── Namespace Filter ─────────────────────────────────────────────────

    private void PopulateNamespaceFilter()
    {
        if (_analysisResult == null) return;

        clbNamespaces.ItemCheck -= clbNamespaces_ItemCheck;
        clbNamespaces.Items.Clear();

        var namespaces = _analysisResult.Nodes
            .Select(n => n.Namespace)
            .Distinct()
            .OrderBy(ns => ns)
            .ToList();

        foreach (var ns in namespaces)
            clbNamespaces.Items.Add(ns, isChecked: true);

        clbNamespaces.ItemCheck += clbNamespaces_ItemCheck;
    }

    private void chkAllNamespaces_CheckedChanged(object? sender, EventArgs e)
    {
        clbNamespaces.ItemCheck -= clbNamespaces_ItemCheck;
        for (int i = 0; i < clbNamespaces.Items.Count; i++)
            clbNamespaces.SetItemChecked(i, chkAllNamespaces.Checked);
        clbNamespaces.ItemCheck += clbNamespaces_ItemCheck;
        RebuildGraphFiltered();
    }

    private void clbNamespaces_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        // ItemCheck는 상태 변경 전에 발생 — BeginInvoke로 변경 완료 후 실행
        BeginInvoke(RebuildGraphFiltered);
    }

    private void RebuildGraphFiltered()
    {
        if (_analysisResult == null) return;

        var selectedNs = Enumerable.Range(0, clbNamespaces.Items.Count)
            .Where(i => clbNamespaces.GetItemChecked(i))
            .Select(i => clbNamespaces.Items[i]!.ToString()!)
            .ToHashSet();

        var filtered = new Models.AnalysisResult();
        filtered.Nodes.AddRange(_analysisResult.Nodes.Where(n => selectedNs.Contains(n.Namespace)));

        var filteredNames = filtered.Nodes.Select(n => n.Name).ToHashSet();
        filtered.Edges.AddRange(_analysisResult.Edges.Where(e =>
            filteredNames.Contains(e.Source) && filteredNames.Contains(e.Target)));

        RebuildGraph(filtered);
    }

    // ── Error Log ────────────────────────────────────────────────────────

    private void lstErrors_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= lstErrors.Items.Count) return;

        var text = lstErrors.Items[e.Index]?.ToString() ?? string.Empty;
        var bg = e.Index % 2 == 0
            ? Color.FromArgb(45, 45, 48)
            : Color.FromArgb(50, 50, 54);

        e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);

        // 빨간 ● 인디케이터
        using var dotBrush = new SolidBrush(Color.FromArgb(220, 80, 80));
        e.Graphics.FillEllipse(dotBrush, e.Bounds.X + 6, e.Bounds.Y + 7, 8, 8);

        // 에러 텍스트
        using var textBrush = new SolidBrush(Color.FromArgb(220, 170, 140));
        e.Graphics.DrawString(text, lstErrors.Font, textBrush,
            new System.Drawing.RectangleF(e.Bounds.X + 20, e.Bounds.Y + 4,
                e.Bounds.Width - 22, e.Bounds.Height - 4));
    }

    private void PopulateErrorLog()
    {
        lstErrors.Items.Clear();
        if (_analysisResult == null) return;

        foreach (var err in _analysisResult.Errors)
            lstErrors.Items.Add(err);
    }

    // ── 영향 분석 ────────────────────────────────────────────────────────

    private void btnImpact_Click(object? sender, EventArgs e)
    {
        // 영향 분석 활성 상태 → 해제
        if (!string.IsNullOrEmpty(_impactRootId))
        {
            _impactRootId = string.Empty;
            _impactSet    = new();
            UpdateImpactButton();
            RebuildGraphFiltered();
            SetStatus("영향 분석 해제");
            return;
        }

        // 선택된 노드가 없으면 무시
        if (string.IsNullOrEmpty(_focusNodeId) || _analysisResult == null) return;

        _impactRootId = _focusNodeId;
        _impactSet    = ComputeImpactSet(_impactRootId);
        UpdateImpactButton();
        RebuildGraphFiltered();
        SetStatus($"영향 분석: [{_impactRootId}] — 영향 범위 {_impactSet.Count}개 클래스");
    }

    private void UpdateImpactButton()
    {
        var active = !string.IsNullOrEmpty(_impactRootId);
        btnImpact.BackColor = active
            ? Color.FromArgb(160, 80, 10)
            : Color.FromArgb(55, 55, 60);
        btnImpact.Text = active ? "🔍 영향 분석 해제" : "🔍 영향 분석";
    }

    // 역방향 BFS — rootId를 직간접적으로 참조하는 모든 노드 반환 (root 제외)
    private HashSet<string> ComputeImpactSet(string rootId)
    {
        var visited = new HashSet<string>();
        var queue   = new Queue<string>();
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var edge in _analysisResult!.Edges)
            {
                if (edge.Target == current && visited.Add(edge.Source))
                    queue.Enqueue(edge.Source);
            }
        }

        visited.Remove(rootId);
        return visited;
    }

    // ── Graph Rebuild ────────────────────────────────────────────────────

    private void RebuildGraph(Models.AnalysisResult result)
    {
        // 리빌드 전 줌/오프셋 저장 → 리빌드 후 복원
        var savedZoom = _gViewer?.ZoomF ?? 0;

        var renderer = new Rendering.MsaglRenderer();
        _gViewer = renderer.BuildViewer(result, _currentSearch, _focusNodeId, _impactRootId, _impactSet, _codeSmellMode);
        _gViewer.ToolBarIsVisible = false;
        _gViewer.MouseDown  += gViewer_MouseDown;
        _gViewer.MouseClick += gViewer_MouseClick;
        _gViewer.MouseMove  += gViewer_MouseMove;

        pnlGraph.Controls.Clear();
        pnlGraph.Controls.Add(_gViewer);

        pnlLegend.Location = new Point(pnlGraph.ClientSize.Width - pnlLegend.Width - 12, 12);
        pnlGraph.Controls.Add(pnlLegend);
        pnlLegend.Visible = true;
        pnlLegend.BringToFront();

        // Controls.Clear() 후 재추가 필요
        btnPanMode.Location = new Point(12, 12);
        pnlGraph.Controls.Add(btnPanMode);
        btnPanMode.Visible = true;
        btnPanMode.BringToFront();

        btnImpact.Location = new Point(12 + btnPanMode.Width + 8, 12);
        btnImpact.Enabled  = !string.IsNullOrEmpty(_focusNodeId) || !string.IsNullOrEmpty(_impactRootId);
        pnlGraph.Controls.Add(btnImpact);
        btnImpact.Visible = true;
        btnImpact.BringToFront();

        btnCodeSmell.Location = new Point(12 + btnPanMode.Width + 8 + btnImpact.Width + 8, 12);
        pnlGraph.Controls.Add(btnCodeSmell);
        btnCodeSmell.Visible = true;
        btnCodeSmell.BringToFront();

        // 줌 복원 (0이면 최초 로드 — 기본값 유지)
        if (savedZoom > 0)
            _gViewer.ZoomF = savedZoom;

        // GViewer가 패널에 추가된 후 pan 상태 복원
        ApplyPanMode(_panToggled);
        UpdatePanButton();
        UpdateImpactButton();
        UpdateCodeSmellButton();
    }

    // ── Class Info ───────────────────────────────────────────────────────

    private string _lastTooltipNodeId = string.Empty;

    private void gViewer_MouseMove(object? sender, MouseEventArgs e)
    {
        // 마우스 버튼 눌린 채 이동 → 드래그로 판정 (팬 중 click 이벤트 차단용)
        if (e.Button != MouseButtons.None && _mouseDownPoint != Point.Empty)
        {
            var dx = e.X - _mouseDownPoint.X;
            var dy = e.Y - _mouseDownPoint.Y;
            if (dx * dx + dy * dy > 25) // 5px 이상 이동
                _wasDragged = true;
        }

        if (_gViewer == null || _analysisResult == null) return;

        if (_gViewer.ObjectUnderMouseCursor is Microsoft.Msagl.Drawing.IViewerNode viewerNode)
        {
            var nodeId   = viewerNode.Node.Id;
            var typeNode = _analysisResult.Nodes.FirstOrDefault(n => n.FullName == nodeId);
            if (typeNode != null)
            {
                // 같은 노드 위에서 계속 MouseMove 발생 시 중복 Show 방지
                if (_lastTooltipNodeId == nodeId) return;
                _lastTooltipNodeId = nodeId;

                var kind = typeNode.Kind == Models.TypeKind.Interface ? "Interface" : "Class";
                var tip  = $"{typeNode.Name}  [{kind}]\n" +
                           $"Namespace : {typeNode.Namespace}\n" +
                           $"Fields    : {typeNode.FieldCount}   Methods : {typeNode.MethodCount}";
                nodeToolTip.Show(tip, _gViewer, e.X + 16, e.Y + 10, 6000);
                return;
            }
        }

        if (_lastTooltipNodeId != string.Empty)
        {
            _lastTooltipNodeId = string.Empty;
            nodeToolTip.Hide(_gViewer);
        }
    }

    private void gViewer_MouseDown(object? sender, MouseEventArgs e)
    {
        _mouseDownPoint = e.Location;
        _wasDragged     = false;
    }

    private void gViewer_MouseClick(object? sender, MouseEventArgs e)
    {
        // 드래그(팬) 후 mouseup = click 이벤트 발생 → 그래프 리셋 방지
        if (_wasDragged)
        {
            _wasDragged = false;
            return;
        }

        if (e.Button == MouseButtons.Right) return;

        if (_gViewer?.ObjectUnderMouseCursor is Microsoft.Msagl.Drawing.IViewerNode viewerNode
            && _analysisResult != null)
        {
            var nodeId   = viewerNode.Node.Id;
            var typeNode = _analysisResult.Nodes.FirstOrDefault(n => n.FullName == nodeId);
            if (typeNode != null)
            {
                // 같은 노드 재클릭 → 포커스 해제
                if (_focusNodeId == typeNode.Name)
                {
                    _focusNodeId = string.Empty;
                    ClearClassInfo();
                }
                else
                {
                    _focusNodeId = typeNode.Name;
                    ShowClassInfo(typeNode);
                }
                btnImpact.Enabled = !string.IsNullOrEmpty(_focusNodeId) || !string.IsNullOrEmpty(_impactRootId);
                RebuildGraphFiltered();
                return;
            }
        }
        // 빈 곳 클릭 → 포커스/영향 분석만 해제. 코드 스멜은 유지.
        // 실제로 변경된 게 없으면 리빌드 하지 않음 (코드 스멜 뷰 유지)
        var hadFocus  = !string.IsNullOrEmpty(_focusNodeId);
        var hadImpact = !string.IsNullOrEmpty(_impactRootId);

        _focusNodeId  = string.Empty;
        _impactRootId = string.Empty;
        _impactSet    = new();
        btnImpact.Enabled = false;
        UpdateImpactButton();
        ClearClassInfo();

        if (hadFocus || hadImpact)
            RebuildGraphFiltered();
    }

    private void ShowClassInfo(Models.TypeNode node)
    {
        lblInfoName.Text       = node.Name;
        lblInfoKindVal.Text    = node.Kind.ToString();
        lblInfoNsVal.Text      = node.Namespace;
        lblInfoFileVal.Text    = Path.GetFileName(node.FilePath);
        _fieldsExpanded  = false;
        _methodsExpanded = false;
        PopulateDetailPanel(pnlFieldsDetail,  node.FieldNames);
        PopulateDetailPanel(pnlMethodsDetail, node.MethodNames);
        pnlFieldsDetail.Visible  = false;
        pnlMethodsDetail.Visible = false;
        lblInfoFieldsVal.Text  = FormatCountLabel(false, node.FieldCount);
        lblInfoMethodsVal.Text = FormatCountLabel(false, node.MethodCount);

        var deps = _analysisResult!.Edges
            .Where(e => e.Source == node.Name)
            .Select(e => e.Target)
            .Distinct()
            .ToList();
        lblInfoDepsVal.Text = deps.Count == 0 ? "—" : string.Join(", ", deps);

        // Dependency Metrics
        var ca = _analysisResult.Edges.Count(e => e.Target == node.Name);
        var ce = _analysisResult.Edges.Count(e => e.Source == node.Name);
        var instability = (ca + ce) == 0 ? 0.0 : (double)ce / (ca + ce);

        lblMetricCaVal.Text   = ca.ToString();
        lblMetricCeVal.Text   = ce.ToString();
        lblMetricInstVal.Text = instability.ToString("F2");
    }

    private void ClearClassInfo()
    {
        lblInfoName.Text       = "—";
        lblInfoKindVal.Text    = "—";
        lblInfoNsVal.Text      = "—";
        lblInfoFileVal.Text    = "—";
        _fieldsExpanded  = false;
        _methodsExpanded = false;
        pnlFieldsDetail.Visible  = false;
        pnlMethodsDetail.Visible = false;
        lblInfoFieldsVal.Text  = "—";
        lblInfoMethodsVal.Text = "—";
        lblInfoDepsVal.Text    = "—";
        lblMetricCaVal.Text    = "—";
        lblMetricCeVal.Text    = "—";
        lblMetricInstVal.Text  = "—";
    }

    // ── Node Tooltip ─────────────────────────────────────────────────────

    private void nodeToolTip_Popup(object? sender, PopupEventArgs e)
    {
        using var font = new Font("Segoe UI", 9f);
        var size = TextRenderer.MeasureText(
            nodeToolTip.GetToolTip(e.AssociatedControl!),
            font,
            new Size(280, 0),
            TextFormatFlags.Left | TextFormatFlags.WordBreak);
        e.ToolTipSize = new Size(size.Width + 24, size.Height + 20);
    }

    private void nodeToolTip_Draw(object? sender, DrawToolTipEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // 배경
        g.FillRectangle(new SolidBrush(Color.FromArgb(40, 40, 45)), e.Bounds);

        // 상단 파란 액센트 바
        g.FillRectangle(new SolidBrush(Color.FromArgb(0, 122, 204)),
            new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, 3));

        // 테두리
        g.DrawRectangle(new Pen(Color.FromArgb(70, 70, 85)),
            new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1));

        // 텍스트
        using var font = new Font("Segoe UI", 9f);
        TextRenderer.DrawText(g, e.ToolTipText, font,
            new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 8, e.Bounds.Width - 20, e.Bounds.Height - 12),
            Color.FromArgb(220, 220, 225),
            TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak);
    }

    // ── Legend ───────────────────────────────────────────────────────────

    private bool _legendExpanded  = true;
    private bool _fieldsExpanded  = false;
    private bool _methodsExpanded = false;

    private void lblLegendHeader_Click(object? sender, EventArgs e)
    {
        _legendExpanded = !_legendExpanded;
        lblLegendHeader.Text     = _legendExpanded ? "범례  ▼" : "범례  ▶";
        pnlLegendContent.Visible = _legendExpanded;
        pnlLegend.Size = _legendExpanded
            ? new Size(164, 26 + 218)
            : new Size(164, 26);
    }

    private void rowFieldsPanel_Click(object? sender, EventArgs e)
    {
        _fieldsExpanded = !_fieldsExpanded;
        lblInfoFieldsVal.Text    = FormatCountLabel(_fieldsExpanded, pnlFieldsDetail.Controls.Count);
        pnlFieldsDetail.Visible  = _fieldsExpanded;
    }

    private void rowMethodsPanel_Click(object? sender, EventArgs e)
    {
        _methodsExpanded = !_methodsExpanded;
        lblInfoMethodsVal.Text    = FormatCountLabel(_methodsExpanded, pnlMethodsDetail.Controls.Count);
        pnlMethodsDetail.Visible  = _methodsExpanded;
    }

    private static string FormatCountLabel(bool expanded, int count)
        => count == 0 ? "0" : $"{count}  {(expanded ? "▼" : "▶")}";

    private void PopulateDetailPanel(Panel panel, IEnumerable<string> names)
    {
        panel.Controls.Clear();
        const int itemH = 18;
        int i = 0;
        foreach (var name in names)
        {
            panel.Controls.Add(new Label
            {
                Text      = "· " + name,
                Left      = 0, Top = i * itemH,
                Width     = panel.Width - 4, Height = itemH,
                ForeColor = Color.FromArgb(180, 200, 230),
                Font      = new Font("Segoe UI", 8f),
                BackColor = Color.Transparent,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            });
            i++;
        }
        panel.Height = Math.Max(0, i * itemH + 6);
    }

    private void pnlLegendContent_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var titleFont = new Font("Segoe UI", 8f, FontStyle.Bold);
        using var labelFont = new Font("Segoe UI", 8f);
        var textBrush = new SolidBrush(Color.FromArgb(204, 204, 204));

        int xBox = 10, xText = 50;
        int y = 8;

        g.DrawString("범  례", titleFont, textBrush, xBox, y);
        y += 20;

        g.FillRectangle(new SolidBrush(Color.FromArgb(210, 230, 255)), xBox, y, 30, 14);
        g.DrawRectangle(new Pen(Color.FromArgb(30, 80, 160), 1.5f), xBox, y, 30, 14);
        g.DrawString("클래스", labelFont, textBrush, xText, y);
        y += 20;

        g.FillEllipse(new SolidBrush(Color.FromArgb(230, 210, 255)), xBox, y, 30, 14);
        g.DrawEllipse(new Pen(Color.FromArgb(120, 60, 180), 1.5f), xBox, y, 30, 14);
        g.DrawString("인터페이스", labelFont, textBrush, xText, y);
        y += 20;

        // 다이아몬드(struct) 근사 — 사각형 45도 회전 대신 간단히 표시
        g.FillRectangle(new SolidBrush(Color.FromArgb(40, 90, 70)), xBox, y, 30, 14);
        g.DrawRectangle(new Pen(Color.FromArgb(80, 200, 150), 1.5f), xBox, y, 30, 14);
        g.DrawString("struct", labelFont, textBrush, xText, y);
        y += 20;

        g.FillRectangle(new SolidBrush(Color.FromArgb(90, 70, 30)), xBox, y, 30, 14);
        g.DrawRectangle(new Pen(Color.FromArgb(220, 180, 80), 1.5f), xBox, y, 30, 14);
        g.DrawString("record", labelFont, textBrush, xText, y);
        y += 20;

        g.FillRectangle(new SolidBrush(Color.FromArgb(60, 50, 90)), xBox, y, 30, 14);
        g.DrawRectangle(new Pen(Color.FromArgb(160, 130, 240), 1.5f), xBox, y, 30, 14);
        g.DrawString("enum", labelFont, textBrush, xText, y);
        y += 22;

        using (var pen = new Pen(Color.FromArgb(180, 180, 180), 2f))
            g.DrawLine(pen, xBox, y + 6, xBox + 30, y + 6);
        g.DrawString("상속", labelFont, textBrush, xText, y);
        y += 20;

        using (var pen = new Pen(Color.FromArgb(100, 150, 255), 2f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
            g.DrawLine(pen, xBox, y + 6, xBox + 30, y + 6);
        g.DrawString("인터페이스 구현", labelFont, textBrush, xText, y);
        y += 20;

        using (var pen = new Pen(Color.FromArgb(130, 130, 130), 2f))
            g.DrawLine(pen, xBox, y + 6, xBox + 30, y + 6);
        g.DrawString("필드 의존성", labelFont, textBrush, xText, y);
        y += 20;

        using (var pen = new Pen(Color.FromArgb(220, 60, 60), 2.5f))
            g.DrawLine(pen, xBox, y + 6, xBox + 30, y + 6);
        g.DrawString("순환 의존성", labelFont, new SolidBrush(Color.FromArgb(220, 100, 100)), xText, y);
    }

    public void SetStatus(string message) => lblStatus.Text = message;
}
