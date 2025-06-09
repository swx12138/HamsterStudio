using HamsterStudio.Barefeet.Interfaces;
using HamsterStudio.Bilibili;
using HamsterStudio.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;

namespace HamsterStudioGUI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application, IHamsterApp
{
    public string FileStorageHome { get; set; } = @"D:\HamsterStudioHome";

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

        FilenameUtils.StorageHome = Path.Combine(FileStorageHome, "BVDownload");

        InitializeWebApi();
    }

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
                httpsOptions.ServerCertificate = new X509Certificate2("https/localhost.pfx", File.ReadAllText("https/password.txt"));
            });
        });
        builder.Services.AddWebApiServices();                  // 添加控制器
        builder.WebHost.UseUrls(
            $"http://0.0.0.0:{HttpPortNumber}",
            $"https://0.0.0.0:{HttpsPortNumber}");   // 更改监听地址

        var app = builder.Build();
        app.ConfigureWebApi(new StaticFilePathParam() { PhyPath = FileStorageHome, ReqPath = "static" });

        app.RunAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

    }

}

