using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sunroom;

internal sealed class Preferences
{
    public int X { get; set; } = 100;
    public int Y { get; set; } = 100;
    public int Width { get; set; } = 800;
    public int Height { get; set; } = 600;
    public int Red { get; set; } = 38;
    public int Green { get; set; } = 26;
    public int Blue { get; set; }
    public int Opacity { get; set; } = 30;
    public bool Enabled { get; set; } = true;
    public bool Locked { get; set; } = true;
    public bool AlwaysOnTop { get; set; } = true;

    [JsonIgnore] public Color Tint => Color.FromArgb(Red, Green, Blue);
    [JsonIgnore] public Rectangle Bounds => new(X, Y, Width, Height);

    public static Preferences Load(string file)
    {
        Preferences value;
        try { value = JsonSerializer.Deserialize<Preferences>(File.ReadAllText(file)) ?? new(); }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException) { value = new(); }
        value.Red = Math.Clamp(value.Red, 0, 255);
        value.Green = Math.Clamp(value.Green, 0, 255);
        value.Blue = Math.Clamp(value.Blue, 0, 255);
        value.Opacity = Math.Clamp(value.Opacity, 5, 80);
        value.Width = Math.Clamp(value.Width, 160, SystemInformation.VirtualScreen.Width);
        value.Height = Math.Clamp(value.Height, 100, SystemInformation.VirtualScreen.Height);
        if (!Screen.AllScreens.Any(s => Rectangle.Intersect(s.WorkingArea, value.Bounds) is { Width: >= 80, Height: >= 60 }))
        {
            var area = Screen.PrimaryScreen!.WorkingArea;
            value.X = area.X + 60;
            value.Y = area.Y + 60;
            value.Width = Math.Min(value.Width, area.Width - 100);
            value.Height = Math.Min(value.Height, area.Height - 100);
        }
        return value;
    }

    public void Save(string file)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file + ".tmp", JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(file + ".tmp", file, true);
    }
}
