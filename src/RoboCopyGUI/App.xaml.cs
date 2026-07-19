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
            try
            {
                var logDir = Path.Combine(AppContext.BaseDirectory, "log");
                Directory.CreateDirectory(logDir);
                File.WriteAllText(
                    Path.Combine(logDir, $"crash-{DateTime.Now:yyyyMMdd-HHmmss}.log"),
                    $"Exception: {e.Exception}\n\nStack:\n{e.Exception.StackTrace}");
            }
            catch { }
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
            try
            {
                var logDir = Path.Combine(AppContext.BaseDirectory, "log");
                Directory.CreateDirectory(logDir);
                File.WriteAllText(
                    Path.Combine(logDir, $"crash-{DateTime.Now:yyyyMMdd-HHmmss}.log"),
                    $"Exception: {ex}\n\nStack:\n{ex.StackTrace}");
            }
            catch { }
        }
    }
}
