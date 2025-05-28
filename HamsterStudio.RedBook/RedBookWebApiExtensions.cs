using HamsterStudio.RedBook.Services;
using HamsterStudio.RedBook.Services.XhsRestful;
using Refit;

namespace HamsterStudio.RedBook;

public static class RedBookWebApiExtensions
{
    public static IServiceCollection AddRedBookWebApiServices(this IServiceCollection services)
    {
        services.AddRefitClient<IPngService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://ci.xiaohongshu.com/"));
        services.AddRefitClient<IWebpService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://sns-img-bd.xhscdn.com/"));
        services.AddRefitClient<IVideoService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://sns-video-bd.xhscdn.com/"));
        services.AddSingleton<RedBookDownloadService>();
        return services;
    }
}
