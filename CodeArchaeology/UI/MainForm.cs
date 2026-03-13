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
        // S-11에서 파이프라인 연결 예정
    }

    public void SetStatus(string message)
    {
        lblStatus.Text = message;
    }
}
