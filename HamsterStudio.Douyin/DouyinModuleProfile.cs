using HamsterStudio.Douyin.Services;
using HamsterStudio.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HamsterStudio.Douyin;

public static class DouyinModuleProfile
{
    public static IServiceCollection ResisterDouyinService(this IServiceCollection services)
    {
        services.AddSingleton(sp => new DouyinResourcesDownloadService(
            sp.GetRequiredService<CommonDownloader>(),
            new Barefeet.FileSystem.FileCountGroupr()));
        return services;
    }
}
