using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.FileSystem;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;

namespace HamsterStudio.Gallery.Models;

public partial class SearchableGalleryFolderModel : GalleryFolderModel
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    public CollectionViewSource SearchedViewSource { get; }

    public ILogger? Logger { get; set; }

    public SearchableGalleryFolderModel(DirectoryInfo di, ILogger? logger = null) : base(di, logger)
    {
        Logger = logger;

        SearchedViewSource = new();
        SearchedViewSource.Source = Files;
        SearchedViewSource.Filter += (sender, e) =>
        {
            if (e.Item is not HamstertFileInfo file)
            {
                e.Accepted = false;
                return;
            }
            int index = Files.IndexOf(file);
            if (index < _pageIndex * _pageSize || index >= (_pageIndex + 1) * _pageSize)
            {
                e.Accepted = false;
                return;
            }

            if (SearchText == string.Empty)
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = file.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
            }

            if (e.Accepted == true)
            {
                Logger?.LogInformation($"Accepted {file.Name}, index:{index}");
            }

        };
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchText))
        {
            SearchedViewSource.View.Refresh();
        }
        base.OnPropertyChanged(e);
    }

    private int _pageSize = 0;
    private int _pageIndex = 0;

    public void UpdateViewRange(int cols, int rows, int pageIndex)
    {
        _pageSize = cols * rows;
        _pageIndex = pageIndex - 1;
        SearchedViewSource.View.Refresh();
    }

    public void CopyFrom(GalleryFolderModel model)
    {
        Files = model.Files;
        SearchedViewSource.Source = Files;

        Folders = model.Folders;
        DirInfo = model.DirInfo;
    }

}
