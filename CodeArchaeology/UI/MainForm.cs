namespace CodeArchaeology.UI;

public partial class MainForm : Form
{
    private string _lastFolderPath = string.Empty;
    private string _currentSearch  = string.Empty;
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

    // ── Graph Rebuild ────────────────────────────────────────────────────

    private void RebuildGraph(Models.AnalysisResult result)
    {
        var renderer = new Rendering.MsaglRenderer();
        _gViewer = renderer.BuildViewer(result, _currentSearch);
        _gViewer.MouseClick += gViewer_MouseClick;

        pnlGraph.Controls.Clear();
        pnlGraph.Controls.Add(_gViewer);

        pnlLegend.Location = new Point(pnlGraph.ClientSize.Width - pnlLegend.Width - 12, 12);
        pnlGraph.Controls.Add(pnlLegend);
        pnlLegend.Visible = true;
        pnlLegend.BringToFront();
    }

    // ── Class Info ───────────────────────────────────────────────────────

    private void gViewer_MouseClick(object? sender, MouseEventArgs e)
    {
        if (_gViewer?.ObjectUnderMouseCursor is Microsoft.Msagl.Drawing.IViewerNode viewerNode
            && _analysisResult != null)
        {
            var nodeId = viewerNode.Node.Id;
            var typeNode = _analysisResult.Nodes.FirstOrDefault(n => n.FullName == nodeId);
            if (typeNode != null)
            {
                ShowClassInfo(typeNode);
                return;
            }
        }
        ClearClassInfo();
    }

    private void ShowClassInfo(Models.TypeNode node)
    {
        lblInfoName.Text       = node.Name;
        lblInfoKindVal.Text    = node.Kind == Models.TypeKind.Interface ? "Interface" : "Class";
        lblInfoNsVal.Text      = node.Namespace;
        lblInfoFileVal.Text    = Path.GetFileName(node.FilePath);
        lblInfoFieldsVal.Text  = node.FieldCount == 0
            ? "0"
            : $"{node.FieldCount}  ({string.Join(", ", node.FieldNames)})";
        lblInfoMethodsVal.Text = node.MethodCount == 0
            ? "0"
            : $"{node.MethodCount}  ({string.Join(", ", node.MethodNames)})";

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
        lblInfoFieldsVal.Text  = "—";
        lblInfoMethodsVal.Text = "—";
        lblInfoDepsVal.Text    = "—";
        lblMetricCaVal.Text    = "—";
        lblMetricCeVal.Text    = "—";
        lblMetricInstVal.Text  = "—";
    }

    // ── Legend Paint ─────────────────────────────────────────────────────

    private void pnlLegend_Paint(object? sender, PaintEventArgs e)
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
    }

    public void SetStatus(string message) => lblStatus.Text = message;
}
