using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.Gallery.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using System.Windows.Input;
using static System.Net.WebRequestMethods;

namespace HamsterStudio.Gallery.Models;

public partial class FileGroupViewModel : KnownViewModel
{
    public string GroupName => DisplayName;

    [ObservableProperty]
    private List<HamstertFileInfo> _files = [];

    [ObservableProperty]
    private int _currentPageIndex = 0;

    [ObservableProperty]
    private int _pageColumns = 0;

    [ObservableProperty]
    private int _pageRows = 0;

    public int PageSize => PageColumns * PageRows;

    [ObservableProperty]
    private ICollectionView _CurrentPageView;

    public ICommand ViewCommand { get; }
    public ICommand PrevPageCommand { get; }
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
        PrevPageCommand = new RelayCommand(() =>
        {
            if (CurrentPageIndex <= 0)
            {
                CurrentPageIndex = Files.Count / PageSize;
            }
            else
            {
                CurrentPageIndex--;
            }
            Logger.Shared.Trace($"Page of {DisplayName} navigated to {CurrentPageIndex}");
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

    private bool pauseOnPropertyChanged = false;
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Files))
        {
            pauseOnPropertyChanged = true;
            Logger.Shared.Debug($"Files changed of {GroupName}.");

            if (Files.Count <= 64)
            {
                PageColumns = (int)Math.Round(Math.Sqrt(Files.Count));
                var rows = Files.Count / PageColumns;
                if (Files.Count % PageColumns != 0)
                {
                    rows++;
                }
                PageRows = Math.Min(rows, PageColumns);
                PageColumns = Math.Max(rows, PageColumns);
            }
            else
            {
                PageColumns = 9;
                PageRows = 6;
            }
            BuildCurrentPageView();
            pauseOnPropertyChanged = false;
            OnPageRowsChanged(PageRows);
        }
        if (!pauseOnPropertyChanged)
        {
            if (e.PropertyName == nameof(PageRows) || e.PropertyName == nameof(PageColumns) ||
                e.PropertyName == nameof(CurrentPageIndex))
            {
                CurrentPageView?.Refresh();
            }
            base.OnPropertyChanged(e);
        }
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

    public void UpdateFiles(string[] files)
    {
        var fileInfos = files.Select(x =>
        {
            HamstertFileInfo info = null;
            var rmcmd = new RelayCommand(() =>
            {
                Files.Remove(info);
                CurrentPageView?.Refresh();
                ShellApi.SendToRecycleBin(Path.GetFullPath(x));
            });
            info = new HamstertFileInfo(x) { RemoveCommand = rmcmd };
            return info;
        });

        if (Files == null || Files.Count <= 0)
        {
            Files = fileInfos.ToList();
        }
        else
        {
            Files.AddRange(fileInfos);
            CurrentPageView?.Refresh();
            Logger.Shared.Trace($"Reload file group {GroupName}");
        }
    }

}