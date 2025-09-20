using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Toolkits.Services;
using HamsterStudio.Toolkits.Services.ImageMetaInfoReader;
using HamsterStudio.Web.Services;
using HamsterStudio.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace HamsterStudioGUI.Extensions;

internal static class ConfigureWebApiExtensions
{
    public static void ConfigureService(this IServiceCollection services, string home)
    {
        services.AddSingleton<ImageMetaInfoReadService>();
        services.AddSingleton(new DirectoryMgmt(home));
        services.AddSingleton<DataStorageMgmt>();
        services.AddSingleton<HttpClientProvider>();
    }

    public static WebApplication ConfigureStaticFiles(this WebApplication app, params StaticFilePathParam[] static_file_paths)
    {
        Logger.Shared.Information("Configuring Static Files...");
        var directoryMgmt = app.Services.GetService<DirectoryMgmt>() ?? throw new NotSupportedException();
        app.AddStaticFiles(new StaticFilePathParam() { PhyPath = directoryMgmt.StorageHome, ReqPath = "static" });

        foreach (var static_file_path in static_file_paths)
        {
            if (!Directory.Exists(static_file_path.PhyPath))
            {
                Logger.Shared.Trace($"Path {static_file_path} not valid.");
                continue;
            }
            app.AddStaticFiles(static_file_path);
        }

        return app;
    }

    public static WebApplication ConfigureImageMetaInfoReadService(this WebApplication app)
    {
        Logger.Shared.Information("Configuring ImageMetaInfoReadService...");
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
