using System.Globalization;
using System.Windows.Data;

namespace HamsterStudio.Toolkits.Converters;

public class PathToImageSource : IValueConverter
{
    public uint DecodeWidth { get; set; } = 0;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ImageUtils.LoadImageSource(value as string, DecodeWidth) ?? value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
