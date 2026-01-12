using HamsterStudio.Barefeet.Logging;
using HamsterStudio.WebApi;
using HamsterStudioGUI.Extensions;
using HamsterStudioGUI.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;

namespace HamsterStudioGUI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly int httpPortNumber = 5000;

    public int HttpPortNumber => httpPortNumber;
    public int HttpsPortNumber => httpPortNumber + 1;

    public App()
    {
        //AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
        //{
        //    Debug.WriteLine($"异常捕获: {e.Exception}");
        //};
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        InitializeWebApi();
    }

    internal static WebApplication WebApiService { get; private set; }

    private void InitializeWebApi()
    {
        var builder = WebApplication
#if DEBUG
            .CreateBuilder(new WebApplicationOptions() { EnvironmentName = Environments.Development });
#else
            .CreateBuilder();
#endif

        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.ConfigureHttpsDefaults(httpsOptions =>
            {
                httpsOptions.ServerCertificate = new X509Certificate2(
                    "https/localhost.pfx",
                    File.ReadAllText("https/password.txt"));
            });
        });
        builder.Services.AddWebApiServices();
        builder.Services.ConfigureService(@"E:\HamsterStudioHome");
        builder.WebHost.UseUrls(
            $"http://0.0.0.0:{HttpPortNumber}",
            $"https://0.0.0.0:{HttpsPortNumber}");   // 更改监听地址

        WebApiService = builder.Build();
        WebApiService.ConfigureWebApi()
            .ConfigureStaticFiles(new StaticFilePathParam() { PhyPath = @"E:\HamsterStudioHome\_XHS_", ReqPath = "/static/xhs" })
            .ConfigureImageMetaInfoReadService();

        //var logger = WebApiService.Services.GetRequiredService<ILogger<App>>();
        //logger.LogInformation($"Web API Service listening on ports {HttpPortNumber} (HTTP) and {HttpsPortNumber} (HTTPS)");

        WebApiService.RunAsync();
    }

    protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
    {
        base.OnSessionEnding(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        WebApiService.StopAsync();

        ResloveService<HamsterStudio.RedBook.Services.FileMgmt>().Save();

        base.OnExit(e);
    }

    internal static T ResloveService<T>()
    {
        return WebApiService!.Services!.GetService<T>()!;
    }

}

