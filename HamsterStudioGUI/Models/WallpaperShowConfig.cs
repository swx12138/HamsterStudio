using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Constants;
using HamsterStudio.Toolkits;
using HamsterStudio.Toolkits.SysCall;
using System.Drawing;
using System.IO;
using System.Text.Json.Serialization;
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

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}

public partial class WallpaperShowConfig : ObservableObject
{
    private static IDesktopWallpaper DesktopWallpaper = (IDesktopWallpaper)new DesktopWallpaper();

    public List<DesktopWallpaperInfo> MonitorIds { get; private set; } = [];

    [ObservableProperty]
    private List<ImageModelDim> _AlternateWallpappers = [];

    public WallpaperShowConfig()
    {
        MonitorIds = [.. Enumerable.Range(0, (int)DesktopWallpaper.GetMonitorDevicePathCount()).
            Select(i => new DesktopWallpaperInfo(DesktopWallpaper,(uint)i))];
        LoadAlternateWallpappers();
    }

    private void LoadAlternateWallpappers()
    {
        //AlternateWallpappers = [.. Directory.EnumerateFiles(@"E:\HamsterStudioHome\miyoushe","*.*",new EnumerationOptions(){ RecurseSubdirectories = true })
        //    //.Concat(Directory.EnumerateFiles(@"E:\HamsterStudioHome\Bilibili\dynamics","*.*",new EnumerationOptions(){ RecurseSubdirectories = true }))
        //    .Where(ImageUtils.IsImageFile)
        //    .Select(x =>
        //    {
        //        var img = Image.FromFile(x);
        //        return new ImageModelDim()
        //        {
        //            Path = x,
        //            Width = img.Width,
        //            Height = img.Height
        //        };
        //    })
        //    .Where(x => (x.Width >= 3840 && x.Height >= 2160) || (x.Height >= 3840 && x.Width >= 2160))];
    }

    public ICommand AlternateWallpapperDropCommand => new RelayCommand<string[]>(data =>
    {
        //AlternateWallpappers.AddRange(data);
    });
}
