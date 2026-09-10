using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

class Slider : Control
{
    const int EndPadding = 11;
    const int TrackThickness = 6;
    const int KnobRadius = 9;
    const int KnobRadiusHover = 10;

    public int Minimum = 5;
    public int Maximum = 180;
    public int StepSize = 5;
    public event EventHandler ValueChanged;

    int value = 30;
    bool dragging;
    bool hovered;

    public Slider()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Cursor = Cursors.Hand;
        MouseEnter += delegate { hovered = true; Invalidate(); };
        MouseLeave += delegate { hovered = false; Invalidate(); };
    }

    public int Value
    {
        get { return value; }
        set
        {
            int snapped = Minimum + (int)Math.Round((double)(value - Minimum) / StepSize) * StepSize;
            snapped = Math.Max(Minimum, Math.Min(Maximum, snapped));
            if (snapped == this.value) return;
            this.value = snapped;
            Invalidate();
            if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
        }
    }

    int TrackWidth { get { return Math.Max(1, Width - EndPadding * 2); } }

    float FilledFraction { get { return (float)(value - Minimum) / (Maximum - Minimum); } }

    int KnobX { get { return EndPadding + (int)(FilledFraction * TrackWidth); } }

    void SetValueFromPointer(int x)
    {
        float fraction = (float)(x - EndPadding) / TrackWidth;
        Value = Minimum + (int)Math.Round(fraction * (Maximum - Minimum));
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (!Enabled || e.Button != MouseButtons.Left) return;
        dragging = true;
        SetValueFromPointer(e.X);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (dragging) SetValueFromPointer(e.X);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        dragging = false;
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (Enabled) Value = value + (e.Delta > 0 ? StepSize : -StepSize);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent.BackColor);

        int centreY = Height / 2;
        int top = centreY - TrackThickness / 2;

        Rectangle track = new Rectangle(EndPadding, top, TrackWidth, TrackThickness);
        FillRounded(g, track, Theme.Track);

        if (KnobX > EndPadding)
        {
            Rectangle filled = new Rectangle(EndPadding, top, KnobX - EndPadding, TrackThickness);
            FillRounded(g, filled, Enabled ? Theme.Accent : Theme.TrackHover);
        }

        int radius = hovered || dragging ? KnobRadiusHover : KnobRadius;
        Rectangle knob = new Rectangle(KnobX - radius, centreY - radius, radius * 2, radius * 2);
        using (SolidBrush brush = new SolidBrush(Enabled ? Theme.Text : Theme.TrackHover))
            g.FillEllipse(brush, knob);
        if (Enabled)
            using (Pen pen = new Pen(Theme.Accent, 3f))
                g.DrawEllipse(pen, knob);
    }

    static void FillRounded(Graphics g, Rectangle bounds, Color colour)
    {
        using (GraphicsPath path = Theme.RoundedRectangle(bounds, bounds.Height / 2))
        using (SolidBrush brush = new SolidBrush(colour))
            g.FillPath(brush, path);
    }
}
