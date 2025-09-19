using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Toolkits.Logging;
using HamsterStudio.Toolkits.Services;
using HamsterStudioGUI.Models;
using HamsterStudioGUI.ViewModels;

namespace HamsterStudioGUI;

partial class MainWindowModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Hamster Studio GUI";

    [ObservableProperty]
    private string _description = "Hamster Studio GUI is a desktop application for Hamster Studio.";

    [ObservableProperty]
    private bool _topmost = false;

    public ObservableCollectionTarget NlogTarget { get; } = new("App");

    public MainViewModel MainViewModel { get; } = new();

    public WallpaperShowConfig WallpaperShowConfigModel { get; } = new(App.ResloveService<ImageMetaInfoReadService>());

    public MainWindowModel()
    {
        Logger.Shared.AddTarget(NlogTarget, NLog.LogLevel.Info, NLog.LogLevel.Fatal);
    }

}
