using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using RoboCopyGUI.Models;

namespace RoboCopyGUI.Converters;

public sealed class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not CopyTaskStatus status)
            return new SolidColorBrush(Colors.Gray);

        return status switch
        {
            CopyTaskStatus.Pending => new SolidColorBrush(Colors.Gray),
            CopyTaskStatus.Running => new SolidColorBrush(Colors.DodgerBlue),
            CopyTaskStatus.Completed => new SolidColorBrush(Colors.LimeGreen),
            CopyTaskStatus.Failed => new SolidColorBrush(Colors.OrangeRed),
            CopyTaskStatus.Cancelled => new SolidColorBrush(Colors.Orange),
            CopyTaskStatus.Paused => new SolidColorBrush(Colors.Yellow),
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
