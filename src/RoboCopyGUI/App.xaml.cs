using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace RoboCopyGUI;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        InitializeComponent();

        UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
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
            WriteCrashLog(ex);
            ShowFatalError(ex);
        }
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        WriteCrashLog(e.Exception);
    }

    private void OnDomainUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            WriteCrashLog(ex);
    }

    private static void WriteCrashLog(Exception ex)
    {
        try
        {
            var logDir = Path.Combine(AppContext.BaseDirectory, "log");
            Directory.CreateDirectory(logDir);
            var logFile = Path.Combine(logDir, $"crash-{DateTime.Now:yyyyMMdd-HHmmss}.log");
            File.WriteAllText(logFile,
                $"=== RoboCopy GUI Crash Log ===\n" +
                $"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                $".NET: {Environment.Version}\n" +
                $"OS: {Environment.OSVersion}\n" +
                $"Working Dir: {Environment.CurrentDirectory}\n" +
                $"App Dir: {AppContext.BaseDirectory}\n\n" +
                $"Exception: {ex}\n\n" +
                $"Stack Trace:\n{ex.StackTrace}\n");
        }
        catch { }
    }

    private static void ShowFatalError(Exception ex)
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = "Fatal Error / 启动错误",
                Content = $"RoboCopy GUI failed to start.\n\n启动失败，错误信息：\n\n{ex.Message}\n\nFull details saved to log/crash-*.log\n完整错误已保存到 log/crash-*.log",
                CloseButtonText = "OK"
            };
            dialog.ShowAsync();
        }
        catch
        {
            // If even the dialog fails, write to stderr
            Console.Error.WriteLine($"FATAL: {ex}");
        }
    }
}
