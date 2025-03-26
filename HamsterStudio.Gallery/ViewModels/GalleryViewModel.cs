using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Gallery.Models;
using HamsterStudio.Gallery.Models.Filters;
using HamsterStudio.Gallery.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
