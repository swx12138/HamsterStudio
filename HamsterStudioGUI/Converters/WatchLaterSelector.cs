using HamsterStudio.Bilibili.Models;
using System.Globalization;
using System.Windows.Data;

namespace HamsterStudioGUI.Converters
{
    class WatchLaterSelector : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WatchLaterData data)
            {
                return data.List.Select(x => x.Bvid);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
