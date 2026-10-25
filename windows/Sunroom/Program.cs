namespace Sunroom;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        string directory = Environment.GetEnvironmentVariable("SUNROOM_DATA_DIR")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sunroom");
        Directory.CreateDirectory(directory);
        string id = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(directory)))[..16];
        using var mutex = new Mutex(true, @"Local\Sunroom-" + id, out bool first);
        using var show = new EventWaitHandle(false, EventResetMode.AutoReset, @"Local\Sunroom-Show-" + id);
        if (!first)
        {
            show.Set();
            return;
        }
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        try
        {
            using var context = new SunroomContext(directory);
            var wait = ThreadPool.RegisterWaitForSingleObject(show, (_, _) =>
            {
                try { context.Controls.BeginInvoke(context.ShowControls); }
                catch (InvalidOperationException) { }
            }, null, Timeout.Infinite, false);
            try { Application.Run(context); }
            finally { wait.Unregister(null); }
        }
        catch (Exception error)
        {
            File.AppendAllText(Path.Combine(directory, "error.log"), $"{DateTimeOffset.Now:o}\n{error}\n");
            MessageBox.Show("Sunroom stopped unexpectedly. Details were saved to " + directory, "Sunroom", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
