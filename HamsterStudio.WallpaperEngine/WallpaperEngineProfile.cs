using HamsterStudio.WallpaperEngine.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace HamsterStudio.WallpaperEngine;

public static class WallpaperEngineProfile
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<WallpaperEngineViewModel>();
    }
}
