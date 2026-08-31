using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace HamsterStudio.ChatBox
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
            Services.AddLogging(builder =>
            {
                builder
                    .ClearProviders()               // 清除默认提供程序
                    .AddDebug()                     // 输出到调试器（VS/VS Code 输出窗口）
                    .SetMinimumLevel(LogLevel.Debug);
            });
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ChatBoxProfile.RegisterServices(Services);
            ServiceProvider = Services.BuildServiceProvider();
            base.OnStartup(e);
        }
    }

}
