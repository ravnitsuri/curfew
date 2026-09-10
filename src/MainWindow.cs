using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

class MainWindow : Form
{
    public const string AppName = "Curfew";

    const int WindowWidth = 320;
    const int WindowHeight = 462;
    const int EdgeGap = 20;
    const int ContentWidth = WindowWidth - EdgeGap * 2;

    const int HeaderTop = 11;
    const int HeaderButtonWidth = 28;
    const int HeaderButtonHeight = 26;

    const int RingTop = 50;
    const int RingSize = 200;

    const int SliderMargin = 24;
    const int SliderTop = 266;
    const int SliderHeight = 30;

    const int PresetTop = 308;
    const int PresetWidth = 64;
    const int PresetHeight = 34;
    const int PresetGap = 8;

    const int ActionTop = 358;
    const int ActionHeight = 46;

    const int HintTop = 414;
    const int HintHeight = 22;

    const int DwmCornerPreference = 33;
    const int DwmBorderColour = 34;
    const int DwmRoundCorners = 2;
    const int BorderColourBgr = 0x003C2D2A;

    const int WmNcLeftButtonDown = 0xA1;
    const int HitCaption = 2;

    const int TickIntervalMs = 250;

    static readonly int[] PresetMinutes = { 15, 30, 60, 120 };

    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);

    [DllImport("user32.dll")] static extern bool ReleaseCapture();
    [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr window, int message, int wparam, int lparam);
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr window);

    readonly CountdownRing ring = new CountdownRing();
    readonly Slider slider = new Slider();
    readonly PillButton actionButton = new PillButton();
    readonly PillButton[] presetButtons = new PillButton[PresetMinutes.Length];
    readonly TrayIndicator tray = new TrayIndicator();
    readonly Timer ticker = new Timer();

    DateTime deadline;
    int scheduledSeconds;
    int lastShownSecond = -1;
    bool armed;
    bool reallyClosing;

    public MainWindow()
    {
        Text = AppName;
        ClientSize = new Size(WindowWidth, WindowHeight);
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Background;
        ForeColor = Theme.Text;
        Font = Theme.WindowFont;
        DoubleBuffered = true;
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        BuildHeader();
        BuildDial();
        BuildPresets();
        BuildActions();
        BuildTray();

        MouseDown += StartWindowDrag;
        ticker.Interval = TickIntervalMs;
        ticker.Tick += OnTick;
        FormClosing += OnFormClosing;

        ShowSelection();
    }

    void BuildHeader()
    {
        Caption title = new Caption();
        title.Text = AppName;
        title.Foreground = Theme.MutedText;
        title.Font = Theme.TitleFont;
        title.SetBounds(18, HeaderTop, 160, HeaderButtonHeight);
        title.MouseDown += StartWindowDrag;

        PillButton minimise = new PillButton();
        minimise.Glyph = PillGlyph.Minimize;
        minimise.Fill = Theme.Background;
        minimise.Foreground = Theme.MutedText;
        minimise.ForegroundHover = Theme.Text;
        minimise.CornerRadius = 8;
        minimise.SetBounds(WindowWidth - 74, HeaderTop, HeaderButtonWidth, HeaderButtonHeight);
        minimise.Click += delegate { HideToTray(); };

        PillButton close = new PillButton();
        close.Glyph = PillGlyph.Close;
        close.Fill = Theme.Background;
        close.FillHover = Theme.Accent;
        close.Foreground = Theme.MutedText;
        close.ForegroundHover = Color.White;
        close.CornerRadius = 8;
        close.SetBounds(WindowWidth - 40, HeaderTop, HeaderButtonWidth, HeaderButtonHeight);
        close.Click += delegate { HideToTray(); };

        Controls.AddRange(new Control[] { title, minimise, close });
    }

    void BuildDial()
    {
        ring.SetBounds((WindowWidth - RingSize) / 2, RingTop, RingSize, RingSize);

        slider.SetBounds(SliderMargin, SliderTop, WindowWidth - SliderMargin * 2, SliderHeight);
        slider.ValueChanged += delegate { ShowSelection(); };

        Controls.AddRange(new Control[] { ring, slider });
    }

    void BuildPresets()
    {
        for (int i = 0; i < PresetMinutes.Length; i++)
        {
            int minutes = PresetMinutes[i];
            PillButton button = new PillButton();
            button.Text = Duration.Compact(minutes);
            button.Font = Theme.PresetFont;
            button.SetBounds(EdgeGap + i * (PresetWidth + PresetGap), PresetTop, PresetWidth, PresetHeight);
            button.Click += delegate { slider.Value = minutes; };
            presetButtons[i] = button;
            Controls.Add(button);
        }
    }

    void BuildActions()
    {
        actionButton.CornerRadius = 12;
        actionButton.Font = Theme.ActionFont;
        actionButton.SetBounds(EdgeGap, ActionTop, ContentWidth, ActionHeight);
        actionButton.Click += delegate { if (armed) CancelTimer(); else StartTimer(); };

        Caption hint = new Caption();
        hint.Text = "Force-closes all apps. Unsaved work is lost.";
        hint.Foreground = Theme.MutedText;
        hint.Font = Theme.HintFont;
        hint.Centered = true;
        hint.SetBounds(EdgeGap, HintTop, ContentWidth, HintHeight);
        hint.MouseDown += StartWindowDrag;

        Controls.AddRange(new Control[] { actionButton, hint });
        ShowIdleAction();
    }

    void BuildTray()
    {
        tray.ShowIdle();
        tray.Visible = true;
        tray.Notify.DoubleClick += delegate { Restore(); };

        ContextMenuStrip menu = new ContextMenuStrip();
        menu.Items.Add("Show", null, delegate { Restore(); });
        menu.Items.Add("Cancel shutdown", null, delegate { CancelTimer(); });
        menu.Items.Add("Exit", null, delegate { reallyClosing = true; Close(); });
        tray.Notify.ContextMenuStrip = menu;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        int corners = DwmRoundCorners;
        try { DwmSetWindowAttribute(Handle, DwmCornerPreference, ref corners, 4); } catch { }
        int border = BorderColourBgr;
        try { DwmSetWindowAttribute(Handle, DwmBorderColour, ref border, 4); } catch { }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == Program.ShowWindowMessage) Restore();
        base.WndProc(ref m);
    }

    void StartWindowDrag(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        ReleaseCapture();
        SendMessage(Handle, WmNcLeftButtonDown, HitCaption, 0);
    }

    void ShowSelection()
    {
        int minutes = slider.Value;

        for (int i = 0; i < presetButtons.Length; i++)
        {
            presetButtons[i].Selected = PresetMinutes[i] == minutes;
            presetButtons[i].Invalidate();
        }

        string amount, unit;
        Duration.Split(minutes, out amount, out unit);

        ring.Progress = 1f;
        ring.HeadlineSize = unit == "" ? 30f : 38f;
        ring.Headline = amount;
        ring.Caption = unit;
        ring.Invalidate();
    }

    void StartTimer()
    {
        scheduledSeconds = slider.Value * 60;

        int code = ShutdownCommand.Schedule(scheduledSeconds);
        if (code != ShutdownCommand.Success)
        {
            MessageBox.Show(this, "Could not schedule shutdown (code " + code + ").", AppName);
            return;
        }

        deadline = DateTime.Now.AddSeconds(scheduledSeconds);
        armed = true;
        lastShownSecond = -1;

        ShowArmedAction();
        SetInputsEnabled(false);

        ticker.Start();
        OnTick(null, null);
        tray.Balloon("Shutting down in " + Duration.Compact(slider.Value) + ".");
    }

    void CancelTimer()
    {
        if (armed) ShutdownCommand.Cancel();
        armed = false;
        ticker.Stop();

        ShowIdleAction();
        SetInputsEnabled(true);

        tray.ShowIdle();
        lastShownSecond = -1;
        ShowSelection();
    }

    void ShowIdleAction()
    {
        actionButton.Text = "Start";
        actionButton.Fill = Theme.Accent;
        actionButton.FillHover = Theme.AccentHover;
        actionButton.Foreground = Color.White;
        actionButton.Invalidate();
    }

    void ShowArmedAction()
    {
        actionButton.Text = "Cancel";
        actionButton.Fill = Theme.Track;
        actionButton.FillHover = Theme.TrackHover;
        actionButton.Foreground = Theme.Text;
        actionButton.Invalidate();
    }

    void SetInputsEnabled(bool enabled)
    {
        slider.Enabled = enabled;
        slider.Invalidate();
        foreach (PillButton button in presetButtons) button.Enabled = enabled;
    }

    void OnTick(object sender, EventArgs e)
    {
        TimeSpan left = deadline - DateTime.Now;

        if (left.TotalSeconds <= 0)
        {
            ShowShuttingDown();
            return;
        }

        ring.Progress = (float)(left.TotalSeconds / scheduledSeconds);
        ring.HeadlineSize = left.TotalHours >= 1 ? 27f : 34f;
        ring.Headline = Duration.Clock(left);
        ring.Caption = "remaining";
        ring.Invalidate();

        tray.Tooltip = "Shutdown in " + Duration.Spoken(left);

        int second = (int)left.TotalSeconds;
        if (second == lastShownSecond) return;
        lastShownSecond = second;
        tray.ShowCountdown(ring.Progress);
    }

    void ShowShuttingDown()
    {
        ticker.Stop();
        ring.Progress = 0f;
        ring.HeadlineSize = 26f;
        ring.Headline = "0:00";
        ring.Caption = "shutting down";
        ring.Invalidate();
        tray.Tooltip = "Shutting down";
        tray.ShowCountdown(0f);
    }

    void HideToTray()
    {
        Hide();
    }

    void Restore()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
        SetForegroundWindow(Handle);
    }

    void OnFormClosing(object sender, FormClosingEventArgs e)
    {
        if (!reallyClosing && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            HideToTray();
            return;
        }

        if (armed)
        {
            DialogResult answer = MessageBox.Show(this, "Cancel the pending shutdown too?", AppName,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == DialogResult.Yes) ShutdownCommand.Cancel();
        }

        tray.Dispose();
    }
}
