using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Gallery.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace HamsterStudio.Gallery.ViewModels;

public partial class GalleryViewModel2 : KnownViewModel
{
    [ObservableProperty]
    private GalleryFolderModel _galleryFolders = new(new DirectoryInfo("Root"));

    public ThumbnailModeViewModel ThumbnailModeViewModel { get; }
    public LargeImageViewModel LargeImageViewModel { get; private set; }

    [ObservableProperty]
    public object _ContentDataContext;

    public ICommand LoadFolderCommand { get; }
    public ICommand RemoveFolderCommand { get; }

    [ObservableProperty]
    private bool _isThumbnailView = true;

    public GalleryViewModel2(ILogger<GalleryViewModel2> logger, ThumbnailModeViewModel thumbnail, LargeImageViewModel largeImage) : base(logger)
    {
        ThumbnailModeViewModel = thumbnail;
        LargeImageViewModel = largeImage;

        DisplayName = "相册";

        LoadFolderCommand = new RelayCommand<string>(dir =>
        {
            if (dir == null || !Directory.Exists(dir))
            {
                logger.LogWarning($"Directory {dir} does not exist.");
                return;
            }
            LoadFolder(dir);
        });
        RemoveFolderCommand = new RelayCommand<GalleryFolderModel>(folder =>
        {
            if (folder == null)
            {
                logger.LogWarning($"No folder selected.");
                return;
            }

            GalleryFolders.Folders.Remove(folder);
            if (ThumbnailModeViewModel.CurrentFolder.DirInfo.FullName == folder.DirInfo.FullName)
            {
                if (GalleryFolders.Folders.Count > 0)
                {
                    ThumbnailModeViewModel.OnSelectedFolderChanged(GalleryFolders.Folders[0]);
                }
                else
                {
                    ThumbnailModeViewModel.ClearCache();
                }
            }
        });

        ThumbnailModeViewModel.UpdateDataCountPerPage();

#if DEBUG
        using (ThumbnailModeViewModel.CurrentFolder.SearchedViewSource.DeferRefresh())
        {
            //GalleryFolders.Add(
            //    GalleryFolderModel.LoadFolder(new DirectoryInfo(@"E:\Pictures\00_瞎拍\04_Cosplay")));
            LoadFolder(@"E:\HamsterStudioHome\xiaohongshu"); 
            LoadFolder(@"E:\Pictures\Boundhub_Album");
        }
#endif

        ThumbnailModeViewModel.OnPageIndexChanged();
        ThumbnailModeViewModel.StartAutoSwitchPage();

        ContentDataContext = ThumbnailModeViewModel;
    }

    public void LoadFolder(DirectoryInfo di)
    {
        if (GalleryFolders.Folders.Any(f => f.DirInfo.FullName == di.FullName))
        {
            var existFolder = GalleryFolders.Folders.First(f => f.DirInfo.FullName == di.FullName);
            ThumbnailModeViewModel.OnSelectedFolderChanged(existFolder);
        }
        else
        {
            var newFolder = GalleryFolderModel.LoadFolder(di);
            GalleryFolders.Folders.Add(newFolder);
            ThumbnailModeViewModel.OnSelectedFolderChanged(newFolder);
        }
    }

    public void LoadFolder(string dir)
    {
        LoadFolder(new DirectoryInfo(dir));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(IsThumbnailView))
        {
            ContentDataContext = IsThumbnailView ? ThumbnailModeViewModel : LargeImageViewModel;
        }
        base.OnPropertyChanged(e);
    }

}
