using HamsterStudio.Barefeet.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows;

namespace HamsterStudio.Photogrammetry
{
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
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var home = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HamsterStudio",
                "Photogrammetry");

            Services.AddSingleton(sp => new DirectoryMgmt(home, sp.GetService<ILogger>()));
            Services.AddSingleton<DataStorageMgmt>();

            PhotogrammetryProfile.RegisterServices(Services);
            ServiceProvider = Services.BuildServiceProvider();
            base.OnStartup(e);
        }
    }

}
