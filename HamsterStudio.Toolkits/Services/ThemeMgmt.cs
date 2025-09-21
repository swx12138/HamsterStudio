using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace HamsterStudio.Toolkits.Services;

public partial class ThemeMgmt : ObservableObject
{
    [ObservableProperty]
    private SolidColorBrush _HeaderBorderColor = new(Color.FromArgb(255, 200, 200, 200));

    [ObservableProperty]
    private SolidColorBrush _BodyBorderColor = new(Colors.HotPink);


}
