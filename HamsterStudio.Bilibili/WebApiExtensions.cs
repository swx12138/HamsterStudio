using HamsterStudio.Bilibili.Services;
using HamsterStudio.Bilibili.Services.Restful;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace HamsterStudio.Bilibili;

public static class WebApiExtensions
{
    public static IServiceCollection AddBilibiliWebApiServices(this IServiceCollection services)
    {
        services.AddSingleton<DownloadService>();
        services.AddSingleton<BiliApiClient>();

        services.AddSingleton(CreateServ());

        return services;
    }

    public static IBilibiliApiService CreateServ()
    {
        var handler = new HttpClientHandler();
        var bhandler = new BpiHandler(handler);
        return RestService.For<IBilibiliApiService>(new HttpClient(bhandler)
        {
            BaseAddress = new Uri("https://api.bilibili.com")
        });
    }
}
