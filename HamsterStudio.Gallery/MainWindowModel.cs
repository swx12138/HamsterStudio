using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Gallery.ViewModels;
using HamsterStudio.Gallery.Views;
using Microsoft.Win32;
using System.Windows.Input;

namespace HamsterStudio.Gallery;

partial class MainWindowModel :ViewModel
{
    public GalleryView MainView { get; } = new();
    public GalleryViewModel ViewModel => (MainView.DataContext as GalleryViewModel)!;

    [ObservableProperty]
    private bool _topmost = false;

    public MainWindowModel()
    {

    }

}
