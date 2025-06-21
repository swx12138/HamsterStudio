using HamsterStudio.SinaWeibo.Services;
using HamsterStudio.SinaWeibo.Services.Restful;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace HamsterStudio.SinaWeibo;

public static class WebApiConfigExtensions
{
    public static IServiceCollection AddWeiboServices(this IServiceCollection services)
    {

        services.AddSingleton(RestService.For<IWeiboApi>(new HttpClient()
        {
            BaseAddress = new Uri("https://weibo.com")
        }));

        services.AddSingleton<DownloadService>();

        return services;
    }
}
