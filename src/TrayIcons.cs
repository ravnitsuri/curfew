using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

sealed class OwnedIcon : IDisposable
{
    [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr handle);

    public readonly Icon Icon;
    readonly IntPtr handle;

    public OwnedIcon(Bitmap source)
    {
        handle = source.GetHicon();
        source.Dispose();
        Icon = Icon.FromHandle(handle);
    }

    public void Dispose()
    {
        Icon.Dispose();
        if (handle != IntPtr.Zero) DestroyIcon(handle);
    }
}

static class TrayIcons
{
    static readonly Color RingTrack = Color.FromArgb(86, 90, 102);

    static int Size
    {
        get { return Math.Max(16, SystemInformation.SmallIconSize.Width); }
    }

    static Bitmap NewCanvas(int size, out Graphics g)
    {
        Bitmap bitmap = new Bitmap(size, size);
        g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);
        return bitmap;
    }

    public static OwnedIcon Moon()
    {
        int size = Size;
        Graphics g;
        Bitmap bitmap = NewCanvas(size, out g);
        using (g)
        using (GraphicsPath crescent = CrescentPath(size * 0.5f, size * 0.5f, size * 0.46f))
        using (SolidBrush brush = new SolidBrush(Theme.Accent))
            g.FillPath(brush, crescent);
        return new OwnedIcon(bitmap);
    }

    public static OwnedIcon Countdown(float remaining)
    {
        int size = Size;
        float thickness = size * 0.22f;
        Graphics g;
        Bitmap bitmap = NewCanvas(size, out g);
        using (g)
        {
            RectangleF circle = new RectangleF(thickness / 2f + 0.5f, thickness / 2f + 0.5f,
                size - thickness - 1, size - thickness - 1);

            using (Pen track = new Pen(RingTrack, thickness))
                g.DrawArc(track, circle, 0, 360);

            if (remaining > 0.01f)
                using (Pen arc = new Pen(Theme.Accent, thickness))
                {
                    arc.StartCap = LineCap.Round;
                    arc.EndCap = LineCap.Round;
                    g.DrawArc(arc, circle, -90, 360f * Math.Min(remaining, 1f));
                }
        }
        return new OwnedIcon(bitmap);
    }

    static GraphicsPath CrescentPath(float centreX, float centreY, float radius)
    {
        float cutRadius = radius * 1.24f;
        float cutOffset = radius * 0.96f;

        GraphicsPath path = new GraphicsPath();
        path.AddArc(centreX - radius, centreY - radius, radius * 2, radius * 2, 78.5f, 203f);
        path.AddArc(centreX + cutOffset - cutRadius, centreY - cutRadius,
            cutRadius * 2, cutRadius * 2, 232.2f, -104.4f);
        path.CloseFigure();

        using (Matrix tilt = new Matrix())
        {
            tilt.RotateAt(-35f, new PointF(centreX, centreY));
            path.Transform(tilt);
        }
        return path;
    }
}
