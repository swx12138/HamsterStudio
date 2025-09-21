using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Algorithm.Random;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.Constants;
using HamsterStudio.Toolkits;
using HamsterStudio.Toolkits.Services;
using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Data;
using System.Windows.Input;

namespace HamsterStudio.WallpaperEngine.Services;

public partial class WallpaperShowConfig : ObservableObject, IDisposable
{
    const string BaseWallpapperDir = @"E:\Pictures\bizhi";

    private static string TempDdir = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        SystemConsts.ApplicationName);

    private static IDesktopWallpaper DesktopWallpaper = (IDesktopWallpaper)new DesktopWallpaper();
    private readonly ImageMetaInfoReadService _imageMetaInfoReadService;
    private List<ImageModelDim> _AlternateWallpappers = [];

    public List<DesktopWallpaperInfo> MonitorIds { get; private set; } = [];

    [ObservableProperty]
    private ICollectionView _filteredAlternateWallpappersView;

    [ObservableProperty]
    private IImageModelDimFilter _alternateWallpappersFilter;

    [ObservableProperty]
    private bool _isMarkedFirst = false;

    public ICommand LoadMoreImagesCommand { get; }
    public ICommand AlternateWallpapperDropCommand => new RelayCommand<string[]>(data =>
    {
        //AlternateWallpappers.AddRange(data);
    });

    public WallpaperShowConfig(ImageMetaInfoReadService imageMetaInfoReadService)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        MonitorIds = [.. Enumerable.Range(0, (int)DesktopWallpaper.GetMonitorDevicePathCount()).
            Select(i => new DesktopWallpaperInfo(DesktopWallpaper,(uint)i))];
        foreach (var monitor in MonitorIds)
        {
            monitor.RequestNewWallpapper += Monitor_RequestNewWallpapper;
        }

        AlternateWallpappersFilter = new ImageModelDimFilter();
        (AlternateWallpappersFilter as ImageModelDimFilter).PropertyChanged += (s, e) =>
        {
            FilteredAlternateWallpappersView?.Refresh();
        };

        LoadMoreImagesCommand = new RelayCommand(() =>
        {
            OpenFolderDialog dialog = new();
            dialog.Title = "选择文件夹";
            dialog.Multiselect = false;
            if (!(dialog.ShowDialog() ?? false))
            {
                return;
            }

            LoadAlternateWallpappers(dialog.FolderName);
        });

        try
        {
            var dat = File.ReadAllBytes(Path.Combine(TempDdir, "wallpapers.dat"));
            var arr = BinaryDataSerializer.Deserialize<List<ImageModelDim>>(dat);
            if (arr != null)
            {
                _AlternateWallpappers = arr;
            }
        }
        catch
        {
        }

        _imageMetaInfoReadService = imageMetaInfoReadService;
        LoadAlternateWallpappers(BaseWallpapperDir);

        FilteredAlternateWallpappersView = CollectionViewSource.GetDefaultView(_AlternateWallpappers);
        FilteredAlternateWallpappersView.Filter = o =>
        {
            if (o is ImageModelDim item)
            {
                return AlternateWallpappersFilter.Filter(item);
            }
            return false;
        };

        Logger.Shared.Trace($"WallpaperShowConfig initialized in {stopwatch.Elapsed}");
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(IsMarkedFirst))
        {
            if (IsMarkedFirst)
            {
                FilteredAlternateWallpappersView?.SortDescriptions.Clear();
                FilteredAlternateWallpappersView?.SortDescriptions.Add(new SortDescription(nameof(ImageModelDim.Mark), ListSortDirection.Descending));
                //FilteredAlternateWallpappersView?.SortDescriptions.Add(new SortDescription(nameof(ImageModelDim.FileName), ListSortDirection.Ascending));
            }
            else
            {
                FilteredAlternateWallpappersView?.SortDescriptions.Clear();
                //FilteredAlternateWallpappersView?.SortDescriptions.Add(new SortDescription(nameof(ImageModelDim.FileName), ListSortDirection.Ascending));
            }
        }
        base.OnPropertyChanged(e);
    }

    private void Monitor_RequestNewWallpapper(object? sender, EventArgs e)
    {
        if (sender is DesktopWallpaperInfo info)
        {
            var wpp = _AlternateWallpappers.Where(x => info.Filter.Filter(x)).Choice();
            info.CurrentWallpaper = wpp.Path;
        }
    }

    private void LoadAlternateWallpappers(string dir)
    {
        Logger.Shared.Information($"Loading {dir}");
        var newfiles = Directory.EnumerateFiles(dir, "*.*", new EnumerationOptions() { RecurseSubdirectories = true })
            .Where(ImageUtils.IsImageFile)
            .Select(x =>
            {
                try
                {
                    return new ImageModelDim()
                    {
                        Path = x,
                        MetaInfo = _imageMetaInfoReadService.Read(x),
                        RemoveImageCommand = new RelayCommand<ImageModelDim>(img =>
                        {
                            if (img == null) return;
                            _AlternateWallpappers.Remove(img);
                            FilteredAlternateWallpappersView?.Refresh();
                        })
                    };
                }
                catch
                {
                    Debug.WriteLine($"Read file {x} failed.");
                    return new ImageModelDim()
                    {
                        Path = x,
                        MetaInfo = new ImageMetaInfo()
                        {
                            Width = 0,
                            Height = 0,
                            Type = "unknown"
                        }
                    };
                }
            })
            //.Where(x => (x.MetaInfo.Width >= 3840 && x.MetaInfo.Height >= 2160) || (x.MetaInfo.Height >= 3840 && x.MetaInfo.Width >= 2160))
            .Where(x => AlternateWallpappersFilter.Filter(x)) // 会导致部分图片一开始就不加载，that's I want.
            .Where(x => !_AlternateWallpappers.Any(y => string.Equals(y.Path, x.Path, StringComparison.OrdinalIgnoreCase))) // 去重
            .ToArray();
        Logger.Shared.Trace("Updating alternate wallpappers");

        using (FilteredAlternateWallpappersView?.DeferRefresh())
            _AlternateWallpappers.AddRange(newfiles);

        Logger.Shared.Trace("Update done.");
    }

    public void SaveConfig()
    {
        try
        {
            var dat = BinaryDataSerializer.Serialize(_AlternateWallpappers);
            File.WriteAllBytes(Path.Combine(TempDdir, "wallpapers.dat"), dat);
            Logger.Shared.Debug($"Saved wallpapper dat.");
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        SaveConfig();
        foreach (var monitor in MonitorIds)
        {
            monitor.RequestNewWallpapper -= Monitor_RequestNewWallpapper;
        }
    }

}
