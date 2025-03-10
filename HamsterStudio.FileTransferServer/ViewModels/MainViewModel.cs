using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using HamsterStudio.Barefeet.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HamsterStudio.FileTransferServer.ViewModels;

partial class MainViewModel : ObservableObject
{
    private IFtpServerHost ftpServerHost;

    private BackgroundWorker Worker { get; } = new();

    public ICommand StartCommand { get; }

    public ICommand StopCommand { get; }

    public MainViewModel()
    {
        {
            // Setup dependency injection
            var services = new ServiceCollection();

            // use %TEMP%/TestFtpServer as root folder
            services.Configure<DotNetFileSystemOptions>(opt => opt
                .RootPath = Path.Combine(Path.GetTempPath(), "TestFtpServer"));

            // Add FTP server services
            // DotNetFileSystemProvider = Use the .NET file system functionality
            // AnonymousMembershipProvider = allow only anonymous logins
            services.AddFtpServer(builder => builder
                .UseDotNetFileSystem() // Use the .NET file system functionality
                .EnableAnonymousAuthentication()); // allow anonymous logins

            // Configure the FTP server
            services.Configure<FtpServerOptions>(opt => opt.ServerAddress = "127.0.0.1");

            // Build the service provider
            using var serviceProvider = services.BuildServiceProvider();

            // Initialize the FTP server
            ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();
        }

        Worker.WorkerSupportsCancellation = true;
        Worker.DoWork += Worker_DoWork;

        StartCommand = new RelayCommand(() =>
        {
            if(!Worker.IsBusy)
            {
                Worker.RunWorkerAsync();
            }
            else
            {
                Logger.Shared.Warning("Ftp server is already running.");
            }
        });

        StopCommand = new RelayCommand(() =>
        {
            if (Worker.IsBusy)
            {
                Worker.CancelAsync();
            }
            else
            {
                Logger.Shared.Warning("Ftp server has not been started.");
            }
        });

    }

    private async void Worker_DoWork(object? sender, DoWorkEventArgs e)
    {
        // Start the FTP server
        await ftpServerHost.StartAsync();

        Console.WriteLine("Press ENTER/RETURN to close the test application.");
        Console.ReadLine();

        // Stop the FTP server
        await ftpServerHost.StopAsync();

    }
}
