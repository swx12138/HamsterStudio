using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Bilibili;
using HamsterStudio.RedBook;
using HamsterStudio.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.FileProviders;

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
        services.AddRedBookWebApiServices()
            .AddBilibiliWebApiServices();

        services.AddControllers()
            .ConfigureApplicationPartManager(apm =>
            {
                apm.ApplicationParts.Add(new AssemblyPart(typeof(RedBookController).Assembly));
            });// 添加特定的程序集

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        return services;
    }

    public static WebApplication ConfigureWebApi(this WebApplication app, params StaticFilePathParam[] static_file_paths)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        foreach (var static_file_path in static_file_paths)
        {
            if (!Directory.Exists(static_file_path.PhyPath))
            {
                Logger.Shared.Trace($"Path {static_file_path} not valid.");
                continue;
            }
            var fileProvider = new PhysicalFileProvider(static_file_path.PhyPath);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                RequestPath = $"/{static_file_path.ReqPath}"
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = fileProvider,
                RequestPath = $"/{static_file_path.ReqPath}"
            });
        }

        //app.UseHttpsRedirection();
        app.UseCors("AllowAll"); // 启用 CORS

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

}
