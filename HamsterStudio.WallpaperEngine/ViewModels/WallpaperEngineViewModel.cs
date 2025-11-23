using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Toolkits.Services;
using HamsterStudio.WallpaperEngine.Services;

namespace HamsterStudio.WallpaperEngine.ViewModels;

public partial class WallpaperEngineViewModel : KnownViewModel
{
    [ObservableProperty]
    public WallpaperShowConfig _configuration;

    public ThemeMgmt ThemeMgmt { get; }

    public WallpaperEngineViewModel(ImageMetaInfoReadService svc, ThemeMgmt themeMgmt)
    {
        DisplayName = "壁纸预览";
        
        ThemeMgmt = themeMgmt;
        
        _configuration = new WallpaperShowConfig(svc);

    }
}
