using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Toolkits.Services;
using HamsterStudio.WallpaperEngine.Services;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.WallpaperEngine.ViewModels;

public partial class WallpaperEngineViewModel(
    ImageMetaInfoReadService svc, 
    ThemeMgmt themeMgmt, 
    ILogger<WallpaperEngineViewModel> logger, 
    DirectoryMgmt directoryMgmt) : KnownViewModel("壁纸预览", logger)
{
    [ObservableProperty]
    public WallpaperShowConfig _configuration = new WallpaperShowConfig(svc, logger, directoryMgmt);

    public ThemeMgmt ThemeMgmt { get; } = themeMgmt;
}
