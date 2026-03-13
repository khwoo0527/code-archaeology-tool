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
        InitImpactButton();
        nodeToolTip = new ToolTip(components)
        {
            AutoPopDelay = 8000,
            InitialDelay = 0,
            ReshowDelay  = 0,
            ShowAlways   = true,
            OwnerDraw    = true
        };
        nodeToolTip.Draw   += nodeToolTip_Draw;
        nodeToolTip.Popup  += nodeToolTip_Popup;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize    = new Size(1400, 860);
        Text          = "Code Archaeology";
        BackColor     = Color.FromArgb(30, 30, 30);
        Font          = new Font("Segoe UI", 9f);
        KeyPreview    = true;

        // ── ToolStrip ────────────────────────────────────────────────────
        var toolStrip = new ToolStrip
        {
            Renderer = new DarkToolStripRenderer(),
            GripStyle = ToolStripGripStyle.Hidden,
            Padding   = new Padding(4, 0, 4, 0),
            ImageScalingSize = new Size(16, 16)
        };

        var btnOpenFolder = new ToolStripButton("  📂  폴더 열기  ")
        {
            ForeColor   = Color.FromArgb(210, 210, 210),
            Font        = new Font("Segoe UI", 9f),
            Padding     = new Padding(6, 0, 6, 0)
        };
        var btnRefresh = new ToolStripButton("  🔄  새로고침  ")
        {
            ForeColor = Color.FromArgb(210, 210, 210),
            Font      = new Font("Segoe UI", 9f),
            Padding   = new Padding(6, 0, 6, 0)
        };
        var btnExportPng = new ToolStripButton("  💾  PNG 내보내기  ")
        {
            ForeColor = Color.FromArgb(210, 210, 210),
            Font      = new Font("Segoe UI", 9f),
            Padding   = new Padding(6, 0, 6, 0)
        };
        btnOpenFolder.Click += btnOpenFolder_Click;
        btnRefresh.Click    += btnRefresh_Click;
        btnExportPng.Click  += btnExportPng_Click;

        var lblSearch = new ToolStripLabel("🔍")
        {
            ForeColor = Color.FromArgb(160, 160, 170),
            Font      = new Font("Segoe UI", 10f),
            Padding   = new Padding(0, 0, 2, 0),
            Alignment = ToolStripItemAlignment.Right
        };
        txtSearch = new ToolStripTextBox
        {
            Width       = 260,
            BackColor   = Color.FromArgb(50, 50, 55),
            ForeColor   = Color.FromArgb(220, 220, 220),
            BorderStyle = BorderStyle.FixedSingle,
            Font        = new Font("Segoe UI", 9.5f),
            Alignment   = ToolStripItemAlignment.Right
        };
        txtSearch.TextChanged += txtSearch_TextChanged;

        toolStrip.Items.AddRange(new ToolStripItem[]
        {
            btnOpenFolder,
            new ToolStripSeparator(),
            btnRefresh,
            new ToolStripSeparator(),
            btnExportPng,
            lblSearch, txtSearch
        });

        // ── StatusStrip ──────────────────────────────────────────────────
        var statusStrip = new StatusStrip
        {
            BackColor = Color.FromArgb(0, 122, 204),
            Renderer  = new ToolStripProfessionalRenderer(new BlueStatusColorTable())
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

        // ── SplitContainer 외부 (Left | Rest) ────────────────────────────
        splitOuter = new SplitContainer
        {
            Dock          = DockStyle.Fill,
            Orientation   = Orientation.Vertical,
            SplitterWidth = 4,
            BackColor     = Color.FromArgb(50, 50, 54)
        };
        splitOuter.Panel1.BackColor = Color.FromArgb(37, 37, 38);
        splitOuter.Panel2.BackColor = Color.FromArgb(30, 30, 30);

        // ── Left 패널 — splitLeft (Namespace Filter | Error Log) ─────────
        splitLeft = new SplitContainer
        {
            Dock          = DockStyle.Fill,
            Orientation   = Orientation.Horizontal,
            SplitterWidth = 4,
            BackColor     = Color.FromArgb(50, 50, 54)
        };
        splitLeft.Panel1.BackColor = Color.FromArgb(37, 37, 38);
        splitLeft.Panel2.BackColor = Color.FromArgb(37, 37, 38);

        // splitLeft.Panel1 — Namespace Filter
        var lblNsHeader = MakeSectionHeader("NAMESPACE FILTER");

        chkAllNamespaces = new CheckBox
        {
            Dock      = DockStyle.Top,
            Height    = 26,
            Text      = "All Namespaces",
            Checked   = true,
            ForeColor = Color.FromArgb(204, 204, 204),
            BackColor = Color.FromArgb(50, 50, 53),
            Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            Padding   = new Padding(8, 0, 0, 0)
        };
        chkAllNamespaces.CheckedChanged += chkAllNamespaces_CheckedChanged;

        clbNamespaces = new CheckedListBox
        {
            Dock         = DockStyle.Fill,
            BackColor    = Color.FromArgb(45, 45, 48),
            ForeColor    = Color.FromArgb(204, 204, 204),
            BorderStyle  = BorderStyle.None,
            CheckOnClick = true,
            Font         = new Font("Segoe UI", 8.5f),
            ItemHeight   = 22
        };
        clbNamespaces.ItemCheck += clbNamespaces_ItemCheck;

        splitLeft.Panel1.Controls.Add(clbNamespaces);
        splitLeft.Panel1.Controls.Add(chkAllNamespaces);
        splitLeft.Panel1.Controls.Add(lblNsHeader);

        // splitLeft.Panel2 — Error Log
        var lblErrHeader = MakeSectionHeader("ERROR LOG");

        lstErrors = new ListBox
        {
            Dock        = DockStyle.Fill,
            BackColor   = Color.FromArgb(45, 45, 48),
            BorderStyle = BorderStyle.None,
            Font        = new Font("Segoe UI", 8f),
            ItemHeight  = 24,
            DrawMode    = DrawMode.OwnerDrawFixed
        };
        lstErrors.DrawItem += lstErrors_DrawItem;

        splitLeft.Panel2.Controls.Add(lstErrors);
        splitLeft.Panel2.Controls.Add(lblErrHeader);

        splitOuter.Panel1.Controls.Add(splitLeft);

        // ── SplitContainer 내부 (Graph | Right) ──────────────────────────
        splitInner = new SplitContainer
        {
            Dock          = DockStyle.Fill,
            Orientation   = Orientation.Vertical,
            SplitterWidth = 4,
            BackColor     = Color.FromArgb(50, 50, 54)
        };
        splitInner.Panel1.BackColor = Color.FromArgb(30, 30, 30);
        splitInner.Panel2.BackColor = Color.FromArgb(37, 37, 38);

        // ── Graph 패널 (splitInner.Panel1) ───────────────────────────────
        pnlGraph = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.FromArgb(30, 30, 30)
        };

        pnlLegend = new Panel
        {
            Size      = new Size(164, 26 + 218),  // 기본 펼침
            BackColor = Color.FromArgb(45, 45, 48),
            Anchor    = AnchorStyles.Top | AnchorStyles.Right,
            Visible   = false
        };

        lblLegendHeader = new Label
        {
            Dock      = DockStyle.Top,
            Height    = 26,
            Text      = "범례  ▼",
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Cursor    = Cursors.Hand
        };
        lblLegendHeader.Click += lblLegendHeader_Click;

        pnlLegendContent = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 48),
            Visible   = true
        };
        pnlLegendContent.Paint += pnlLegendContent_Paint;

        pnlLegend.Controls.Add(pnlLegendContent);
        pnlLegend.Controls.Add(lblLegendHeader);
        pnlGraph.Controls.Add(pnlLegend);

        btnPanMode = new Button
        {
            Size      = new Size(64, 26),
            Text      = "✋ 이동",
            BackColor = Color.FromArgb(50, 50, 54),
            ForeColor = Color.FromArgb(180, 180, 190),
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 8.5f),
            Cursor    = Cursors.Default,
            Visible   = false
        };
        btnPanMode.FlatAppearance.BorderColor    = Color.FromArgb(75, 75, 85);
        btnPanMode.FlatAppearance.CheckedBackColor = Color.FromArgb(0, 100, 180);
        btnPanMode.Click += btnPanMode_Click;
        pnlGraph.Controls.Add(btnPanMode);

        splitInner.Panel1.Controls.Add(pnlGraph);

        // ── Right 패널 — splitRight (Class Info | Dependency Metrics) ────
        splitRight = new SplitContainer
        {
            Dock          = DockStyle.Fill,
            Orientation   = Orientation.Horizontal,
            SplitterWidth = 4,
            BackColor     = Color.FromArgb(50, 50, 54)
        };
        splitRight.Panel1.BackColor = Color.FromArgb(37, 37, 38);
        splitRight.Panel2.BackColor = Color.FromArgb(37, 37, 38);

        // splitRight.Panel1 — Class Info
        var lblClassInfoHeader = MakeSectionHeader("CLASS INFO");

        var pnlClassInfoBody = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 48),
            Padding   = new Padding(12, 10, 10, 8)
        };

        lblInfoName = new Label
        {
            Dock      = DockStyle.Top,
            Height    = 28,
            ForeColor = Color.FromArgb(220, 220, 220),
            Font      = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            BackColor = Color.Transparent,
            Text      = "—"
        };

        var (rowKind,    lblKindVal)    = MakeInfoRow("Kind");
        var (rowNs,      lblNsVal)      = MakeInfoRow("Namespace");
        var (rowFile,    lblFileVal)    = MakeInfoRow("File");
        var (rowFields,  lblFieldsVal)  = MakeExpandableInfoRow("Fields",    out rowFieldsPanel);
        var (rowMethods, lblMethodsVal) = MakeExpandableInfoRow("Methods",   out rowMethodsPanel);
        var (rowDeps,    lblDepsVal)    = MakeInfoRow("Dependencies");

        lblInfoKindVal    = lblKindVal;
        lblInfoNsVal      = lblNsVal;
        lblInfoFileVal    = lblFileVal;
        lblInfoFieldsVal  = lblFieldsVal;
        lblInfoMethodsVal = lblMethodsVal;
        lblInfoDepsVal    = lblDepsVal;

        rowFieldsPanel.Click  += rowFieldsPanel_Click;
        rowMethodsPanel.Click += rowMethodsPanel_Click;
        foreach (Control c in rowFieldsPanel.Controls)  c.Click += rowFieldsPanel_Click;
        foreach (Control c in rowMethodsPanel.Controls) c.Click += rowMethodsPanel_Click;

        pnlFieldsDetail = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 0,
            BackColor = Color.FromArgb(40, 40, 44),
            Padding   = new Padding(20, 2, 8, 2),
            Visible   = false
        };
        pnlMethodsDetail = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 0,
            BackColor = Color.FromArgb(40, 40, 44),
            Padding   = new Padding(20, 2, 8, 2),
            Visible   = false
        };

        pnlClassInfoBody.Controls.Add(rowDeps);
        pnlClassInfoBody.Controls.Add(pnlMethodsDetail);
        pnlClassInfoBody.Controls.Add(rowMethods);
        pnlClassInfoBody.Controls.Add(pnlFieldsDetail);
        pnlClassInfoBody.Controls.Add(rowFields);
        pnlClassInfoBody.Controls.Add(rowFile);
        pnlClassInfoBody.Controls.Add(rowNs);
        pnlClassInfoBody.Controls.Add(rowKind);
        pnlClassInfoBody.Controls.Add(lblInfoName);

        splitRight.Panel1.Controls.Add(pnlClassInfoBody);
        splitRight.Panel1.Controls.Add(lblClassInfoHeader);

        // splitRight.Panel2 — Dependency Metrics
        var lblMetricsHeader = MakeSectionHeader("DEPENDENCY METRICS");

        var pnlMetricsBody = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 48),
            Padding   = new Padding(12, 10, 10, 8)
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

        splitRight.Panel2.Controls.Add(pnlMetricsBody);
        splitRight.Panel2.Controls.Add(lblMetricsHeader);

        splitInner.Panel2.Controls.Add(splitRight);
        splitOuter.Panel2.Controls.Add(splitInner);

        // ── Form Controls ────────────────────────────────────────────────
        Controls.Add(splitOuter);
        Controls.Add(statusStrip);
        Controls.Add(toolStrip);
    }

    private static Label MakeSectionHeader(string text) => new Label
    {
        Text      = text,
        Dock      = DockStyle.Top,
        Height    = 28,
        BackColor = Color.FromArgb(0, 122, 204),
        ForeColor = Color.White,
        Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
        TextAlign = System.Drawing.ContentAlignment.MiddleCenter
    };

    private static (Panel row, Label valLabel) MakeExpandableInfoRow(string key, out Panel rowPanel)
    {
        var (row, val) = MakeInfoRow(key);
        row.Cursor = Cursors.Hand;
        rowPanel = row;
        return (row, val);
    }

    private static (Panel row, Label valLabel) MakeInfoRow(string key)
    {
        var row = new Panel { Dock = DockStyle.Top, Height = 22, BackColor = Color.Transparent };
        row.Controls.Add(new Label
        {
            Text      = key + ":",
            Left = 0, Top = 3, Width = 90, Height = 18,
            ForeColor = Color.FromArgb(120, 120, 130),
            Font      = new Font("Segoe UI", 8.5f),
            BackColor = Color.Transparent
        });
        var val = new Label
        {
            Text      = "—",
            Left = 92, Top = 3, Width = 126, Height = 18,
            ForeColor = Color.FromArgb(200, 200, 210),
            Font      = new Font("Segoe UI", 8.5f),
            BackColor = Color.Transparent
        };
        row.Controls.Add(val);
        return (row, val);
    }

    private static (Panel row, Label valLabel) MakeMetricRow(string key)
    {
        var row = new Panel { Dock = DockStyle.Top, Height = 26, BackColor = Color.Transparent };
        row.Controls.Add(new Label
        {
            Text      = key + ":",
            Left = 0, Top = 4, Width = 130, Height = 18,
            ForeColor = Color.FromArgb(120, 120, 130),
            Font      = new Font("Segoe UI", 8.5f),
            BackColor = Color.Transparent
        });
        var val = new Label
        {
            Text      = "—",
            Left = 132, Top = 4, Width = 76, Height = 18,
            ForeColor = Color.FromArgb(220, 220, 220),
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            BackColor = Color.Transparent,
            TextAlign = System.Drawing.ContentAlignment.MiddleRight
        };
        row.Controls.Add(val);
        return (row, val);
    }

    private void InitImpactButton()
    {
        btnImpact = new Button
        {
            Text      = "🔍 영향 분석",
            AutoSize  = true,
            BackColor = Color.FromArgb(55, 55, 60),
            ForeColor = Color.FromArgb(210, 210, 210),
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9f),
            Cursor    = Cursors.Hand,
            Enabled   = false
        };
        btnImpact.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 90);
        btnImpact.Click += btnImpact_Click;
    }

    private SplitContainer splitOuter;
    private SplitContainer splitInner;
    private SplitContainer splitLeft;
    private SplitContainer splitRight;
    private Panel pnlGraph;
    private Panel pnlLegend;
    private Panel pnlLegendContent;
    private Label lblLegendHeader;
    private Panel pnlFieldsDetail;
    private Panel pnlMethodsDetail;
    private Panel rowFieldsPanel;
    private Panel rowMethodsPanel;
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
    private ToolStripTextBox txtSearch;
    private Button btnPanMode;
    private Button btnImpact;
    private ToolTip nodeToolTip;
}
