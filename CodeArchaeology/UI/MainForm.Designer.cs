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

        var lblNsHeader = MakeSectionHeader("NAMESPACE FILTER");

        chkAllNamespaces = new CheckBox
        {
            Dock = DockStyle.Top,
            Height = 26,
            Text = "All Namespaces",
            Checked = true,
            ForeColor = Color.FromArgb(204, 204, 204),
            BackColor = Color.FromArgb(50, 50, 53),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            Padding = new Padding(8, 0, 0, 0)
        };
        chkAllNamespaces.CheckedChanged += chkAllNamespaces_CheckedChanged;

        clbNamespaces = new CheckedListBox
        {
            Dock = DockStyle.Top,
            Height = 200,
            BackColor = Color.FromArgb(45, 45, 48),
            ForeColor = Color.FromArgb(204, 204, 204),
            BorderStyle = BorderStyle.None,
            CheckOnClick = true,
            Font = new Font("Segoe UI", 8.5f),
            ItemHeight = 22
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
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 8f),
            ItemHeight = 24,
            DrawMode = DrawMode.OwnerDrawFixed
        };
        lstErrors.DrawItem += lstErrors_DrawItem;

        // Fill 먼저, 이후 Top들은 역순으로 추가
        pnlLeft.Controls.Add(lstErrors);
        pnlLeft.Controls.Add(lblErrHeader);
        pnlLeft.Controls.Add(divider);
        pnlLeft.Controls.Add(clbNamespaces);
        pnlLeft.Controls.Add(chkAllNamespaces);
        pnlLeft.Controls.Add(lblNsHeader);

        // ── Right Panel ──────────────────────────────────────────────────
        pnlRight = new Panel
        {
            Dock = DockStyle.Right,
            Width = 240,
            BackColor = Color.FromArgb(37, 37, 38)
        };

        // CLASS INFO 카드
        var lblClassInfoHeader = MakeSectionHeader("CLASS INFO");

        var pnlClassInfoBody = new Panel
        {
            Dock = DockStyle.Top,
            Height = 210,
            BackColor = Color.FromArgb(45, 45, 48),
            Padding = new Padding(12, 10, 10, 8)
        };

        lblInfoName = new Label
        {
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = Color.FromArgb(220, 220, 220),
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            BackColor = Color.Transparent,
            Text = "—"
        };

        var (rowKind,    lblKindVal)    = MakeInfoRow("Kind");
        var (rowNs,      lblNsVal)      = MakeInfoRow("Namespace");
        var (rowFile,    lblFileVal)    = MakeInfoRow("File");
        var (rowFields,  lblFieldsVal)  = MakeInfoRow("Fields");
        var (rowMethods, lblMethodsVal) = MakeInfoRow("Methods");
        var (rowDeps,    lblDepsVal)    = MakeInfoRow("Dependencies");

        lblInfoKindVal    = lblKindVal;
        lblInfoNsVal      = lblNsVal;
        lblInfoFileVal    = lblFileVal;
        lblInfoFieldsVal  = lblFieldsVal;
        lblInfoMethodsVal = lblMethodsVal;
        lblInfoDepsVal    = lblDepsVal;

        // Top은 역순 추가 (마지막 추가 = 최상단)
        pnlClassInfoBody.Controls.Add(rowDeps);
        pnlClassInfoBody.Controls.Add(rowMethods);
        pnlClassInfoBody.Controls.Add(rowFields);
        pnlClassInfoBody.Controls.Add(rowFile);
        pnlClassInfoBody.Controls.Add(rowNs);
        pnlClassInfoBody.Controls.Add(rowKind);
        pnlClassInfoBody.Controls.Add(lblInfoName);

        // DEPENDENCY METRICS 카드
        var dividerRight = new Label
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = Color.FromArgb(60, 60, 65)
        };

        var lblMetricsHeader = MakeSectionHeader("DEPENDENCY METRICS");

        var pnlMetricsBody = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 48),
            Padding = new Padding(12, 10, 10, 8)
        };

        var (rowCa,   lblCaVal)   = MakeMetricRow("Afferent Coupling");
        var (rowCe,   lblCeVal)   = MakeMetricRow("Efferent Coupling");
        var (rowInst, lblInstVal) = MakeMetricRow("Instability");

        lblMetricCaVal   = lblCaVal;
        lblMetricCeVal   = lblCeVal;
        lblMetricInstVal = lblInstVal;

        pnlMetricsBody.Controls.Add(rowInst);
        pnlMetricsBody.Controls.Add(rowCe);
        pnlMetricsBody.Controls.Add(rowCa);

        // Fill 먼저, 이후 Top 역순
        pnlRight.Controls.Add(pnlMetricsBody);
        pnlRight.Controls.Add(lblMetricsHeader);
        pnlRight.Controls.Add(dividerRight);
        pnlRight.Controls.Add(pnlClassInfoBody);
        pnlRight.Controls.Add(lblClassInfoHeader);

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

    /// <summary>키(회색) + 값(밝은 흰색) 두 레이블로 구성된 정보 행을 생성한다.</summary>
    private static (Panel row, Label valLabel) MakeInfoRow(string key)
    {
        var row = new Panel { Dock = DockStyle.Top, Height = 22, BackColor = Color.Transparent };
        row.Controls.Add(new Label
        {
            Text = key + ":",
            Left = 0, Top = 3, Width = 90, Height = 18,
            ForeColor = Color.FromArgb(120, 120, 130),
            Font = new Font("Segoe UI", 8.5f),
            BackColor = Color.Transparent
        });
        var val = new Label
        {
            Text = "—",
            Left = 92, Top = 3, Width = 126, Height = 18,
            ForeColor = Color.FromArgb(200, 200, 210),
            Font = new Font("Segoe UI", 8.5f),
            BackColor = Color.Transparent
        };
        row.Controls.Add(val);
        return (row, val);
    }

    /// <summary>메트릭 행 — 키(회색) + 값(굵은 흰색).</summary>
    private static (Panel row, Label valLabel) MakeMetricRow(string key)
    {
        var row = new Panel { Dock = DockStyle.Top, Height = 26, BackColor = Color.Transparent };
        row.Controls.Add(new Label
        {
            Text = key + ":",
            Left = 0, Top = 4, Width = 130, Height = 18,
            ForeColor = Color.FromArgb(120, 120, 130),
            Font = new Font("Segoe UI", 8.5f),
            BackColor = Color.Transparent
        });
        var val = new Label
        {
            Text = "—",
            Left = 132, Top = 4, Width = 76, Height = 18,
            ForeColor = Color.FromArgb(220, 220, 220),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            BackColor = Color.Transparent,
            TextAlign = System.Drawing.ContentAlignment.MiddleRight
        };
        row.Controls.Add(val);
        return (row, val);
    }

    private Panel pnlLeft;
    private Panel pnlRight;
    private Panel pnlGraph;
    private Panel pnlLegend;
    private CheckBox chkAllNamespaces;
    private CheckedListBox clbNamespaces;
    private ListBox lstErrors;
    // Class Info
    private Label lblInfoName;
    private Label lblInfoKindVal;
    private Label lblInfoNsVal;
    private Label lblInfoFileVal;
    private Label lblInfoFieldsVal;
    private Label lblInfoMethodsVal;
    private Label lblInfoDepsVal;
    // Dependency Metrics
    private Label lblMetricCaVal;
    private Label lblMetricCeVal;
    private Label lblMetricInstVal;
    private ToolStripStatusLabel lblStatus;
    private ToolStripStatusLabel lblError;
    private ToolStripStatusLabel lblFolderPath;
}
