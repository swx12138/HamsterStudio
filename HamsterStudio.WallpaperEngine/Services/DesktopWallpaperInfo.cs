using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.Toolkits.DragDrop;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace HamsterStudio.WallpaperEngine.Services;

public partial class DesktopWallpaperInfo : ObservableObject, IDroppable<ImageModelDim>
{
    private readonly IDesktopWallpaper _desktopWallpaper;
    private string _CurrentWallpaper;

    public string MonitorId { get; set; }
    public string CurrentWallpaper
    {
        get => _CurrentWallpaper;
        set
        {
            if (value == null) return;
            SetProperty(ref _CurrentWallpaper, value);
            _desktopWallpaper.SetWallpaper(MonitorId, value);
            LastUpdateTime = DateTime.Now;
            NextUpdateTime = AutoChangeWallpaper ?
                (DateTime.Now + (ChangeWallpaperTimer?.Interval ?? throw new Exception("Not possible!"))) :
                DateTime.MaxValue;
        }
    }

    public event EventHandler RequestNewWallpapper;

    [ObservableProperty]
    private bool _AutoChangeWallpaper = false;

    [ObservableProperty]
    private DispatcherTimer _ChangeWallpaperTimer = null;

    [ObservableProperty]
    private uint _DispatcherTimerInterval = 15;

    [ObservableProperty]
    private IImageModelDimFilter _Filter = new ImageModelDimFilter() { Is4kOnly = true, IsMarkedOnly = true }; // TBD:看看Filter更新时时间消耗在哪里

    [ObservableProperty]
    private DateTime _lastUpdateTime = DateTime.Now;

    [ObservableProperty]
    private DateTime _nextUpdateTime = DateTime.MaxValue;

    public ICommand DropCommand => new RelayCommand<string[]>(data =>
    {
        CurrentWallpaper = data[0];
    });
    public ICommand RequestNewWallpapperCommand { get; }

    public string[] AcceptDataFormat { get; } = [nameof(ImageModelDim), DataFormats.FileDrop];

    public bool InitSucceeds { get; } = true;

    public DesktopWallpaperInfo(IDesktopWallpaper desktopWallpaper, uint mIdx)
    {
        try
        {
            _desktopWallpaper = desktopWallpaper;
            MonitorId = desktopWallpaper.GetMonitorDevicePathAt(mIdx);
            var rect = desktopWallpaper.GetMonitorRECT(MonitorId);
            if (rect.Right - rect.Left > rect.Bottom - rect.Top) // 宽大于高
            {
                Filter.IsHorizontalOnly = true;
                Filter.IsVerticalOnly = false;
            }
            else
            {
                Filter.IsVerticalOnly = true;
                Filter.IsHorizontalOnly = false;
            }

            _CurrentWallpaper = desktopWallpaper.GetWallpaper(MonitorId);
            RequestNewWallpapperCommand = new RelayCommand(() =>
            {
                RequestNewWallpapper?.Invoke(this, null);
                if (ChangeWallpaperTimer?.IsEnabled ?? false)
                {
                    ChangeWallpaperTimer.Stop();
                    ChangeWallpaperTimer.Start();
                }
            });
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(AutoChangeWallpaper)));
        }
        catch (Exception ex)
        {
            Logger.Shared.Trace(ex.Message + "\n" + ex.StackTrace);
            InitSucceeds = false;
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AutoChangeWallpaper))
        {
            if (ChangeWallpaperTimer == null)
            {
                ChangeWallpaperTimer = new();
                ChangeWallpaperTimer.Interval = TimeSpan.FromMinutes(15);
                ChangeWallpaperTimer.Tick += (sdrr, ee) =>
                {
                    RequestNewWallpapper?.Invoke(this, e);
                };
            }
            if (AutoChangeWallpaper)
            {
                ChangeWallpaperTimer.Start();
            }
            else
            {
                ChangeWallpaperTimer.Stop();
            }

            NextUpdateTime = AutoChangeWallpaper ?
                (DateTime.Now + (ChangeWallpaperTimer?.Interval ?? throw new Exception("Not possible!"))) :
                DateTime.MaxValue;
        }
        else if (e.PropertyName == nameof(DispatcherTimerInterval))
        {
            if (ChangeWallpaperTimer != null)
            {
                ChangeWallpaperTimer.Interval = TimeSpan.FromMinutes(DispatcherTimerInterval);
                NextUpdateTime = AutoChangeWallpaper ?
                    (DateTime.Now + (ChangeWallpaperTimer?.Interval ?? throw new Exception("Not possible!"))) :
                    DateTime.MaxValue;
            }
        }

        base.OnPropertyChanged(e);
    }

    public void Drop(ImageModelDim data)
    {
        CurrentWallpaper = data.Path;
    }

    public void Drop(object? data)
    {
        if (AcceptDataFormat.Contains(DataFormats.FileDrop) && data is string[] paths)
        {
            CurrentWallpaper = Path.GetFullPath(paths[0]);
        }
    }
}
