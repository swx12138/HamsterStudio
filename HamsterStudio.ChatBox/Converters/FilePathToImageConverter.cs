using System.Globalization;
using System.IO;
using System.Windows.Data;
using HamsterStudio.ChatBox.Services;

namespace HamsterStudio.ChatBox.Converters;

/// <summary>把图片文件路径转换为缩略图 BitmapImage。</summary>
public class FilePathToImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;
        return ImageFileHelper.LoadThumbnail(path);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
