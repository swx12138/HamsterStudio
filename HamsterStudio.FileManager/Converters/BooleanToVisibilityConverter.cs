using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HamsterStudio.FileManager.Converters
{
    /// <summary>
    /// Boolean 转 Visibility 转换器
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;
            if (parameter is string p && p.Equals("Invert", StringComparison.OrdinalIgnoreCase))
                boolValue = !boolValue;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Visible;
        }
    }
}
