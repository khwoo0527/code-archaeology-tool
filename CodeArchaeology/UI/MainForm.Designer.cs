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
        lblStatus = new ToolStripStatusLabel("준비");
        statusStrip.Items.Add(lblStatus);

        Controls.Add(pnlGraph);
        Controls.Add(toolStrip);
        Controls.Add(statusStrip);
    }

    private Panel pnlGraph;
    private ToolStripStatusLabel lblStatus;
}
