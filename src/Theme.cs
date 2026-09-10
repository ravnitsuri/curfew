using System;
using System.Drawing;
using System.Drawing.Drawing2D;

static class Theme
{
    public static readonly Color Background = Color.FromArgb(18, 19, 26);
    public static readonly Color Surface = Color.FromArgb(28, 30, 40);
    public static readonly Color Track = Color.FromArgb(42, 45, 60);
    public static readonly Color TrackHover = Color.FromArgb(56, 60, 78);
    public static readonly Color Accent = Color.FromArgb(124, 108, 255);
    public static readonly Color AccentHover = Color.FromArgb(146, 132, 255);
    public static readonly Color Text = Color.FromArgb(238, 239, 245);
    public static readonly Color MutedText = Color.FromArgb(166, 170, 186);

    public static readonly Font WindowFont = new Font("Segoe UI", 9.5f);
    public static readonly Font TitleFont = new Font("Segoe UI", 10f);
    public static readonly Font HintFont = new Font("Segoe UI", 9f);
    public static readonly Font PresetFont = new Font("Segoe UI Semibold", 10f);
    public static readonly Font ActionFont = new Font("Segoe UI Semibold", 11f);
    public static readonly Font RingUnitFont = new Font("Segoe UI", 10f);

    public static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
    {
        int diameter = Math.Max(2, radius * 2);
        GraphicsPath path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
