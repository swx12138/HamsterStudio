using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.FileSystem.Filters;
using System.Windows;

namespace HamsterStudio.Gallery.ViewModels;

public partial class GalleryViewModel : ObservableObject
{
    public FileManagerModel FileManager { get; } = new();

    [ObservableProperty]
    private Visibility _galleriesVisible = Visibility.Visible;

    [ObservableProperty]
    private Visibility _galleryVisible = Visibility.Visible;

    public GalleryViewModel()
    {
        FileManager.Filters.Add(new ImageFileFilter());


    }
}
