using Microsoft.Extensions.DependencyInjection;
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
            PhotogrammetryProfile.RegisterServices(Services);
            ServiceProvider = Services.BuildServiceProvider();
            base.OnStartup(e);
        }
    }

}
