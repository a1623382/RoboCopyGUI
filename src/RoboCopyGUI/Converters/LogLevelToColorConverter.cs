using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using RoboCopyGUI.Models;

namespace RoboCopyGUI.Converters;

public sealed class LogLevelToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not LogLevel level)
            return new SolidColorBrush(Colors.White);

        return level switch
        {
            LogLevel.Info => new SolidColorBrush(Colors.White),
            LogLevel.Warning => new SolidColorBrush(Colors.Gold),
            LogLevel.Error => new SolidColorBrush(Colors.OrangeRed),
            LogLevel.Success => new SolidColorBrush(Colors.LimeGreen),
            _ => new SolidColorBrush(Colors.White)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
