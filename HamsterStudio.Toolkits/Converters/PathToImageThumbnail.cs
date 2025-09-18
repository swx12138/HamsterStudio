using System.Globalization;
using System.Windows.Data;

namespace HamsterStudio.Toolkits.Converters;

public class PathToImageThumbnail : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ImageUtils.LoadThumbnail (value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
