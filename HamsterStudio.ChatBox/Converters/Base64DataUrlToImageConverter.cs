using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace HamsterStudio.ChatBox.Converters;

/// <summary>把图片 data URL（data:image/...;base64,...）转换为 BitmapImage。</summary>
public class Base64DataUrlToImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string dataUrl || string.IsNullOrWhiteSpace(dataUrl))
            return null;

        var base64 = ExtractBase64(dataUrl);
        if (base64 is null)
            return null;

        try
        {
            var bytes = System.Convert.FromBase64String(base64);
            using var stream = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static string? ExtractBase64(string dataUrl)
    {
        const string marker = "base64,";
        var idx = dataUrl.IndexOf(marker, StringComparison.Ordinal);
        return idx >= 0 ? dataUrl[(idx + marker.Length)..] : dataUrl;
    }
}
