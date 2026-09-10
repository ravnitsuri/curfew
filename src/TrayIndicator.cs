using System;
using System.Windows.Forms;

sealed class TrayIndicator : IDisposable
{
    readonly NotifyIcon notify = new NotifyIcon();
    OwnedIcon current;

    public NotifyIcon Notify { get { return notify; } }

    public string Tooltip
    {
        set { notify.Text = value; }
    }

    public bool Visible
    {
        set { notify.Visible = value; }
    }

    public void ShowIdle()
    {
        Swap(TrayIcons.Moon());
        notify.Text = MainWindow.AppName;
    }

    public void ShowCountdown(float remaining)
    {
        Swap(TrayIcons.Countdown(remaining));
    }

    public void Balloon(string message)
    {
        notify.ShowBalloonTip(3000, MainWindow.AppName, message, ToolTipIcon.Info);
    }

    void Swap(OwnedIcon next)
    {
        notify.Icon = next.Icon;
        if (current != null) current.Dispose();
        current = next;
    }

    public void Dispose()
    {
        notify.Visible = false;
        notify.Dispose();
        if (current != null) current.Dispose();
    }
}
