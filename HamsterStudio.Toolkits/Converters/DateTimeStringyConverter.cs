using System.Globalization;
using System.Windows.Data;

namespace HamsterStudio.Toolkits.Converters;

public class DateTimeStringyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss fff");
        }
        throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
