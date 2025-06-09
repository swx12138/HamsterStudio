using HamsterStudio.Barefeet.Services;
using HamsterStudio.Bilibili.Services;
using HamsterStudio.Bilibili.Services.Restful;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Bilibili;

public static class WebApiExtensions
{
    public static IServiceCollection AddBilibiliWebApiServices(this IServiceCollection services)
    {
        services.AddSingleton<DownloadService>();
        services.AddSingleton<BiliApiClient>();

        services.AddSingleton(RestService.For<IBilibiliApiService>(new HttpClient()
        {
            BaseAddress = new Uri("https://api.bilibili.com")
        }));

        return services;
    }
}
