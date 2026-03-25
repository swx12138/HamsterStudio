using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Gallery.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.Gallery;

partial class MainWindowModel : ViewModel
{
    public GalleryViewModel2 ViewModel { get; }

    [ObservableProperty]
    private bool _topmost = false;

    public MainWindowModel() : base(null)
    {
        logger = App.ServiceProvider?.GetRequiredService<ILogger<MainWindowModel>>()!;
        ViewModel = App.ServiceProvider?.GetRequiredService<GalleryViewModel2>()!;

    }

}
