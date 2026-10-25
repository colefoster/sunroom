using Microsoft.Win32;

namespace Sunroom;

internal sealed class SunroomContext : ApplicationContext
{
    internal readonly Preferences State;
    internal readonly TintWindow Overlay;
    internal readonly ControlsWindow Controls;
    private readonly string file;
    private readonly NotifyIcon tray;
    private readonly Icon icon;
    private readonly System.Windows.Forms.Timer saveTimer = new() { Interval = 250 };
    private bool refreshing;
    private readonly ToolStripMenuItem showItem = new("Show tint") { CheckOnClick = false };
    private readonly ToolStripMenuItem lockItem = new("Unlock · move & resize");

    internal SunroomContext(string directory, bool showControls = true)
    {
        file = Path.Combine(directory, "settings.json");
        State = Preferences.Load(file);
        icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
        Overlay = new(State);
        Controls = new() { Icon = icon };
        _ = Controls.Handle;
        tray = new NotifyIcon { Icon = icon, Text = "Sunroom", Visible = true };
        var menu = new ContextMenuStrip();
        menu.Items.Add("Sunroom settings…", null, (_, _) => ShowControls());
        menu.Items.Add(showItem);
        menu.Items.Add(lockItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Quit Sunroom", null, (_, _) => ExitThread());
        tray.ContextMenuStrip = menu;
        tray.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) ShowControls(); };
        showItem.Click += (_, _) => { State.Enabled = !State.Enabled; Apply(); };
        lockItem.Click += (_, _) => SetLocked(!State.Locked);
        Controls.ShowTint.CheckedChanged += (_, _) => { if (!refreshing) { State.Enabled = Controls.ShowTint.Checked; Apply(); } };
        Controls.PinTint.CheckedChanged += (_, _) => { if (!refreshing) { State.AlwaysOnTop = Controls.PinTint.Checked; Apply(); } };
        Controls.LockTint.Click += (_, _) => SetLocked(!State.Locked);
        Controls.Strength.ValueChanged += (_, _) => { if (!refreshing) { State.Opacity = Controls.Strength.Value; Apply(); } };
        Controls.Amber.Click += (_, _) => SetTint(51, 31, 0, 30);
        Controls.Blue.Click += (_, _) => SetTint(0, 13, 51, 25);
        Controls.Gray.Click += (_, _) => SetTint(13, 13, 13, 35);
        Controls.CustomColor.Click += (_, _) =>
        {
            using var picker = new ColorDialog { Color = State.Tint, FullOpen = true };
            if (picker.ShowDialog(Controls) == DialogResult.OK)
                SetTint(picker.Color.R, picker.Color.G, picker.Color.B, State.Opacity);
        };
        Controls.ResetBox.Click += (_, _) =>
        {
            var area = Screen.FromControl(Controls).WorkingArea;
            Overlay.Bounds = new(area.X + 40, area.Y + 60, Math.Min(800, area.Width - 80), Math.Min(600, area.Height - 120));
            State.Enabled = true;
            SetLocked(false);
        };
        Controls.Quit.Click += (_, _) => ExitThread();
        Overlay.LockRequested = () => SetLocked(true);
        Overlay.Move += (_, _) => SaveBounds();
        Overlay.Resize += (_, _) => SaveBounds();
        saveTimer.Tick += (_, _) => { saveTimer.Stop(); Save(); };
        SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;
        Apply();
        if (showControls) ShowControls();
    }

    internal void ShowControls()
    {
        Controls.Show();
        Controls.WindowState = FormWindowState.Normal;
        Controls.BringToFront();
        Controls.Activate();
    }

    internal void SetLocked(bool locked)
    {
        State.Locked = locked;
        Apply();
        if (!locked && State.Enabled) { Controls.Hide(); Overlay.Activate(); }
    }

    private void SetTint(int red, int green, int blue, int opacity)
    {
        State.Red = red; State.Green = green; State.Blue = blue; State.Opacity = opacity;
        Apply();
    }

    internal void Apply()
    {
        refreshing = true;
        Controls.RefreshState(State);
        showItem.Checked = State.Enabled;
        lockItem.Text = State.Locked ? "Unlock · move & resize" : "Lock · click through";
        tray.Text = $"Sunroom · {(State.Enabled ? State.Locked ? "locked" : "move & resize" : "hidden")}";
        Overlay.Apply();
        refreshing = false;
        QueueSave();
    }

    private void SaveBounds()
    {
        State.X = Overlay.Left; State.Y = Overlay.Top;
        State.Width = Overlay.Width; State.Height = Overlay.Height;
        QueueSave();
    }

    private void QueueSave() { saveTimer.Stop(); saveTimer.Start(); }
    private void Save()
    {
        try { State.Save(file); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            tray.ShowBalloonTip(4000, "Sunroom", "Could not save settings. Check disk space and folder permissions.", ToolTipIcon.Warning);
        }
    }

    private void DisplaySettingsChanged(object? sender, EventArgs e)
    {
        if (Controls.IsDisposed) return;
        Controls.BeginInvoke(() =>
        {
            if (!Screen.AllScreens.Any(s => Rectangle.Intersect(s.WorkingArea, Overlay.Bounds) is { Width: >= 80, Height: >= 60 }))
            {
                var area = Screen.PrimaryScreen!.WorkingArea;
                Overlay.Location = new(area.X + 40, area.Y + 60);
            }
        });
    }

    protected override void ExitThreadCore()
    {
        Save();
        base.ExitThreadCore();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SystemEvents.DisplaySettingsChanged -= DisplaySettingsChanged;
            saveTimer.Stop(); saveTimer.Dispose();
            tray.Visible = false; tray.ContextMenuStrip?.Dispose(); tray.Dispose();
            Controls.Exit(); Controls.Dispose(); Overlay.Dispose(); icon.Dispose();
        }
        base.Dispose(disposing);
    }
}
