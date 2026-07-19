using Microsoft.UI.Xaml.Data;

namespace RoboCopyGUI.Converters;

public sealed class BytesToHumanConverter : IValueConverter
{
    private static readonly string[] Units = ["B", "KB", "MB", "GB", "TB"];

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not long bytes || bytes < 0)
            return "0 B";

        int unitIndex = 0;
        double size = bytes;

        while (size >= 1024 && unitIndex < Units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:F2} {Units[unitIndex]}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
