using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Gallery.Services;
using HamsterStudio.Gallery.ViewModels;
using HamsterStudio.Toolkits.Logging;
using HamsterStudio.Toolkits.Services;
using HamsterStudio.WallpaperEngine.ViewModels;
using HamsterStudioGUI.ViewModels;

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

    public ObservableCollectionTarget NlogTarget { get; } = new("App");

    public MainViewModel MainViewModel { get; } = new();
    public WallpaperEngineViewModel  WallpaperEngineViewModel { get; } 
    public GalleryViewModel GalleryViewModel { get; }
    public SpacialDownloadsViewModel SpacialDownloadsViewModel { get; } = new();

    [ObservableProperty]
    private ThemeMgmt _ThemeMgmt;

    public MainWindowModel()
    {
        Logger.Shared.AddTarget(NlogTarget, NLog.LogLevel.Info, NLog.LogLevel.Fatal);

        ThemeMgmt = App.ResloveService<ThemeMgmt>();
        GalleryViewModel = new(App.ResloveService<GalleriaFileMgmt>().FileManager, ThemeMgmt);
        WallpaperEngineViewModel = new(App.ResloveService<ImageMetaInfoReadService>(), ThemeMgmt);
    }

    public void Dispose()
    {
        App.ResloveService<DataStorageMgmt>().Persist();
        WallpaperEngineViewModel.Configuration.Dispose();
    }

}
