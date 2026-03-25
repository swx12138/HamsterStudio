using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Gallery.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace HamsterStudio.Gallery.ViewModels;

public partial class ThumbnailModeViewModel : ViewModel
{
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
    private SearchableGalleryFolderModel _currentFolder = new(new DirectoryInfo(Environment.CurrentDirectory));

    [ObservableProperty]
    private bool _AutoSwitchPage = false;

    [ObservableProperty]
    private int _AutoSwitchPageMinDelay = 3999;

    private BackgroundWorker _autoSwitchPageWorker;
    private CancellationTokenSource _autoSwitchPageCancellationTokenSource;

    public ThumbnailModeViewModel(ILogger<ThumbnailModeViewModel> logger) : base(logger)
    {
        CurrentFolder.Logger = logger;
        _autoSwitchPageCancellationTokenSource = new CancellationTokenSource();
        _autoSwitchPageWorker = new BackgroundWorker();
        _autoSwitchPageWorker.DoWork += (s, e) =>
        {
            while (!_autoSwitchPageCancellationTokenSource.Token.IsCancellationRequested)
            {
                if (AutoSwitchPage)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PageIndex++;
                        if (PageIndex > MaxPageCount)
                        {
                            PageIndex = 1;
                        }
                    });
                }
                Thread.Sleep(AutoSwitchPageMinDelay);
            }
        };
    }

    ~ThumbnailModeViewModel()
    {
        _autoSwitchPageCancellationTokenSource.Cancel();
        //_autoSwitchPageWorker.CancelAsync();
        _autoSwitchPageWorker.Dispose();
        _autoSwitchPageCancellationTokenSource.Dispose();
    }

    public void StartAutoSwitchPage()
    {
        if (!AutoSwitchPage)
        {
            if (!_autoSwitchPageWorker.IsBusy)
            {
                _autoSwitchPageWorker.RunWorkerAsync();
            }
        }
    }

    public void ClearCache()
    {
        CurrentFolder.Files.Clear();
        CurrentFolder.DirInfo = new DirectoryInfo("Root");
        PageIndex = 0;
        MaxPageCount = 0;
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
    }

    public void UpdateMaxPageCount()
    {
        logger?.LogDebug(nameof(UpdateMaxPageCount));
        MaxPageCount = (CurrentFolder.Files.Count / DataCountPerPage) + (CurrentFolder.Files.Count % DataCountPerPage == 0 ? 0 : 1);
    }

    public void UpdateDataCountPerPage()
    {
        logger?.LogDebug(nameof(UpdateDataCountPerPage));
        DataCountPerPage = Rows * Columns;
    }

    public void OnPageIndexChanged()
    {
        CurrentFolder.UpdateViewRange(Columns, Rows, PageIndex);
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
}
