using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Gallery.Models;
using HamsterStudio.Toolkits;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace HamsterStudio.Gallery.ViewModels;

public partial class GalleryViewModel2 : KnownViewModel
{
    [ObservableProperty]
    private ObservableCollection<GalleryFolderModel> _galleryFolders = [];

    [ObservableProperty]
    private ObservableCollection<ImageSource> _CurrentImages = [];

    [ObservableProperty]
    private int _MaxPageCount = 0;

    [ObservableProperty]
    private int _DataCountPerPage = 30;

    [ObservableProperty]
    private int _PageIndex = 0;

    [ObservableProperty]
    private int _Columns = 6;

    [ObservableProperty]
    private int _Rows = 5;

    [ObservableProperty]
    private GalleryFolderModel _currentFolder = new(new DirectoryInfo(Environment.CurrentDirectory)) { };

    public GalleryViewModel2(ILogger<GalleryViewModel2> logger) : base(logger)
    {
        GalleryFolders.Add(
            GalleryFolderModel.LoadFolder(new DirectoryInfo(@"E:\Pictures\00_瞎拍\04_Cosplay")));
        GalleryFolders.Add(
            GalleryFolderModel.LoadFolder(new DirectoryInfo(@"E:\Pictures\Boundhub_Album")));

    }

    public void OnSelectedFolderChanged(GalleryFolderModel newItem)
    {
        CurrentFolder = newItem;
        MaxPageCount = (newItem.Files.Count / DataCountPerPage) + (newItem.Files.Count % DataCountPerPage == 0 ? 0 : 1);
        PageIndex = 1;
        OnPageIndexChanged();
    }

    public void OnPageIndexChanged()
    {
        var files = CurrentFolder.Files.Skip((PageIndex - 1) * DataCountPerPage).Take(DataCountPerPage);
        CurrentImages.Clear();
        foreach (var file in files)
        {
            var imgSrc = ImageUtils.LoadImageSource(file.FullName, 400);
            if (imgSrc == null)
            {
                logger?.LogError($"Load image {file.Name} failed!!!");
                continue;
            }
            CurrentImages.Add(imgSrc);
        }
    }

}
