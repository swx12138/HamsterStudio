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

        services.AddSingleton(RestService.For<IWeiboMediaApi>(new HttpClient()
        {
            BaseAddress = new Uri("https://wx3.sinaimg.cn")
        }));

        services.AddSingleton<DownloadService>();

        return services;
    }
}
