using HamsterStudio.Bilibili;
using HamsterStudio.Douyin;
using HamsterStudio.RedBook;
using HamsterStudio.SinaWeibo;
using HamsterStudio.Web.Services;
using HamsterStudio.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace HamsterStudio.WebApi;

public record StaticFilePathParam
{
    public required string PhyPath { get; init; }
    public required string ReqPath { get; init; }
}

public static class WebApiServiceExtensions
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services)
    {
        // Add services to the container.

        services.AddSingleton<CommonDownloader>();

        services.AddRedBookWebApiServices()
            .AddBilibiliWebApiServices()
            .AddWeiboServices()
            .ResisterDouyinService();

        services.AddControllers()
            .ConfigureApplicationPartManager(apm =>
            {
                apm.ApplicationParts.Add(new AssemblyPart(typeof(WeatherForecastController).Assembly));
            });// 添加特定的程序集

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        return services;
    }

    public static WebApplication ConfigureWebApi(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //app.UseHttpsRedirection();
        app.UseCors("AllowAll"); // 启用 CORS

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

}
