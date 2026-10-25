using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.Json;
using Sunroom;

namespace Sunroom.Tests;

internal static class Program
{
    private static readonly List<string> passed = [];
    private static string output = "";
    private static SunroomContext? app;

    [STAThread]
    private static void Main(string[] args)
    {
        output = args.Length > 0 ? args[0] : Path.Combine(Path.GetTempPath(), "Sunroom-tests");
        Directory.CreateDirectory(output);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        if (args.Contains("--preferences-only"))
        {
            try
            {
                TestPreferences();
                File.WriteAllText(Path.Combine(output, "preferences-result.json"), JsonSerializer.Serialize(new { Success = true, Passed = passed }));
            }
            catch (Exception error)
            {
                File.WriteAllText(Path.Combine(output, "preferences-result.json"), JsonSerializer.Serialize(new { Success = false, Error = error.ToString() }));
                Environment.ExitCode = 1;
            }
            return;
        }
        var area = Screen.PrimaryScreen!.WorkingArea;
        using var target = new Form { Text = "Sunroom interaction test — temporary", StartPosition = FormStartPosition.Manual,
            Bounds = new(area.X + 100, area.Y + 80, Math.Min(1100, area.Width - 150), Math.Min(760, area.Height - 120)), BackColor = Color.White };
        var button = new Button { Text = "Shared screen: click target", Bounds = new(220, 190, 260, 65), Font = new("Segoe UI", 12) };
        var title = new Label { Text = "A bright shared screen", AutoSize = true, Location = new(32, 38), Font = new("Segoe UI", 24) };
        var clicks = new Label { Text = "Clicks received: 0", AutoSize = true, Location = new(220, 275) };
        var description = new Label { Text = "Sunroom's tint is an independent rectangle.\nLocked: this button still works. Unlocked: move and resize the tint.", AutoSize = true, Location = new(32, 100) };
        int count = 0;
        button.Click += (_, _) => { count++; clicks.Text = "Clicks received: " + count; };
        target.Controls.AddRange([button, title, clicks, description]);
        nint previousForeground = GetForegroundWindow();
        bool minimizedPrevious = false;
        GetCursorPos(out Point cursor);
        target.Shown += async (_, _) =>
        {
            try
            {
                await Task.Delay(500);
                // A full-screen game may capture the cursor. Minimize it temporarily, then restore it in finally.
                if (previousForeground != 0 && previousForeground != target.Handle)
                    minimizedPrevious = ShowWindowAsync(previousForeground, 6);
                target.TopMost = true;
                SetForegroundWindow(target.Handle);
                await Task.Delay(400);
                Point titleBar = new(target.Left + 80, target.Top + 15);
                Assert(GetAncestor(WindowFromPoint(titleBar), 2) == target.Handle, "Test title bar is exposed before clicking it");
                await Click(titleBar);
                target.TopMost = false;
                Assert(GetForegroundWindow() == target.Handle, "Test window has foreground before injecting mouse input");
                string data = Path.Combine(output, "preferences");
                Directory.CreateDirectory(data);
                File.Delete(Path.Combine(data, "settings.json"));
                app = new(data, false);
                Assert(app.Overlay.Visible && app.State.Locked, "Starts visible and locked");
                app.Overlay.Bounds = new(target.Left + 150, target.Top + 130, 620, 430);
                app.Apply();
                target.Activate();
                await Task.Delay(300);
                Assert((Native.GetWindowLong(app.Overlay.Handle, Native.GwlExStyle) & Native.ExTransparent) != 0, "Locked window uses native click-through");
                await Click(button.PointToScreen(new(button.Width / 2, button.Height / 2)));
                Assert(count == 1, "Real mouse click reaches button through locked tint");
                Assert(GetForegroundWindow() == target.Handle, "Locked click focuses the underlying app");
                app.Apply();
                Assert(GetForegroundWindow() == target.Handle, "Updating locked overlay does not take focus");
                await Capture(target, "locked-click-through.png");
                Point sample = new(app.Overlay.Left + 500, app.Overlay.Top + 350);
                Color pixel = Sample(sample);
                int expected = (int)Math.Round(255 * 0.7 + app.State.Red * 0.3);
                Assert(Math.Abs(pixel.R - expected) < 8, $"Actual tint alpha blends correctly (red {pixel.R}, expected {expected})");

                Press(app.Controls.LockTint);
                await Task.Delay(150);
                Assert(!app.State.Locked && (Native.GetWindowLong(app.Overlay.Handle, Native.GwlExStyle) & Native.ExTransparent) == 0, "Unlock removes click-through");
                await Click(button.PointToScreen(new(button.Width / 2, button.Height / 2)));
                Assert(count == 1, "Unlocked box consumes clicks instead of hitting content behind it");
                Rectangle before = app.Overlay.Bounds;
                await Drag(new(before.Left + 100, before.Top + 19), new(before.Left + 180, before.Top + 69));
                Assert(Math.Abs(app.Overlay.Left - before.Left - 80) <= 3 && Math.Abs(app.Overlay.Top - before.Top - 50) <= 3, "Native mouse drag moves the independent box");
                before = app.Overlay.Bounds;
                await Drag(new(before.Right - 3, before.Bottom - 3), new(before.Right + 87, before.Bottom + 57));
                Assert(app.Overlay.Width >= before.Width + 85 && app.Overlay.Height >= before.Height + 55, "Native corner drag resizes the box");
                await Capture(target, "unlocked-moved-resized.png");

                app.Controls.Strength.Value = 5;
                await Task.Delay(100);
                Assert(app.State.Opacity == 5, "Strength control applies 5% minimum");
                var header = Sample(new(app.Overlay.Left + 50, app.Overlay.Top + 30));
                Assert(header.R < 80, "Editing header stays opaque at minimum tint");
                app.Controls.Strength.Value = 80;
                Assert(app.State.Opacity == 80, "Strength control applies 80% maximum");
                Press(app.Controls.Blue);
                Assert(app.State.Tint == Color.FromArgb(0, 13, 51) && app.State.Opacity == 25, "Blue preset applies color and strength");
                Press(app.Controls.Gray);
                Assert(app.State.Tint == Color.FromArgb(13, 13, 13) && app.State.Opacity == 35, "Gray preset applies color and strength");
                Press(app.Controls.Amber);
                Assert(app.State.Tint == Color.FromArgb(51, 31, 0) && app.State.Opacity == 30, "Amber preset applies color and strength");
                app.Controls.PinTint.Checked = false;
                Assert(!app.Overlay.IsPinned && !app.State.AlwaysOnTop, "Always-on-top can be disabled");
                app.Controls.PinTint.Checked = true;
                Assert(app.Overlay.IsPinned, "Always-on-top can be re-enabled");

                before = app.Overlay.Bounds;
                app.Controls.ShowTint.Checked = false;
                Assert(!app.Overlay.Visible && !app.State.Locked, "Hiding preserves unlocked mode");
                app.Controls.ShowTint.Checked = true;
                Assert(app.Overlay.Visible && app.Overlay.Bounds == before && !app.State.Locked, "Showing restores geometry and mode");
                await Click(new(app.Overlay.Right - 55, app.Overlay.Top + 20));
                Assert(app.State.Locked, "On-box Lock tint button locks the overlay");
                app.ShowControls();
                await Task.Delay(200);
                await Capture(app.Controls, "settings.png");
                app.Controls.Close();
                Assert(!app.Controls.Visible && app.Overlay.Visible, "Closing settings keeps tint and tray app running");
                app.ShowControls();
                Assert(app.Controls.Visible, "Settings can reopen after close");
                app.Controls.Hide();

                var monitors = new List<object>();
                foreach (var screen in Screen.AllScreens)
                {
                    app.Overlay.Bounds = new(screen.WorkingArea.Left + 60, screen.WorkingArea.Top + 80, 380, 240);
                    app.Apply();
                    await Task.Delay(250);
                    monitors.Add(new { screen.DeviceName, screen.Bounds, Dpi = GetDpiForWindow(app.Overlay.Handle) });
                    Assert(screen.Bounds.Contains(app.Overlay.Bounds), "Overlay moves to monitor " + screen.DeviceName);
                    Assert((Native.GetWindowLong(app.Overlay.Handle, Native.GwlExStyle) & Native.ExTransparent) != 0, "Click-through survives monitor change " + screen.DeviceName);
                }
                app.Overlay.Bounds = new(target.Left + 150, target.Top + 130, 620, 430);
                app.State.Enabled = false;
                app.State.Locked = false;
                app.Apply();
                await Task.Delay(600);
                Rectangle saved = app.Overlay.Bounds;
                app.Dispose(); app = null;
                app = new(data, false);
                Assert(app.Overlay.Bounds == saved && !app.State.Enabled && !app.State.Locked && app.State.Red == 51, "Geometry, color, hidden state and lock survive restart");
                app.State.Enabled = true; app.State.Locked = true; app.Apply();
                await Capture(target, "restored.png");
                app.Dispose(); app = null;

                TestPreferences();
                File.WriteAllText(Path.Combine(output, "result.json"), JsonSerializer.Serialize(new { Success = true, Passed = passed, Monitors = monitors, Session = System.Diagnostics.Process.GetCurrentProcess().SessionId }, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception error)
            {
                if (GetForegroundWindow() == target.Handle) await Capture(target, "failure.png");
                GetCursorPos(out Point actualCursor);
                GetWindowThreadProcessId(GetForegroundWindow(), out uint owner);
                File.WriteAllText(Path.Combine(output, "result.json"), JsonSerializer.Serialize(new { Success = false, Passed = passed, Error = error.ToString(), Session = System.Diagnostics.Process.GetCurrentProcess().SessionId, Target = target.Bounds, Cursor = actualCursor, Foreground = GetForegroundWindow().ToInt64(), ForegroundProcess = System.Diagnostics.Process.GetProcessById((int)owner).ProcessName, TargetHandle = target.Handle.ToInt64(), UnderCursor = WindowFromPoint(actualCursor).ToInt64() }, new JsonSerializerOptions { WriteIndented = true }));
                Environment.ExitCode = 1;
            }
            finally
            {
                app?.Dispose();
                SetCursorPos(cursor.X, cursor.Y);
                if (minimizedPrevious) ShowWindowAsync(previousForeground, 9);
                if (previousForeground != 0) SetForegroundWindow(previousForeground);
                target.Close();
            }
        };
        Application.Run(target);
    }

    private static void Assert(bool value, string description)
    {
        if (!value) throw new Exception(description);
        passed.Add(description);
        File.WriteAllLines(Path.Combine(output, "progress.txt"), passed);
    }

    private static void Press(Button button)
    {
        app!.ShowControls();
        button.PerformClick();
        app.Controls.Hide();
    }

    private static void TestPreferences()
    {
        string file = Path.Combine(output, "invalid-settings.json");
        File.WriteAllText(file, "{broken");
        Assert(Preferences.Load(file).Opacity == 30, "Malformed settings recover to defaults");
        var invalid = new Preferences { X = -99999, Y = -99999, Width = -1, Height = int.MaxValue, Red = -1, Blue = 500, Opacity = 500 };
        invalid.Save(file);
        var repaired = Preferences.Load(file);
        Assert(repaired.Red == 0 && repaired.Blue == 255 && repaired.Opacity == 80 && repaired.Width >= 160 && Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(repaired.Bounds)), "Invalid or off-screen preferences recover safely");
        File.Delete(file);
    }

    private static async Task Capture(Form target, string name)
    {
        await Task.Delay(200);
        using var image = new Bitmap(target.Width, target.Height);
        using var graphics = Graphics.FromImage(image);
        graphics.CopyFromScreen(target.Location, Point.Empty, target.Size);
        image.Save(Path.Combine(output, name), ImageFormat.Png);
    }
    private static Color Sample(Point point)
    {
        using var image = new Bitmap(1, 1);
        using var graphics = Graphics.FromImage(image);
        graphics.CopyFromScreen(point, Point.Empty, new(1, 1));
        return image.GetPixel(0, 0);
    }
    private static async Task Click(Point point)
    {
        await Task.Run(() => { SetCursorPos(point.X, point.Y); Thread.Sleep(80); Mouse(2); Thread.Sleep(60); Mouse(4); });
        await Task.Delay(200);
    }
    private static async Task Drag(Point from, Point to)
    {
        await Task.Run(() =>
        {
            SetCursorPos(from.X, from.Y); Thread.Sleep(100); Mouse(2); Thread.Sleep(100);
            for (int i = 1; i <= 12; i++) { SetCursorPos(from.X + (to.X - from.X) * i / 12, from.Y + (to.Y - from.Y) * i / 12); Thread.Sleep(25); }
            Mouse(4);
        });
        await Task.Delay(250);
    }
    private static void Mouse(uint flags)
    {
        var input = new Input { Type = 0, Data = new MouseInput { Flags = flags } };
        if (SendInput(1, [input], Marshal.SizeOf<Input>()) != 1) throw new System.ComponentModel.Win32Exception();
    }
    [StructLayout(LayoutKind.Sequential)] private struct Input { public uint Type; public MouseInput Data; }
    [StructLayout(LayoutKind.Sequential)] private struct MouseInput { public int X, Y; public uint Data, Flags, Time; public nuint Extra; }
    [DllImport("user32.dll", SetLastError = true)] private static extern uint SendInput(uint count, Input[] input, int size);
    [DllImport("user32.dll")] private static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out Point point);
    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(nint window);
    [DllImport("user32.dll")] private static extern uint GetDpiForWindow(nint window);
    [DllImport("user32.dll")] private static extern nint WindowFromPoint(Point point);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint window, out uint process);
    [DllImport("user32.dll")] private static extern bool ShowWindowAsync(nint window, int command);
    [DllImport("user32.dll")] private static extern nint GetAncestor(nint window, uint flags);
}
