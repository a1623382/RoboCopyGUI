using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace RoboCopyGUI.Converters;

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool flag = value is bool b && b;
        if (parameter is string s && s == "Invert")
            flag = !flag;
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        bool flag = value is Visibility v && v == Visibility.Visible;
        if (parameter is string s && s == "Invert")
            flag = !flag;
        return flag;
    }
}
