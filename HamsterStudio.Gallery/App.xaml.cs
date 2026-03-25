using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace HamsterStudio.Gallery;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    ServiceCollection Services { get; } = new();
    public static IServiceProvider? ServiceProvider { get; private set; }

    public App()
    {
        Services.AddLogging(builder =>
        {
            builder
                .ClearProviders() // 清除默认提供程序
                .AddDebug()       // 添加调试输出
                .SetMinimumLevel(LogLevel.Information);
        });
        GalleryProfile.RegisterServices(Services);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        ServiceProvider = Services.BuildServiceProvider();
        base.OnStartup(e);
    }


}
