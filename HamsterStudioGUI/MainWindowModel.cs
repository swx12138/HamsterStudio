using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Gallery.Services;
using HamsterStudio.Gallery.ViewModels;
using HamsterStudio.Toolkits.Logging;
using HamsterStudio.Toolkits.Services;
using HamsterStudio.WallpaperEngine.ViewModels;
using HamsterStudioGUI.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;

namespace HamsterStudioGUI;

partial class MainWindowModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    private string _title = "Hamster Studio GUI";

    [ObservableProperty]
    private string _description = "Hamster Studio GUI is a desktop application for Hamster Studio.";

    [ObservableProperty]
    private bool _topmost = false;

    public string UserName => Environment.UserName;

    //public ObservableCollectionTarget NlogTarget { get; } = new("App");
    [ObservableProperty]
    private LogViewModel _logViewModel;

    public MainViewModel MainViewModel { get; } = new();
    public WallpaperEngineViewModel  WallpaperEngineViewModel { get; } 
    public GalleryViewModel GalleryViewModel { get; }
    public SpacialDownloadsViewModel SpacialDownloadsViewModel { get; }

    [ObservableProperty]
    private ThemeMgmt _ThemeMgmt;

    public MainWindowModel()
    {
        //Logger.Shared.AddTarget(NlogTarget, NLog.LogLevel.Info, NLog.LogLevel.Fatal);

        LogViewModel = App.ResloveService<LogViewModel>();

        ThemeMgmt = App.ResloveService<ThemeMgmt>();
        var loggerFactory = App.ResloveService<ILoggerFactory>();

        GalleryViewModel = new(App.ResloveService<GalleriaFileMgmt>().FileManager, ThemeMgmt, App.ResloveService<ILogger<GalleryViewModel>>());
        SpacialDownloadsViewModel = new(loggerFactory);
        WallpaperEngineViewModel = new(
            App.ResloveService<ImageMetaInfoReadService>(),
            ThemeMgmt,
            loggerFactory.CreateLogger<WallpaperEngineViewModel>());

        LogViewModel.ClearLogs();
        App.WebApiService.Logger.LogInformation("Ready.");
    }

    private void ClearLogs_Click(object sender, RoutedEventArgs e)
    {
        LogViewModel.ClearLogs();
    }

    private void ExportLogs_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "日志文件|*.log|所有文件|*.*",
            DefaultExt = ".log",
            FileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}.log"
        };

        if (dialog.ShowDialog() == true)
        {
            LogViewModel.ExportToFile(dialog.FileName);
            MessageBox.Show($"日志已导出到: {dialog.FileName}", "成功",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public void Dispose()
    {
        App.ResloveService<DataStorageMgmt>().Persist();
        WallpaperEngineViewModel.Configuration.Dispose();
    }

}
