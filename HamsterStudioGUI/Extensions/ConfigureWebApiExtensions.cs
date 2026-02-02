using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Gallery.Services;
using HamsterStudio.Toolkits.Services;
using HamsterStudio.Toolkits.Services.ImageMetaInfoReader;
using HamsterStudio.Web.Services;
using HamsterStudio.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System.IO;

namespace HamsterStudioGUI.Extensions;

internal static class ConfigureWebApiExtensions
{
    public static void ConfigureService(this IServiceCollection services, string home)
    {
        services.AddSingleton<ThemeMgmt>();

        services.AddSingleton<GalleriaFileMgmt>();
        services.AddSingleton<ImageMetaInfoReadService>();

        services.AddSingleton(sp => new DirectoryMgmt(home, sp.GetService<ILogger>()));
        services.AddSingleton<DataStorageMgmt>();

        services.AddSingleton<HttpClientProvider>();

    }

    private static string GetContentType(string path)
    {
        var extension = Path.GetExtension(path).ToLower();
        return extension switch
        {
            ".txt" => "text/plain",
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }

    public static WebApplication ConfigureStaticFiles(this WebApplication app, params StaticFilePathParam[] static_file_paths)
    {
        app.Logger.LogInformation("Configuring Static Files...");
        var directoryMgmt = app.Services.GetService<DirectoryMgmt>() ?? throw new NotSupportedException();

#if true
        app.Map("/static/{**path}", async context =>
        {
            var path = context.Request.Path.Value?.Substring("/static/".Length) ?? "";
            var filePath = Path.Combine(directoryMgmt.StorageHome, path);

            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                var contentType = GetContentType(filePath);
                context.Response.ContentType = contentType;
                context.Response.ContentLength = fileInfo.Length;

                await using var fileStream = File.OpenRead(filePath);
                await fileStream.CopyToAsync(context.Response.Body);
            }
            else
            {
                context.Response.StatusCode = 404;
            }
        });
#else
        app.AddStaticFiles(new StaticFilePathParam() { PhyPath = directoryMgmt.StorageHome, ReqPath = "static" });
        foreach (var static_file_path in static_file_paths)
        {
            if (!Directory.Exists(static_file_path.PhyPath))
            {
                app.Logger.LogInformation($"Path {static_file_path} not valid.");
                continue;
            }
            app.AddStaticFiles(static_file_path);
        }
#endif

        return app;
    }

    public static WebApplication ConfigureImageMetaInfoReadService(this WebApplication app)
    {
        app.Logger.LogInformation("Configuring ImageMetaInfoReadService...");
        var svc = app.Services.GetService<ImageMetaInfoReadService>() ?? throw new NotSupportedException();
        svc.ImageMetaInfoReaders.Add(new JpegImageMetaInfoReader());
        svc.ImageMetaInfoReaders.Add(new PngImageMetaInfoReader());
        svc.ImageMetaInfoReaders.Add(new WebpImageMetaInfoReader());
        return app;
    }

    private static void AddStaticFiles(this WebApplication app, StaticFilePathParam static_file_path)
    {
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

}
