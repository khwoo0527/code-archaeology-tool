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
        // TODO S-11: 전체 파이프라인 연결 후 아래 임시 코드 제거

        var files = Analysis.FolderScanner.GetCsFiles(folderPath);
        var analyzer = new Analysis.RoslynAnalyzer();
        var result = analyzer.Analyze(files);

        var classCount = result.Nodes.Count(n => n.Kind == Models.TypeKind.Class);
        SetStatus($"발견된 클래스: {classCount}개 | .cs 파일: {files.Count}개 | 에러: {result.Errors.Count}개");
    }

    public void SetStatus(string message)
    {
        lblStatus.Text = message;
    }
}
