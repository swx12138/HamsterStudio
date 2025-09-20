using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Gallery.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace HamsterStudio.Gallery.Models;

public partial class FileGroupViewModel : KnownViewModel
{
    public string GroupName => DisplayName;

    [ObservableProperty]
    private ReadOnlyCollection<HamstertFileInfo> _files = new([]);

    [ObservableProperty]
    private int _currentPageIndex = 0;

    [ObservableProperty]
    private int _pageColumns = 6;

    [ObservableProperty]
    private int _pageRows = 4;

    public int PageSize => PageColumns * PageRows;

    [ObservableProperty]
    private ICollectionView _CurrentPageView;

    public ICommand ViewCommand { get; }
    public ICommand NextPageCommand { get; }

    public FileGroupViewModel(string groupName)
    {
        DisplayName = groupName;

        BuildCurrentPageView();

        ViewCommand = new RelayCommand(() =>
        {
            FileGroupWindow window = new();
            window.DataContext = this;
            _ = window.ShowDialog();
        });
        NextPageCommand = new RelayCommand(() =>
        {
            if (CurrentPageIndex < Files.Count / PageSize)
            {
                CurrentPageIndex++;
            }
            else
            {
                CurrentPageIndex = 0;
            }
            Logger.Shared.Trace($"Page of {DisplayName} navigated to {CurrentPageIndex}");
        });
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Files))
        {
            Logger.Shared.Debug($"Files changed of {GroupName}.");

            if (Files.Count <= 1)
            {
                PageColumns = PageRows = 1;
            }
            else if (Files.Count <= 4)
            {
                PageColumns = PageRows = 2;
            }
            else if (Files.Count <= 9)
            {
                PageColumns = PageRows = 3;
            }
            else if (Files.Count <= 16)
            {
                PageColumns = PageRows = 4;
            }
            else
            {
                PageColumns = 6;
                PageRows = 4;
            }

            BuildCurrentPageView();
        }
        if (e.PropertyName == nameof(PageRows) || e.PropertyName == nameof(PageColumns) ||
            e.PropertyName == nameof(CurrentPageIndex))
        {
            CurrentPageView?.Refresh();
        }
        base.OnPropertyChanged(e);
    }

    private void BuildCurrentPageView()
    {
        CurrentPageView = CollectionViewSource.GetDefaultView(Files);
        CurrentPageView.Filter = (obj) =>
        {
            if (obj is HamstertFileInfo fileInfo)
            {
                int ps = PageSize;
                int idx = Files.IndexOf(fileInfo);
                int startIndex = CurrentPageIndex * PageSize;
                return idx >= startIndex && idx < (startIndex + PageSize);
            }
            return false;
        };
    }

}