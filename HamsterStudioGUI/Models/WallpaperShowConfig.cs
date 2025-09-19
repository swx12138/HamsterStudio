using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.Constants;
using HamsterStudio.Toolkits;
using HamsterStudio.Toolkits.Services;
using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Data;
using System.Windows.Input;

namespace HamsterStudioGUI.Models;

public class DesktopWallpaperInfo : ObservableObject
{
    public string MonitorId { get; set; }

    private readonly IDesktopWallpaper _desktopWallpaper;
    private string _CurrentWallpaper;

    public DesktopWallpaperInfo(IDesktopWallpaper desktopWallpaper, uint mIdx)
    {
        _desktopWallpaper = desktopWallpaper;
        MonitorId = desktopWallpaper.GetMonitorDevicePathAt(mIdx);
        _CurrentWallpaper = desktopWallpaper.GetWallpaper(MonitorId);

    }

    public string CurrentWallpaper
    {
        get => _CurrentWallpaper;
        set
        {
            if (value == null) return;
            SetProperty(ref _CurrentWallpaper, value);
            _desktopWallpaper.SetWallpaper(MonitorId, value);
        }
    }

    private static string TempDdir = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        SystemConsts.ApplicationName);


    public ICommand DropCommand => new RelayCommand<string[]>(data =>
    {
        //var img = Image.FromFile(data[0]);
        //if ((img.Width > img.Height && img.Width > 6000) || (img.Width < img.Height && img.Height > 6000))
        //{
        //    string temp = Path.Combine(TempDdir, Path.GetFileName(data[0]));
        //    if (img.Width > img.Height) // 横图
        //    {
        //        ImageUtils.ScaleImage(data[0], 3840.0 / img.Width, temp);
        //    }
        //    else   // 竖图
        //    {
        //        ImageUtils.ScaleImage(data[0], 2160.0 / img.Height, temp);
        //    }
        //    CurrentWallpaper = temp;
        //}
        //else
        {
            CurrentWallpaper = data[0];
        }
    });
}

public class ImageModelDim
{
    [JsonIgnore]
    public string FileName => System.IO.Path.GetFileName(Path);

    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("meta")]
    public ImageMetaInfo MetaInfo { get; init; }

    [JsonIgnore]
    public ICommand RevalInExplorerCommand { get; }

    public ImageModelDim()
    {
        RevalInExplorerCommand = new RelayCommand(() => ShellApi.SelectFile(Path));
    }

}

public interface IImageModelDimFilter
{
    bool Filter(ImageModelDim item);
}

public partial class ImageModelDimFilter : ObservableObject, IImageModelDimFilter
{
    [ObservableProperty]
    private bool _Is4kOnly = true;

    [ObservableProperty]
    private bool _IsVerticalOnly = false;

    [ObservableProperty]
    private bool _IsHorizontalOnly = false;

    public event EventHandler<PropertyChangedEventArgs> PropertyChanged;

    public bool Filter(ImageModelDim item)
    {
        if (item == null) return false;
        if (Is4kOnly && !((item.MetaInfo.Width >= 3840 && item.MetaInfo.Height >= 2160) || (item.MetaInfo.Height >= 3840 && item.MetaInfo.Width >= 2160)))
        {
            // ShellApi.SendToRecycleBin(item.Path);
            return false;
        }
        if (IsVerticalOnly && item.MetaInfo.Width > item.MetaInfo.Height)
        {
            return false;
        }
        if (IsHorizontalOnly && item.MetaInfo.Width < item.MetaInfo.Height)
        {
            return false;
        }
        return true;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        Logger.Shared.Debug($"Property {e.PropertyName} has Changed.");
        PropertyChanged?.Invoke(this, e);
        base.OnPropertyChanged(e);
    }
}

public partial class WallpaperShowConfig : ObservableObject
{
    private static IDesktopWallpaper DesktopWallpaper = (IDesktopWallpaper)new DesktopWallpaper();
    private readonly ImageMetaInfoReadService _imageMetaInfoReadService;

    public List<DesktopWallpaperInfo> MonitorIds { get; private set; } = [];

    private List<ImageModelDim> _AlternateWallpappers = [];

    [ObservableProperty]
    private ICollectionView _filteredAlternateWallpappersView;

    [ObservableProperty]
    private IImageModelDimFilter _alternateWallpappersFilter;

    public ICommand LoadMoreImagesCommand { get; }

    public WallpaperShowConfig(ImageMetaInfoReadService imageMetaInfoReadService)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        MonitorIds = [.. Enumerable.Range(0, (int)DesktopWallpaper.GetMonitorDevicePathCount()).
            Select(i => new DesktopWallpaperInfo(DesktopWallpaper,(uint)i))];

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

        _imageMetaInfoReadService = imageMetaInfoReadService;
        //LoadAlternateWallpappers(@"E:\Pictures\DCIMm");
        //LoadAlternateWallpappers(@"E:\Pictures\bili");
        LoadAlternateWallpappers(@"E:\Pictures\bizhi");

        FilteredAlternateWallpappersView = CollectionViewSource.GetDefaultView(_AlternateWallpappers);
        FilteredAlternateWallpappersView.Filter = o =>
        {
            if (o is ImageModelDim item)
            {
                return AlternateWallpappersFilter.Filter(item);
            }
            return false;
        };


        Logger.Shared.Information($"WallpaperShowConfig initialized in {stopwatch.Elapsed}");
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
                        MetaInfo = _imageMetaInfoReadService.Read(x)
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
            .ToArray();
        Logger.Shared.Information("Updating alternate wallpappers");

        _AlternateWallpappers.AddRange(newfiles);
        Logger.Shared.Information("Update done.");
    }

    public ICommand AlternateWallpapperDropCommand => new RelayCommand<string[]>(data =>
    {
        //AlternateWallpappers.AddRange(data);
    });

}
