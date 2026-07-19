using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RoboCopyGUI.ViewModels;
using WinRT;
using WinRT.Interop;
using Windows.Storage.Pickers;

namespace RoboCopyGUI;

public sealed partial class MainWindow : Window
{
    private MicaController? _micaController;
    private SystemBackdropConfiguration? _backdropConfiguration;
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        Title = "RoboCopy GUI";

        _viewModel = new MainViewModel();
        MainGrid.DataContext = _viewModel;

        ExtendsContentIntoTitleBar = true;

        TrySetMicaBackdrop();

        Closed += Window_Closed;
    }

    private void TrySetMicaBackdrop()
    {
        try
        {
            if (!MicaController.IsSupported())
                return;

            _backdropConfiguration = new SystemBackdropConfiguration
            {
                Theme = SystemBackdropTheme.Dark
            };

            _micaController = new MicaController();
            _micaController.SetSystemBackdropConfiguration(_backdropConfiguration);

            _micaController.AddSystemBackdropTarget(
                this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
        }
        catch
        {
            _micaController?.Dispose();
            _micaController = null;
        }
    }

    private void AddTask_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SourcePath = SourceBox.Text;
        _viewModel.DestinationPath = DestBox.Text;
        _viewModel.AddTaskCommand.Execute(null);
        SourceBox.Text = string.Empty;
        DestBox.Text = string.Empty;
    }

    private void StartQueue_Click(object sender, RoutedEventArgs e)
        => _viewModel.StartQueueCommand.Execute(null);

    private void CancelQueue_Click(object sender, RoutedEventArgs e)
        => _viewModel.CancelQueueCommand.Execute(null);

    private void SaveQueue_Click(object sender, RoutedEventArgs e)
        => _viewModel.SaveQueueCommand.Execute(null);

    private void MoveUp_Click(object sender, RoutedEventArgs e)
        => _viewModel.MoveTaskUpCommand.Execute(_viewModel.SelectedTask);

    private void MoveDown_Click(object sender, RoutedEventArgs e)
        => _viewModel.MoveTaskDownCommand.Execute(_viewModel.SelectedTask);

    private void Duplicate_Click(object sender, RoutedEventArgs e)
        => _viewModel.DuplicateTaskCommand.Execute(_viewModel.SelectedTask);

    private void Remove_Click(object sender, RoutedEventArgs e)
        => _viewModel.RemoveTaskCommand.Execute(_viewModel.SelectedTask);

    private async void Settings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var panel = new Controls.SettingsPanel
            {
                DataContext = _viewModel,
                Settings = _viewModel.Settings
            };

            var dialog = new ContentDialog
            {
                Title = "设置",
                Content = panel,
                CloseButtonText = "关闭",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = MainGrid.XamlRoot
            };

            await dialog.ShowAsync();
        }
        catch { }
    }

    private async void BrowseSource_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var folder = await PickFolderAsync();
            if (folder is not null)
            {
                SourceBox.Text = folder.Path;
                _viewModel.SourcePath = folder.Path;
            }
        }
        catch { }
    }

    private async void BrowseDest_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var folder = await PickFolderAsync();
            if (folder is not null)
            {
                DestBox.Text = folder.Path;
                _viewModel.DestinationPath = folder.Path;
            }
        }
        catch { }
    }

    private async Task<Windows.Storage.StorageFolder?> PickFolderAsync()
    {
        var picker = new FolderPicker();
        InitializeWithWindow.Initialize(picker, GetWindowHandle());
        picker.FileTypeFilter.Add("*");
        return await picker.PickSingleFolderAsync();
    }

    private IntPtr GetWindowHandle()
    {
        return WindowNative.GetWindowHandle(this);
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        _micaController?.Dispose();
        _micaController = null;
    }
}
