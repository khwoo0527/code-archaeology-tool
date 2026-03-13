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
            // CPU 바운드 분석 작업을 백그라운드 스레드에서 실행 — UI 프리징 방지
            var (result, files) = await Task.Run(() =>
            {
                var csFiles = Analysis.FolderScanner.GetCsFiles(folderPath);
                var analyzer = new Analysis.RoslynAnalyzer();
                return (analyzer.Analyze(csFiles), csFiles);
            });

            // GViewer 생성 및 UI 반영은 반드시 UI 스레드에서
            var renderer = new Rendering.MsaglRenderer();
            var gViewer = renderer.BuildViewer(result);

            pnlGraph.Controls.Clear();
            pnlGraph.Controls.Add(gViewer);

            // 범례 패널 재추가 (Controls.Clear() 이후 복원) 및 우상단 위치 고정
            pnlLegend.Location = new Point(pnlGraph.ClientSize.Width - pnlLegend.Width - 12, 12);
            pnlGraph.Controls.Add(pnlLegend);
            pnlLegend.Visible = true;
            pnlLegend.BringToFront();

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

    private void pnlLegend_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        using var titleFont = new Font("Segoe UI", 8f, FontStyle.Bold);
        using var labelFont = new Font("Segoe UI", 8f);

        int xBox = 10, xText = 50;
        int y = 8;

        g.DrawString("범  례", titleFont, Brushes.Black, xBox, y);
        y += 20;

        // 클래스: 연파랑 사각형
        g.FillRectangle(new SolidBrush(Color.FromArgb(210, 230, 255)), xBox, y, 30, 14);
        g.DrawRectangle(new Pen(Color.FromArgb(30, 80, 160), 1.5f), xBox, y, 30, 14);
        g.DrawString("클래스", labelFont, Brushes.Black, xText, y);
        y += 20;

        // 인터페이스: 연보라 타원
        g.FillEllipse(new SolidBrush(Color.FromArgb(230, 210, 255)), xBox, y, 30, 14);
        g.DrawEllipse(new Pen(Color.FromArgb(120, 60, 180), 1.5f), xBox, y, 30, 14);
        g.DrawString("인터페이스", labelFont, Brushes.Black, xText, y);
        y += 22;

        // 상속: 검정 실선
        using (var pen = new Pen(Color.Black, 2f))
        {
            g.DrawLine(pen, xBox, y + 6, xBox + 30, y + 6);
        }
        g.DrawString("상속", labelFont, Brushes.Black, xText, y);
        y += 20;

        // 인터페이스 구현: 파랑 점선
        using (var pen = new Pen(Color.Blue, 2f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
        {
            g.DrawLine(pen, xBox, y + 6, xBox + 30, y + 6);
        }
        g.DrawString("인터페이스 구현", labelFont, Brushes.Black, xText, y);
        y += 20;

        // 필드 의존성: 회색 실선
        using (var pen = new Pen(Color.Gray, 2f))
        {
            g.DrawLine(pen, xBox, y + 6, xBox + 30, y + 6);
        }
        g.DrawString("필드 의존성", labelFont, Brushes.Black, xText, y);
    }

    public void SetStatus(string message)
    {
        lblStatus.Text = message;
    }
}
