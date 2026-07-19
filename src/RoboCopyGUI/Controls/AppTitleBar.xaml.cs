using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace RoboCopyGUI.Controls;

public sealed partial class AppTitleBar : UserControl
{
    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(AppTitleBar),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TrailingContentProperty =
        DependencyProperty.Register(nameof(TrailingContent), typeof(object), typeof(AppTitleBar),
            new PropertyMetadata(null));

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public object TrailingContent
    {
        get => GetValue(TrailingContentProperty);
        set => SetValue(TrailingContentProperty, value);
    }

    public AppTitleBar()
    {
        InitializeComponent();
    }
}
