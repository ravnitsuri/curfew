using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

enum PillGlyph { None, Minimize, Close }

class PillButton : Control
{
    public Color Fill = Theme.Track;
    public Color FillHover = Theme.TrackHover;
    public Color Foreground = Theme.Text;
    public Color ForegroundHover = Color.Empty;
    public PillGlyph Glyph = PillGlyph.None;
    public int CornerRadius = 10;
    public bool Selected;

    bool hovered;

    public PillButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Cursor = Cursors.Hand;
        MouseEnter += delegate { hovered = true; Invalidate(); };
        MouseLeave += delegate { hovered = false; Invalidate(); };
    }

    Color CurrentFill
    {
        get
        {
            if (!Enabled) return Theme.Surface;
            if (hovered || Selected) return FillHover;
            return Fill;
        }
    }

    Color CurrentForeground
    {
        get
        {
            if (!Enabled) return Theme.MutedText;
            if ((hovered || Selected) && ForegroundHover != Color.Empty) return ForegroundHover;
            return Foreground;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent.BackColor);

        Rectangle body = new Rectangle(0, 0, Width - 1, Height - 1);
        using (GraphicsPath path = Theme.RoundedRectangle(body, CornerRadius))
        using (SolidBrush brush = new SolidBrush(CurrentFill))
            g.FillPath(brush, path);

        if (Selected && Enabled)
            using (GraphicsPath path = Theme.RoundedRectangle(body, CornerRadius))
            using (Pen pen = new Pen(Theme.Accent, 1.5f))
                g.DrawPath(pen, path);

        if (Glyph == PillGlyph.None)
            DrawLabel(g);
        else
            DrawGlyph(g);
    }

    void DrawLabel(Graphics g)
    {
        Rectangle area = ClientRectangle;
        area.Offset(0, -1);
        TextRenderer.DrawText(g, Text, Font, area, CurrentForeground,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    void DrawGlyph(Graphics g)
    {
        float x = (Width - 1) / 2f;
        float y = (Height - 1) / 2f;
        using (Pen pen = new Pen(CurrentForeground, 1.6f))
        {
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            if (Glyph == PillGlyph.Minimize)
            {
                g.DrawLine(pen, x - 5f, y, x + 5f, y);
            }
            else
            {
                g.DrawLine(pen, x - 4.5f, y - 4.5f, x + 4.5f, y + 4.5f);
                g.DrawLine(pen, x + 4.5f, y - 4.5f, x - 4.5f, y + 4.5f);
            }
        }
    }
}
