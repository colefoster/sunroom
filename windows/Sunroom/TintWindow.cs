using System.Drawing.Imaging;

namespace Sunroom;

internal sealed class TintWindow : Form
{
    private readonly Preferences preferences;
    internal Action? LockRequested;
    internal bool IsPinned => (Native.GetWindowLong(Handle, Native.GwlExStyle) & 8) != 0;
    private bool applying;

    internal TintWindow(Preferences preferences)
    {
        this.preferences = preferences;
        Text = "Sunroom tint";
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        AutoScaleMode = AutoScaleMode.None;
        MinimumSize = new(160, 100);
        Bounds = preferences.Bounds;
    }

    protected override bool ShowWithoutActivation => preferences?.Locked ?? true;
    protected override CreateParams CreateParams
    {
        get
        {
            var value = base.CreateParams;
            value.ExStyle |= Native.ExLayered | Native.ExToolWindow;
            if (preferences?.Locked ?? true) value.ExStyle |= Native.ExTransparent | Native.ExNoActivate;
            return value;
        }
    }

    internal void Apply()
    {
        applying = true;
        int style = Native.GetWindowLong(Handle, Native.GwlExStyle);
        style &= ~(Native.ExTransparent | Native.ExNoActivate);
        if (preferences.Locked) style |= Native.ExTransparent | Native.ExNoActivate;
        Native.SetWindowLong(Handle, Native.GwlExStyle, style);
        if (preferences.Enabled)
        {
            if (!Visible) Show();
            RenderSurface();
        }
        else Hide();
        // Form.TopMost may activate the window; changing Z-order must preserve the user's keyboard focus.
        Native.SetWindowPos(Handle, preferences.AlwaysOnTop ? -1 : -2, 0, 0, 0, 0, 0x13);
        applying = false;
    }

    protected override void OnShown(EventArgs e) { base.OnShown(e); RenderSurface(); }
    protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); if (IsHandleCreated) RenderSurface(); }
    protected override void OnLocationChanged(EventArgs e) { base.OnLocationChanged(e); if (IsHandleCreated && !applying) RenderSurface(); }

    private Rectangle LockButton => new(Width - 112, 6, 104, 27);

    private void RenderSurface()
    {
        if (Width < 1 || Height < 1) return;
        // Per-pixel alpha keeps editing handles readable even at 5% tint.
        using var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.FromArgb((int)Math.Round(preferences.Opacity * 2.55), preferences.Tint));
            if (!preferences.Locked)
            {
                using var outline = new Pen(Color.FromArgb(255, 230, 186, 140), 3);
                graphics.DrawRectangle(outline, 1, 1, Width - 3, Height - 3);
                using var header = new SolidBrush(Color.FromArgb(255, 35, 37, 35));
                graphics.FillRectangle(header, 3, 3, Width - 6, 34);
                using var label = new SolidBrush(Color.White);
                using var font = new Font("Segoe UI", 10);
                graphics.DrawString(Width > 330 ? "Sunroom · drag to move" : "Sunroom", font, label, 10, 10);
                using var button = new SolidBrush(Color.FromArgb(255, 83, 65, 48));
                graphics.FillRectangle(button, LockButton);
                graphics.DrawString("Lock tint", font, label, LockButton.X + 18, 10);
                foreach (var point in new[] { new Point(0, 0), new Point(Width - 8, 0), new Point(0, Height - 8), new Point(Width - 8, Height - 8) })
                    graphics.FillRectangle(label, point.X, point.Y, 8, 8);
            }
        }
        nint screen = Native.GetDC(0), memory = Native.CreateCompatibleDC(screen);
        nint surface = bitmap.GetHbitmap(Color.FromArgb(0)), previous = Native.SelectObject(memory, surface);
        try
        {
            var destination = new Native.Point(Left, Top);
            var size = new Native.Size(Width, Height);
            var source = new Native.Point(0, 0);
            var blend = new Native.Blend { Alpha = 255, Format = 1 };
            if (!Native.UpdateLayeredWindow(Handle, screen, ref destination, ref size, memory, ref source, 0, ref blend, 2))
                throw new System.ComponentModel.Win32Exception();
        }
        finally
        {
            Native.SelectObject(memory, previous);
            Native.DeleteObject(surface);
            Native.DeleteDC(memory);
            Native.ReleaseDC(0, screen);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (!preferences.Locked && e.Button == MouseButtons.Left && LockButton.Contains(e.Location)) LockRequested?.Invoke();
    }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == 0x84 && !preferences.Locked) // WM_NCHITTEST enables native dragging and edge resizing.
        {
            long packed = message.LParam.ToInt64();
            var point = PointToClient(new Point((short)(packed & 0xffff), (short)((packed >> 16) & 0xffff)));
            bool left = point.X < 8, right = point.X >= Width - 8, top = point.Y < 8, bottom = point.Y >= Height - 8;
            int hit = top && left ? 13 : top && right ? 14 : bottom && left ? 16 : bottom && right ? 17
                : left ? 10 : right ? 11 : top ? 12 : bottom ? 15 : LockButton.Contains(point) ? 1 : 2;
            message.Result = hit;
            return;
        }
        if (message.Msg == 0x21 && preferences.Locked) { message.Result = 3; return; } // MA_NOACTIVATE
        base.WndProc(ref message);
    }
}
