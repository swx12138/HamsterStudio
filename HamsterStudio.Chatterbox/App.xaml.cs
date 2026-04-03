using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace HamsterStudio.Chatterbox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceCollection Services;
        public IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            Services = new ServiceCollection();
            Services.AddLogging();
            ChatterboxProfile.RegisterService(Services);
            ServiceProvider = Services.BuildServiceProvider();
        }


    }

}
