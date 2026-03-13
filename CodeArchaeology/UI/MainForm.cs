namespace CodeArchaeology.UI;

public partial class MainForm : Form
{
    private string _lastFolderPath = string.Empty;
    private Models.AnalysisResult? _analysisResult;

    public MainForm()
    {
        InitializeComponent();
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
        var gViewer = renderer.BuildViewer(result);

        pnlGraph.Controls.Clear();
        pnlGraph.Controls.Add(gViewer);

        pnlLegend.Location = new Point(pnlGraph.ClientSize.Width - pnlLegend.Width - 12, 12);
        pnlGraph.Controls.Add(pnlLegend);
        pnlLegend.Visible = true;
        pnlLegend.BringToFront();
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
