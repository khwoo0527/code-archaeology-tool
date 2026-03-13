namespace CodeArchaeology.UI;

/// <summary>
/// VS Code / 모던 IDE 스타일의 다크 플랫 ToolStrip 렌더러.
/// 기본 WinForms 그라디언트/3D 효과를 완전히 제거하고 단색 플랫 스타일을 적용한다.
/// </summary>
internal class DarkToolStripRenderer : ToolStripSystemRenderer
{
    private static readonly Color BackColor   = Color.FromArgb(37, 37, 38);
    private static readonly Color HoverColor  = Color.FromArgb(62, 62, 64);
    private static readonly Color ActiveColor = Color.FromArgb(0, 122, 204);
    private static readonly Color ForeColor   = Color.FromArgb(204, 204, 204);
    private static readonly Color SepColor    = Color.FromArgb(60, 60, 60);

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        => e.Graphics.FillRectangle(new SolidBrush(BackColor), e.AffectedBounds);

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        // 하단에만 얇은 구분선
        var r = e.AffectedBounds;
        e.Graphics.DrawLine(new Pen(SepColor), r.Left, r.Bottom - 1, r.Right, r.Bottom - 1);
    }

    protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
    {
        var rect = new Rectangle(Point.Empty, e.Item.Size);
        if (e.Item.Pressed)
            e.Graphics.FillRectangle(new SolidBrush(ActiveColor), rect);
        else if (e.Item.Selected)
            e.Graphics.FillRectangle(new SolidBrush(HoverColor), rect);
        // 기본 상태: 배경 없음 (완전 플랫)
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = e.Item.Enabled ? ForeColor : Color.FromArgb(100, 100, 100);
        base.OnRenderItemText(e);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        var x = e.Item.Bounds.Width / 2;
        e.Graphics.DrawLine(new Pen(SepColor), x, 4, x, e.Item.Bounds.Height - 4);
    }
}

/// <summary>
/// StatusStrip 전용 컬러 테이블 — 파란 하단 바 배경을 단색으로 고정한다.
/// </summary>
internal class BlueStatusColorTable : ProfessionalColorTable
{
    private static readonly Color Blue = Color.FromArgb(0, 122, 204);

    public override Color StatusStripGradientBegin => Blue;
    public override Color StatusStripGradientEnd   => Blue;
}
