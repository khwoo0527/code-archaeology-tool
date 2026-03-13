namespace CodeArchaeology.UI;

public partial class MainForm : Form
{
    private string _lastFolderPath = string.Empty;

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
            RunAnalysis(_lastFolderPath);
        }
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_lastFolderPath))
        {
            SetStatus("먼저 폴더를 선택해 주세요.");
            return;
        }
        RunAnalysis(_lastFolderPath);
    }

    private void RunAnalysis(string folderPath)
    {
        SetStatus($"분석 중... ({Path.GetFileName(folderPath)})");
        Cursor = Cursors.WaitCursor;

        try
        {
            var files = Analysis.FolderScanner.GetCsFiles(folderPath);
            var analyzer = new Analysis.RoslynAnalyzer();
            var result = analyzer.Analyze(files);

            var renderer = new Rendering.MsaglRenderer();
            var gViewer = renderer.BuildViewer(result);

            pnlGraph.Controls.Clear();
            pnlGraph.Controls.Add(gViewer);

            var classCount = result.Nodes.Count(n => n.Kind == Models.TypeKind.Class);
            var interfaceCount = result.Nodes.Count(n => n.Kind == Models.TypeKind.Interface);
            var errorText = result.Errors.Count > 0 ? $" | ⚠ 에러: {result.Errors.Count}개" : string.Empty;
            SetStatus($"분석 완료 — 클래스: {classCount}개 | 인터페이스: {interfaceCount}개 | .cs 파일: {files.Count}개{errorText}");
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    public void SetStatus(string message)
    {
        lblStatus.Text = message;
    }
}
