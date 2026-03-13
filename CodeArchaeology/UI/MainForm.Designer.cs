namespace CodeArchaeology.UI;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1400, 860);
        Text = "Code Archaeology";
        BackColor = Color.FromArgb(30, 30, 30);
        Font = new Font("Segoe UI", 9f);

        // ── ToolStrip ────────────────────────────────────────────────────
        var toolStrip = new ToolStrip { Renderer = new DarkToolStripRenderer() };
        var btnOpenFolder = new ToolStripButton("폴더 열기");
        var btnRefresh = new ToolStripButton("새로고침");
        btnOpenFolder.Click += btnOpenFolder_Click;
        btnRefresh.Click += btnRefresh_Click;
        toolStrip.Items.AddRange(new ToolStripItem[] { btnOpenFolder, btnRefresh });

        // ── StatusStrip ──────────────────────────────────────────────────
        var statusStrip = new StatusStrip
        {
            BackColor = Color.FromArgb(0, 122, 204),
            Renderer = new ToolStripProfessionalRenderer(new BlueStatusColorTable())
        };
        lblStatus = new ToolStripStatusLabel("폴더를 열어 분석을 시작하세요.")
        {
            ForeColor = Color.White,
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        lblError = new ToolStripStatusLabel(string.Empty)
        {
            ForeColor = Color.FromArgb(255, 220, 100),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        var lblSpring = new ToolStripStatusLabel(string.Empty) { Spring = true };
        lblFolderPath = new ToolStripStatusLabel(string.Empty)
        {
            ForeColor = Color.FromArgb(200, 235, 255),
            TextAlign = System.Drawing.ContentAlignment.MiddleRight
        };
        statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblError, lblSpring, lblFolderPath });

        // ── Left Panel ───────────────────────────────────────────────────
        pnlLeft = new Panel
        {
            Dock = DockStyle.Left,
            Width = 190,
            BackColor = Color.FromArgb(37, 37, 38)
        };

        var lblNsHeader = MakeSectionHeader("NAMESPACE");
        clbNamespaces = new CheckedListBox
        {
            Dock = DockStyle.Top,
            Height = 220,
            BackColor = Color.FromArgb(45, 45, 48),
            ForeColor = Color.FromArgb(204, 204, 204),
            BorderStyle = BorderStyle.None,
            CheckOnClick = true,
            Font = new Font("Segoe UI", 8.5f)
        };

        var divider = new Label
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = Color.FromArgb(60, 60, 65)
        };

        var lblErrHeader = MakeSectionHeader("ERROR LOG");
        lstErrors = new ListBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 48),
            ForeColor = Color.FromArgb(255, 150, 100),
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 8f),
            HorizontalScrollbar = true
        };

        // Fill 먼저, 이후 Top들은 역순으로 추가
        pnlLeft.Controls.Add(lstErrors);
        pnlLeft.Controls.Add(lblErrHeader);
        pnlLeft.Controls.Add(divider);
        pnlLeft.Controls.Add(clbNamespaces);
        pnlLeft.Controls.Add(lblNsHeader);

        // ── Right Panel ──────────────────────────────────────────────────
        pnlRight = new Panel
        {
            Dock = DockStyle.Right,
            Width = 230,
            BackColor = Color.FromArgb(37, 37, 38)
        };

        var lblInfoHeader = MakeSectionHeader("CLASS INFO");

        var pnlInfoContent = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            BackColor = Color.FromArgb(45, 45, 48),
            Padding = new Padding(10, 12, 6, 0),
            WrapContents = false,
            AutoScroll = true
        };

        lblClassName    = MakeInfoLabel("(노드를 클릭하세요)", bold: true);
        lblClassKind    = MakeInfoLabel("");
        lblClassNs      = MakeInfoLabel("");
        lblClassFields  = MakeInfoLabel("");
        lblClassMethods = MakeInfoLabel("");
        lblClassFile    = MakeInfoLabel("");

        pnlInfoContent.Controls.AddRange(new Control[]
        {
            lblClassName, lblClassKind, lblClassNs,
            lblClassFields, lblClassMethods, lblClassFile
        });

        pnlRight.Controls.Add(pnlInfoContent);
        pnlRight.Controls.Add(lblInfoHeader);

        // ── Center Graph Panel ───────────────────────────────────────────
        pnlGraph = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(30, 30, 30)
        };

        pnlLegend = new Panel
        {
            Size = new Size(160, 140),
            BackColor = Color.FromArgb(45, 45, 48),
            BorderStyle = BorderStyle.None,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Visible = false
        };
        pnlLegend.Paint += pnlLegend_Paint;
        pnlGraph.Controls.Add(pnlLegend);

        // ── Form Controls (순서 중요: Fill이 마지막) ─────────────────────
        Controls.Add(pnlGraph);     // Fill — 나머지 공간 전부
        Controls.Add(pnlRight);     // Right
        Controls.Add(pnlLeft);      // Left
        Controls.Add(statusStrip);  // Bottom
        Controls.Add(toolStrip);    // Top
    }

    private static Label MakeSectionHeader(string text) => new Label
    {
        Text = text,
        Dock = DockStyle.Top,
        Height = 28,
        BackColor = Color.FromArgb(0, 122, 204),
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 8f, FontStyle.Bold),
        TextAlign = System.Drawing.ContentAlignment.MiddleCenter
    };

    private static Label MakeInfoLabel(string text, bool bold = false) => new Label
    {
        Text = text,
        AutoSize = false,
        Width = 195,
        Height = bold ? 22 : 20,
        ForeColor = Color.FromArgb(204, 204, 204),
        BackColor = Color.Transparent,
        Font = new Font("Segoe UI", bold ? 9f : 8.5f, bold ? FontStyle.Bold : FontStyle.Regular),
        Padding = new Padding(0, 2, 0, 0)
    };

    private Panel pnlLeft;
    private Panel pnlRight;
    private Panel pnlGraph;
    private Panel pnlLegend;
    private CheckedListBox clbNamespaces;
    private ListBox lstErrors;
    private Label lblClassName;
    private Label lblClassKind;
    private Label lblClassNs;
    private Label lblClassFields;
    private Label lblClassMethods;
    private Label lblClassFile;
    private ToolStripStatusLabel lblStatus;
    private ToolStripStatusLabel lblError;
    private ToolStripStatusLabel lblFolderPath;
}
