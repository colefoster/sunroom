using System.Runtime.InteropServices;

namespace Sunroom;

internal static class Native
{
    internal const int ExLayered = 0x80000, ExTransparent = 0x20, ExNoActivate = 0x8000000, ExToolWindow = 0x80;
    internal const int GwlExStyle = -20;

    [StructLayout(LayoutKind.Sequential)] internal struct Point { public int X, Y; public Point(int x, int y) { X = x; Y = y; } }
    [StructLayout(LayoutKind.Sequential)] internal struct Size { public int Width, Height; public Size(int w, int h) { Width = w; Height = h; } }
    [StructLayout(LayoutKind.Sequential, Pack = 1)] internal struct Blend { public byte Op, Flags, Alpha, Format; }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")] internal static extern int GetWindowLong(nint window, int index);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")] internal static extern int SetWindowLong(nint window, int index, int value);
    [DllImport("user32.dll")] internal static extern bool SetWindowPos(nint window, nint after, int x, int y, int width, int height, uint flags);
    [DllImport("user32.dll")] internal static extern nint GetDC(nint window);
    [DllImport("user32.dll")] internal static extern int ReleaseDC(nint window, nint dc);
    [DllImport("gdi32.dll")] internal static extern nint CreateCompatibleDC(nint dc);
    [DllImport("gdi32.dll")] internal static extern nint SelectObject(nint dc, nint obj);
    [DllImport("gdi32.dll")] internal static extern bool DeleteObject(nint obj);
    [DllImport("gdi32.dll")] internal static extern bool DeleteDC(nint dc);
    [DllImport("user32.dll", SetLastError = true)] internal static extern bool UpdateLayeredWindow(nint window, nint dc, ref Point destination, ref Size size, nint sourceDC, ref Point source, uint key, ref Blend blend, uint flags);
}
