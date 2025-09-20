using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.FileSystem.Filters;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Gallery.Models;
using System.ComponentModel;
using System.Windows;

namespace HamsterStudio.Gallery.ViewModels;

public partial class GalleryViewModel : ViewModel
{
    public FileManagerViewModel FileManager { get; } = new();

    [ObservableProperty]
    private Visibility _galleriesVisible = Visibility.Visible;

    [ObservableProperty]
    private Visibility _galleryVisible = Visibility.Visible;

    public GalleryViewModel()
    {
        FileManager.Filters.Add(new ImageFileFilter());
    }
}
