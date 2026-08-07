using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Toolkits.Services;
using HamsterStudio.WallpaperEngine.Services;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.WallpaperEngine.ViewModels;

public partial class WallpaperEngineViewModel : KnownViewModel
{
    [ObservableProperty]
    public WallpaperShowConfig _configuration;

    public ThemeMgmt ThemeMgmt { get; }

    public WallpaperEngineViewModel(ImageMetaInfoReadService svc, ThemeMgmt themeMgmt, ILogger<WallpaperEngineViewModel> logger, DirectoryMgmt directoryMgmt) : base(logger)
    {
        DisplayName = "壁纸预览";

        ThemeMgmt = themeMgmt;

        _configuration = new WallpaperShowConfig(svc, logger, directoryMgmt);

    }
}
