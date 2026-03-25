using HamsterStudio.Gallery.Services;
using HamsterStudio.Gallery.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace HamsterStudio.Gallery;

public static class GalleryProfile
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<GalleriaFileMgmt>();

        //services.AddTransient<GalleryViewModel>();
        services.AddTransient<GalleryViewModel2>();
        services.AddTransient<ThumbnailModeViewModel>();
        services.AddTransient<LargeImageViewModel>();

    }


}
