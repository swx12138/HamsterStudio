using HamsterStudio.Barefeet.Constants;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.SinaWeibo.Services;
using HamsterStudio.SinaWeibo.Services.Restful;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace HamsterStudio.SinaWeibo;

public static class WebApiConfigExtensions
{
    public static IServiceCollection AddWeiboServices(this IServiceCollection services)
    {
        var loggingHandler = new LoggingHandler(new HttpClientHandler());

        HttpClient client = new(loggingHandler)
        {
            BaseAddress = new Uri("https://weibo.com"),
        };
        client.DefaultRequestHeaders.Add("Cookie", File.ReadAllText(@"auth\cookies_com_weibo.txt"));
        services.AddSingleton(RestService.For<IWeiboApi>(client));

        services.AddSingleton(RestService.For<IWeiboMediaApi>(new HttpClient(loggingHandler)
        {
            BaseAddress = new Uri("https://wx3.sinaimg.cn")
        }));

        services.AddSingleton<DownloadService>();

        return services;
    }
}
