using Microsoft.UI.Xaml;

namespace RoboCopyGUI;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        InitializeComponent();
        UnhandledException += (_, e) =>
        {
            e.Handled = true;
            WriteCrashLog(e.Exception, "UnhandledException");
        };
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            _window = new MainWindow();
            _window.Activate();
        }
        catch (Exception ex)
        {
            WriteCrashLog(ex, "OnLaunched");
        }
    }

    private static void WriteCrashLog(Exception ex, string source)
    {
        try
        {
            var logDir = Path.Combine(AppContext.BaseDirectory, "log");
            Directory.CreateDirectory(logDir);
            var logFile = Path.Combine(logDir, $"crash-{DateTime.Now:yyyyMMdd-HHmmss}.log");

            var inner = ex.InnerException;
            var innerChain = "";
            while (inner is not null)
            {
                innerChain += $"\n  -> {inner.GetType().FullName}: {inner.Message}";
                inner = inner.InnerException;
            }

            var log = $"""
=== RoboCopy GUI Crash Log ===
Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}
Source: {source}
OS: {Environment.OSVersion}
64-bit OS: {Environment.Is64BitOperatingSystem}
64-bit Process: {Environment.Is64BitProcess}
.NET Version: {Environment.Version}
App Dir: {AppContext.BaseDirectory}
Working Dir: {Environment.CurrentDirectory}

--- Exception ---
Type: {ex.GetType().FullName}
Message: {ex.Message}
HRESULT: 0x{ex.HResult:X8}
Source: {ex.Source}
{(innerChain.Length > 0 ? $"Inner Exceptions:{innerChain}" : "Inner: (none)")}

--- Stack Trace ---
{ex.StackTrace}

--- Files in App Dir ---
""";

            try
            {
                foreach (var f in Directory.GetFiles(AppContext.BaseDirectory, "*", SearchOption.TopDirectoryOnly).OrderBy(x => x))
                {
                    var fi = new FileInfo(f);
                    log += $"\n  {fi.Name} ({fi.Length} bytes)";
                }
            }
            catch { log += "\n  (failed to list files)"; }

            File.WriteAllText(logFile, log);
        }
        catch { }
    }
}
