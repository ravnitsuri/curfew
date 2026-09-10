using System.Drawing;
using System.Windows.Forms;

class Caption : Control
{
    public Color Foreground = Theme.MutedText;
    public bool Centered;

    public Caption()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(Parent.BackColor);

        Rectangle area = ClientRectangle;
        area.Offset(0, -1);
        TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding |
            (Centered ? TextFormatFlags.HorizontalCenter : TextFormatFlags.Left);
        TextRenderer.DrawText(g, Text, Font, area, Foreground, flags);
    }
}
