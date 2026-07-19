using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RoboCopyGUI.ViewModels;
using WinRT.Interop;

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
        SetTitleBar(TitleArea);

        TrySetMicaBackdrop();

        Closed += Window_Closed;
    }

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

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        _micaController?.Dispose();
        _micaController = null;
    }
}
