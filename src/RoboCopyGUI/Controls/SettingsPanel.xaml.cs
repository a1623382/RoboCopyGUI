using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RoboCopyGUI.Models;
using RoboCopyGUI.ViewModels;
using Windows.Storage.Pickers;

namespace RoboCopyGUI.Controls;

public sealed partial class SettingsPanel : UserControl
{
    public AppSettings Settings { get; set; } = new();

    public SettingsPanel()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        LoadFromSettings();
    }

    private void LoadFromSettings()
    {
        AutoSaveToggle.IsOn = Settings.AutoSaveQueue;
        ConfirmDeleteToggle.IsOn = Settings.ConfirmBeforeDelete;
        AutoStartToggle.IsOn = Settings.AutoStartQueueOnLaunch;
        NotificationsToggle.IsOn = Settings.ShowNotifications;
        MaxLogEntriesBox.Value = Settings.MaxLogEntries;
        DefaultSourceBox.Text = Settings.DefaultSourcePath;
        DefaultDestBox.Text = Settings.DefaultDestinationPath;
    }

    private void SaveToSettings()
    {
        Settings.AutoSaveQueue = AutoSaveToggle.IsOn;
        Settings.ConfirmBeforeDelete = ConfirmDeleteToggle.IsOn;
        Settings.AutoStartQueueOnLaunch = AutoStartToggle.IsOn;
        Settings.ShowNotifications = NotificationsToggle.IsOn;
        Settings.MaxLogEntries = MaxLogEntriesBox.Value;
        Settings.DefaultSourcePath = DefaultSourceBox.Text;
        Settings.DefaultDestinationPath = DefaultDestBox.Text;
    }

    private MainViewModel? GetViewModel() => DataContext as MainViewModel;

    private void TrimLog_Click(object sender, RoutedEventArgs e)
    {
        SaveToSettings();
        GetViewModel()?.TrimLogCommand.Execute(null);
    }

    private async void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        SaveToSettings();
        await (GetViewModel()?.SaveSettingsCommand.ExecuteAsync(null) ?? Task.CompletedTask);
    }

    private async void ResetSettings_Click(object sender, RoutedEventArgs e)
    {
        var vm = GetViewModel();
        if (vm is null) return;

        var dialog = new ContentDialog
        {
            Title = "Reset Settings",
            Content = "Are you sure you want to reset all settings to defaults?",
            PrimaryButtonText = "Reset",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await vm.ResetSettingsCommand.ExecuteAsync(null);
            LoadFromSettings();
        }
    }

    private async Task<string?> PickSaveFileAsync(string fileType, string suggestedName)
    {
        var picker = new FileSavePicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeChoices.Add(fileType, [suggestedName]);
        picker.SuggestedFileName = suggestedName;
        var file = await picker.PickSaveFileAsync();
        return file?.Path;
    }

    private async Task<string?> PickOpenFileAsync(string fileType, string[] extensions)
    {
        var picker = new FileOpenPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        foreach (var ext in extensions)
            picker.FileTypeFilter.Add(ext);
        var file = await picker.PickSingleFileAsync();
        return file?.Path;
    }

    private async void ExportQueue_Click(object sender, RoutedEventArgs e)
    {
        var path = await PickSaveFileAsync("JSON File", "robocopy_queue.json");
        if (path is not null)
            await (GetViewModel()?.ExportQueueCommand.ExecuteAsync(path) ?? Task.CompletedTask);
    }

    private async void ImportQueue_Click(object sender, RoutedEventArgs e)
    {
        var path = await PickOpenFileAsync("JSON File", [".json"]);
        if (path is not null)
            await (GetViewModel()?.ImportQueueCommand.ExecuteAsync(path) ?? Task.CompletedTask);
    }

    private async void ExportBat_Click(object sender, RoutedEventArgs e)
    {
        var path = await PickSaveFileAsync("Batch File", "robocopy_tasks.bat");
        if (path is not null)
            await (GetViewModel()?.ExportBatCommand.ExecuteAsync(path) ?? Task.CompletedTask);
    }

    private async void ExportPs1_Click(object sender, RoutedEventArgs e)
    {
        var path = await PickSaveFileAsync("PowerShell Script", "robocopy_tasks.ps1");
        if (path is not null)
            await (GetViewModel()?.ExportPowerShellCommand.ExecuteAsync(path) ?? Task.CompletedTask);
    }

    private async void ExportPresets_Click(object sender, RoutedEventArgs e)
    {
        var path = await PickSaveFileAsync("JSON File", "robocopy_presets.json");
        if (path is not null)
            await (GetViewModel()?.ExportPresetsCommand.ExecuteAsync(path) ?? Task.CompletedTask);
    }

    private async void ImportPresets_Click(object sender, RoutedEventArgs e)
    {
        var path = await PickOpenFileAsync("JSON File", [".json"]);
        if (path is not null)
            await (GetViewModel()?.ImportPresetsCommand.ExecuteAsync(path) ?? Task.CompletedTask);
    }
}
