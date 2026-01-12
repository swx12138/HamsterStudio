using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Gallery.ViewModels;
using HamsterStudio.Gallery.Views;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.Gallery;

partial class MainWindowModel(ILogger? logger) : ViewModel(logger)
{
    public GalleryView MainView { get; } = new();
    public GalleryViewModel ViewModel => (MainView.DataContext as GalleryViewModel)!;

    [ObservableProperty]
    private bool _topmost = false;

}
