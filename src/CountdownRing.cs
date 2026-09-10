using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

class CountdownRing : Control
{
    const float ArcThickness = 13f;
    const float StartAngle = -90f;

    public float Progress = 1f;
    public string Headline = "30";
    public string Caption = "minutes";
    public float HeadlineSize = 38f;

    Font headlineFont;

    public CountdownRing()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    Font HeadlineFont
    {
        get
        {
            if (headlineFont == null || headlineFont.Size != HeadlineSize)
            {
                if (headlineFont != null) headlineFont.Dispose();
                headlineFont = new Font("Segoe UI", HeadlineSize);
            }
            return headlineFont;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.Clear(Parent.BackColor);

        DrawArcs(g);
        DrawText(g);
    }

    void DrawArcs(Graphics g)
    {
        float inset = ArcThickness / 2 + 1;
        RectangleF circle = new RectangleF(inset, inset,
            Width - ArcThickness - 2, Height - ArcThickness - 2);

        using (Pen track = new Pen(Theme.Track, ArcThickness))
            g.DrawArc(track, circle, 0, 360);

        if (Progress <= 0.002f) return;

        using (Pen arc = new Pen(Theme.Accent, ArcThickness))
        {
            arc.StartCap = LineCap.Round;
            arc.EndCap = LineCap.Round;
            g.DrawArc(arc, circle, StartAngle, 360f * Math.Min(Progress, 1f));
        }
    }

    void DrawText(Graphics g)
    {
        using (StringFormat format = new StringFormat())
        {
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            format.FormatFlags = StringFormatFlags.NoClip;
            format.Trimming = StringTrimming.None;

            RectangleF headlineArea = new RectangleF(0, Height * 0.22f, Width, Height * 0.42f);
            using (SolidBrush brush = new SolidBrush(Theme.Text))
                g.DrawString(Headline, HeadlineFont, brush, headlineArea, format);

            RectangleF captionArea = new RectangleF(0, Height * 0.60f, Width, 22);
            using (SolidBrush brush = new SolidBrush(Theme.MutedText))
                g.DrawString(Caption, Theme.RingUnitFont, brush, captionArea, format);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && headlineFont != null) headlineFont.Dispose();
        base.Dispose(disposing);
    }
}
