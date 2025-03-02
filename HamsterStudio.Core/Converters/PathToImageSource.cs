using HamsterStudio.Toolkits;
using System.Globalization;
using System.Windows.Data;

namespace HamsterStudio.Converters
{
    public class PathToImageSource : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ImageUtils.LoadImageSource(value as string) ?? null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
