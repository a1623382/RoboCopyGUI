using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RoboCopyGUI.Controls;
using RoboCopyGUI.ViewModels;
using WinRT.Interop;

namespace RoboCopyGUI;

public sealed partial class MainWindow : Window
{
    private MicaController? _micaController;
    private SystemBackdropConfiguration? _backdropConfiguration;

    public MainWindow()
    {
        InitializeComponent();
        Title = "RoboCopy GUI";

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        TrySetMicaBackdrop();

        Closed += Window_Closed;
    }

    private MainViewModel? GetViewModel() => Root.DataContext as MainViewModel;

    private void TrySetMicaBackdrop()
    {
        if (MicaController.IsSupported())
        {
            _backdropConfiguration = new SystemBackdropConfiguration
            {
                Theme = ActualTheme switch
                {
                    ElementTheme.Dark => SystemBackdropTheme.Dark,
                    ElementTheme.Light => SystemBackdropTheme.Light,
                    _ => SystemBackdropTheme.Default
                }
            };

            _micaController = new MicaController();
            _micaController.SetSystemBackdropConfiguration(_backdropConfiguration);

            this.ActualThemeChanged += (_, _) =>
            {
                if (_backdropConfiguration is not null)
                {
                    _backdropConfiguration.Theme = ActualTheme switch
                    {
                        ElementTheme.Dark => SystemBackdropTheme.Dark,
                        ElementTheme.Light => SystemBackdropTheme.Light,
                        _ => SystemBackdropTheme.Default
                    };
                }
            };

            _micaController.AddSystemBackdropTarget(
                this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
        }
    }

    private async void Settings_Click(object sender, RoutedEventArgs e)
    {
        var vm = GetViewModel();
        if (vm is null) return;

        var settingsPanel = new SettingsPanel
        {
            DataContext = vm,
            Settings = vm.Settings
        };

        var dialog = new ContentDialog
        {
            Title = "Settings",
            Content = settingsPanel,
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = MainGrid.XamlRoot
        };

        await dialog.ShowAsync();
    }

    private async void LoadQueue_Click(object sender, RoutedEventArgs e)
    {
        var vm = GetViewModel();
        if (vm is null) return;

        var picker = new FileOpenPicker();
        var hwnd = WindowNative.GetWindowHandle(this);
        InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeFilter.Add(".json");

        var file = await picker.PickSingleFileAsync();
        if (file is not null)
            await vm.ImportQueueCommand.ExecuteAsync(file.Path);
    }

    private async void ExportBat_Click(object sender, RoutedEventArgs e)
    {
        var vm = GetViewModel();
        if (vm is null) return;

        var picker = new FileSavePicker();
        var hwnd = WindowNative.GetWindowHandle(this);
        InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeChoices.Add("Batch File", [".bat"]);
        picker.SuggestedFileName = "robocopy_tasks.bat";

        var file = await picker.PickSaveFileAsync();
        if (file is not null)
            await vm.ExportBatCommand.ExecuteAsync(file.Path);
    }

    private async void ExportPs1_Click(object sender, RoutedEventArgs e)
    {
        var vm = GetViewModel();
        if (vm is null) return;

        var picker = new FileSavePicker();
        var hwnd = WindowNative.GetWindowHandle(this);
        InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeChoices.Add("PowerShell Script", [".ps1"]);
        picker.SuggestedFileName = "robocopy_tasks.ps1";

        var file = await picker.PickSaveFileAsync();
        if (file is not null)
            await vm.ExportPowerShellCommand.ExecuteAsync(file.Path);
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        _micaController?.Dispose();
        _micaController = null;
    }
}
