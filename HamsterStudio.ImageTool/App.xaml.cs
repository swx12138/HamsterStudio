using HamsterStudio.Barefeet.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace HamsterStudio.ImageTool;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    ServiceCollection Services { get; } = new();
    public static IServiceProvider? ServiceProvider { get; set; }

    public App()
    {
        Services.AddLogging();
        Services.AddSingleton(sp => new DirectoryMgmt(@"D:\HamsterStudioHome", sp.GetService<ILogger>()));
        ImageToolProfile.RegisterServices(Services);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        ServiceProvider = Services.BuildServiceProvider();
        base.OnStartup(e);
    }

}
