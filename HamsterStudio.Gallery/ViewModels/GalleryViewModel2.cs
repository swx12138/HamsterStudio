using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Gallery.Models;
using HamsterStudio.Toolkits;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;

namespace HamsterStudio.Gallery.ViewModels;

public partial class GalleryViewModel2 : KnownViewModel
{
    [ObservableProperty]
    private GalleryFolderModel _galleryFolders = new(new DirectoryInfo("Root"));

    [ObservableProperty]
    private int _MaxPageCount = 0;

    [ObservableProperty]
    private int _DataCountPerPage;

    [ObservableProperty]
    private int _PageIndex = 0;

    [ObservableProperty]
    private int _Columns = 6;

    [ObservableProperty]
    private int _Rows = 5;

    [ObservableProperty]
    private SearchableGalleryFolderModel _currentFolder= new(new DirectoryInfo(Environment.CurrentDirectory));

    [ObservableProperty]
    private bool _AutoSwitchPage = false;

    [ObservableProperty]
    private int _AutoSwitchPageMinDelay = 3999;

    public GalleryViewModel2(ILogger<GalleryViewModel2> logger) : base(logger)
    {
        CurrentFolder.Logger = logger;
        //GalleryFolders.Add(
        //    GalleryFolderModel.LoadFolder(new DirectoryInfo(@"E:\Pictures\00_瞎拍\04_Cosplay")));
        GalleryFolders.Folders.Add(
            GalleryFolderModel.LoadFolder(new DirectoryInfo(@"E:\Pictures\Boundhub_Album")));
        GalleryFolders.Folders.Add(
            GalleryFolderModel.LoadFolder(new DirectoryInfo(@"E:\HamsterStudioHome\xiaohongshu")));

        UpdateDataCountPerPage();
        OnPageIndexChanged();

        Task.Run(async () =>
        {
            while (1 > 0)
            {
                if (AutoSwitchPage)
                {
                    if (PageIndex < MaxPageCount)
                    {

                        PageIndex++;
                    }
                    else
                    {
                        PageIndex = 1;
                    }
                }
                await Task.Delay(Math.Max(Columns * Rows * 200, AutoSwitchPageMinDelay));
            }
        });

    }

    void UpdateMaxPageCount()
    {
        MaxPageCount = (CurrentFolder.Files.Count / DataCountPerPage) + (CurrentFolder.Files.Count % DataCountPerPage == 0 ? 0 : 1);
    }

    void UpdateDataCountPerPage()
    {
        DataCountPerPage = Rows * Columns;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DataCountPerPage))
        {
            UpdateMaxPageCount();
            CurrentFolder.UpdateViewRange(Columns, Rows, PageIndex);
        }
        else if (e.PropertyName == nameof(PageIndex))
        {
            OnPageIndexChanged();
        }
        else if (e.PropertyName == nameof(Columns) || e.PropertyName == nameof(Rows))
        {
            UpdateDataCountPerPage();
        }
        base.OnPropertyChanged(e);
    }

    public void OnSelectedFolderChanged(GalleryFolderModel newItem)
    {
        using (CurrentFolder.SearchedViewSource.DeferRefresh())
        {
            CurrentFolder.CopyFrom(newItem);
            UpdateMaxPageCount();
            if (PageIndex != 1)
            {
                PageIndex = 1;
            }
            else
            {
                OnPageIndexChanged();
            }
        }
    }

    public void OnPageIndexChanged()
    {
        CurrentFolder.UpdateViewRange(Columns, Rows, PageIndex);
    }

}
