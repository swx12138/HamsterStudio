using HamsterStudio.Gallery.Models;
using System.Globalization;
using System.Windows.Data;

namespace HamsterStudio.Gallery.Converters;

internal class GalleryFolderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is GalleryFolderModel folder)
        {
            return $"{folder.DirInfo.Name} [{folder.Files.Count}]";
        }
        return nameof(GalleryFolderConverter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
