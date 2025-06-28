using HamsterStudio.RedBook.Interfaces;
using HamsterStudio.RedBook.Services;
using HamsterStudio.RedBook.Services.Parsing;
using HamsterStudio.RedBook.Services.XhsRestful;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace HamsterStudio.RedBook;

public static class WebApiExtensions
{
    public static IServiceCollection AddRedBookWebApiServices(this IServiceCollection services)
    {
        services.AddSingleton<IRedBookParser, RedBookNoteParser>();

        var handler = new RebuildUriHandler(new HttpClientHandler());
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://ci.xiaohongshu.com")
        };
        services.AddSingleton(RestService.For<IPngService>(client));

        services.AddRefitClient<IWebpService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://sns-img-bd.xhscdn.com/"));
        services.AddRefitClient<IVideoService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://sns-video-bd.xhscdn.com/"));

        services.AddSingleton<NoteDownloadService>();
        
        return services;
    }
}
