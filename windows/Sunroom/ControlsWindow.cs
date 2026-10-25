namespace Sunroom;

internal sealed class ControlsWindow : Form
{
    internal readonly CheckBox ShowTint = new() { Name = "ShowTint", Text = "Show tint", AutoSize = true };
    internal readonly Button LockTint = new() { Name = "LockTint", AutoSize = true };
    internal readonly CheckBox PinTint = new() { Name = "PinTint", Text = "Always on top", AutoSize = true };
    internal readonly TrackBar Strength = new() { Name = "Strength", Minimum = 5, Maximum = 80, TickFrequency = 15, Width = 245, Height = 44 };
    internal readonly Button CustomColor = new() { Name = "CustomColor", Text = "Choose color…", AutoSize = true };
    internal readonly Label Mode = new() { Name = "Mode", AutoSize = true, MaximumSize = new(340, 0) };
    internal readonly Label Percentage = new() { AutoSize = true, Padding = new(0, 8, 0, 0) };
    internal readonly Button ResetBox = new() { Name = "ResetBox", Text = "Bring box to this screen", AutoSize = true };
    internal readonly Button Amber = new() { Name = "Amber", Text = "Amber", AutoSize = true };
    internal readonly Button Blue = new() { Name = "Blue", Text = "Blue", AutoSize = true };
    internal readonly Button Gray = new() { Name = "Gray", Text = "Gray", AutoSize = true };
    internal readonly Button Quit = new() { Name = "Quit", Text = "Quit Sunroom", AutoSize = true };
    private bool exiting;

    internal ControlsWindow()
    {
        Text = "Sunroom";
        Name = "SunroomControls";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new(390, 620);
        BackColor = Color.FromArgb(27, 30, 28);
        ForeColor = Color.FromArgb(240, 238, 229);
        var layout = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, Dock = DockStyle.Fill, Padding = new(20), AutoScroll = true };
        Controls.Add(layout);
        layout.Controls.Add(new Label { Text = "Sunroom", Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true });
        layout.Controls.Add(new Label { Text = "A movable tint for bright screens.", AutoSize = true, Margin = new(3, 0, 3, 14) });
        layout.Controls.Add(ShowTint);
        layout.Controls.Add(Mode);
        layout.Controls.Add(LockTint);
        layout.Controls.Add(PinTint);
        layout.Controls.Add(new Label { Text = "Tint strength", AutoSize = true, Margin = new(3, 12, 3, 0) });
        var opacity = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty };
        Strength.BackColor = BackColor;
        opacity.Controls.Add(Strength);
        opacity.Controls.Add(Percentage);
        layout.Controls.Add(opacity);
        var presets = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty };
        presets.Controls.AddRange([Amber, Blue, Gray]);
        layout.Controls.Add(presets);
        layout.Controls.Add(CustomColor);
        layout.Controls.Add(ResetBox);
        layout.Controls.Add(new Label { Text = "Close this panel to keep Sunroom in the tray.", AutoSize = true, Margin = new(3, 12, 3, 5) });
        layout.Controls.Add(Quit);
        foreach (var button in new[] { LockTint, Amber, Blue, Gray, CustomColor, ResetBox, Quit })
        {
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.FromArgb(47, 51, 47);
            button.ForeColor = ForeColor;
            button.Padding = new(5, 2, 5, 2);
        }
        FormClosing += (_, e) => { if (!exiting && e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; Hide(); } };
    }

    internal void Exit() { exiting = true; Close(); }

    internal void RefreshState(Preferences preferences)
    {
        ShowTint.Checked = preferences.Enabled;
        PinTint.Checked = preferences.AlwaysOnTop;
        Strength.Value = preferences.Opacity;
        Percentage.Text = preferences.Opacity + "%";
        LockTint.Text = preferences.Locked ? "Unlock · move & resize" : "Lock · click through";
        Mode.Text = preferences.Locked ? "Locked — clicks pass through the tint." : "Unlocked — drag the box or resize its edges.";
    }
}
