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
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _lastFolderPath = dialog.SelectedPath;
            RunAnalysis(_lastFolderPath);
        }
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(_lastFolderPath))
            RunAnalysis(_lastFolderPath);
    }

    private void RunAnalysis(string folderPath)
    {
        SetStatus("분석 중...");

        var files = Analysis.FolderScanner.GetCsFiles(folderPath);
        var analyzer = new Analysis.RoslynAnalyzer();
        var result = analyzer.Analyze(files);

        // 그래프 렌더링
        var renderer = new Rendering.MsaglRenderer();
        var gViewer = renderer.BuildViewer(result);

        pnlGraph.Controls.Clear();
        pnlGraph.Controls.Add(gViewer);

        var classCount = result.Nodes.Count(n => n.Kind == Models.TypeKind.Class);
        var interfaceCount = result.Nodes.Count(n => n.Kind == Models.TypeKind.Interface);
        SetStatus($"클래스: {classCount}개 | 인터페이스: {interfaceCount}개 | .cs 파일: {files.Count}개 | 에러: {result.Errors.Count}개");
    }

    public void SetStatus(string message)
    {
        lblStatus.Text = message;
    }
}
