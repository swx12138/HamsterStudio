using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Toolkits.Services;
using HamsterStudio.WallpaperEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.WallpaperEngine.ViewModels;

public partial class WallpaperEngineViewModel : KnownViewModel
{
    [ObservableProperty]
    public WallpaperShowConfig _configuration;

    public WallpaperEngineViewModel(ImageMetaInfoReadService svc)
    {
        DisplayName = "壁纸预览";
        _configuration = new WallpaperShowConfig(svc);
    }
}
