namespace CodeArchaeology.UI;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 800);
        Text = "Code Archaeology";

        // ToolStrip
        var toolStrip = new ToolStrip();
        var btnOpenFolder = new ToolStripButton("폴더 열기");
        var btnRefresh = new ToolStripButton("새로고침");
        btnOpenFolder.Click += btnOpenFolder_Click;
        btnRefresh.Click += btnRefresh_Click;
        toolStrip.Items.AddRange(new ToolStripItem[] { btnOpenFolder, btnRefresh });

        // Graph Panel
        pnlGraph = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };

        // StatusStrip
        var statusStrip = new StatusStrip();
        lblStatus = new ToolStripStatusLabel("폴더를 열어 분석을 시작하세요.")
        {
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        lblError = new ToolStripStatusLabel(string.Empty)
        {
            ForeColor = System.Drawing.Color.DarkRed,
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        var lblSpring = new ToolStripStatusLabel(string.Empty) { Spring = true };
        lblFolderPath = new ToolStripStatusLabel(string.Empty)
        {
            ForeColor = System.Drawing.Color.Gray,
            TextAlign = System.Drawing.ContentAlignment.MiddleRight
        };
        statusStrip.Items.Add(lblStatus);
        statusStrip.Items.Add(lblError);
        statusStrip.Items.Add(lblSpring);
        statusStrip.Items.Add(lblFolderPath);

        // Legend Panel (그래프 우상단 오버레이, 분석 전에는 숨김)
        pnlLegend = new Panel
        {
            Size = new Size(160, 100),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Visible = false
        };
        pnlLegend.Paint += pnlLegend_Paint;
        pnlGraph.Controls.Add(pnlLegend);

        Controls.Add(pnlGraph);
        Controls.Add(toolStrip);
        Controls.Add(statusStrip);
    }

    private Panel pnlGraph;
    private Panel pnlLegend;
    private ToolStripStatusLabel lblStatus;
    private ToolStripStatusLabel lblError;
    private ToolStripStatusLabel lblFolderPath;
}
