using Microsoft.UI.Xaml.Data;
using RoboCopyGUI.Models;

namespace RoboCopyGUI.Converters;

public sealed class StatusToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not CopyTaskStatus status)
            return "Unknown";

        return status switch
        {
            CopyTaskStatus.Pending => "Pending",
            CopyTaskStatus.Running => "Running",
            CopyTaskStatus.Completed => "Completed",
            CopyTaskStatus.Failed => "Failed",
            CopyTaskStatus.Cancelled => "Cancelled",
            CopyTaskStatus.Paused => "Paused",
            _ => "Unknown"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
