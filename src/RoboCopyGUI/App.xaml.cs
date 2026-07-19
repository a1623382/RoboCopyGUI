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
.Is 64-bit: {Environment.Is64BitOperatingSystem}
.Is 64-bit process: {Environment.Is64BitProcess}
.NET Version: {Environment.Version}
App Dir: {AppContext.BaseDirectory}
Working Dir: {Environment.CurrentDirectory}
Command Line: {Environment.CommandLine}

--- Exception ---
Type: {ex.GetType().FullName}
Message: {ex.Message}
HRESULT: 0x{ex.HResult:X8}
Source: {ex.Source}
TargetSite: {ex.TargetSite}
{(innerChain.Length > 0 ? $"Inner Exceptions:{innerChain}" : "Inner: (none)")}

--- Stack Trace ---
{ex.StackTrace}

--- Assemblies ---
""";

            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        log += $"\n  {asm.GetName().Name} v{asm.GetName().Version} ({(asm.IsDynamic ? "dynamic" : asm.Location)})";
                    }
                    catch { log += $"\n  (failed to read assembly info)"; }
                }
            }
            catch { log += "\n  (failed to enumerate assemblies)"; }

            log += "\n\n--- Files in App Dir ---\n";
            try
            {
                foreach (var f in Directory.GetFiles(AppContext.BaseDirectory, "*", SearchOption.TopDirectoryOnly).OrderBy(x => x))
                {
                    var fi = new FileInfo(f);
                    log += $"  {fi.Name} ({fi.Length} bytes)\n";
                }
            }
            catch { log += "  (failed to list files)\n"; }

            log += "\n--- PRI/XAML Resources ---\n";
            try
            {
                foreach (var f in Directory.GetFiles(AppContext.BaseDirectory, "*.pri", SearchOption.AllDirectories))
                    log += $"  PRI: {f}\n";
                foreach (var f in Directory.GetFiles(AppContext.BaseDirectory, "*.xbf", SearchOption.AllDirectories))
                    log += $"  XBF: {f}\n";
                foreach (var f in Directory.GetFiles(AppContext.BaseDirectory, "*.xaml", SearchOption.AllDirectories))
                    log += $"  XAML: {f}\n";
            }
            catch { log += "  (failed to list resources)\n"; }

            File.WriteAllText(logFile, log);
        }
        catch { }
    }
}
