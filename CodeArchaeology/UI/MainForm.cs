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
        // TODO S-11: 전체 파이프라인 연결 예정

        // S-04 임시 검증용 — S-11 파이프라인 연결 후 제거
        var files = Analysis.FolderScanner.GetCsFiles(folderPath);
        SetStatus($"발견된 .cs 파일: {files.Count}개");
    }

    public void SetStatus(string message)
    {
        lblStatus.Text = message;
    }
}
