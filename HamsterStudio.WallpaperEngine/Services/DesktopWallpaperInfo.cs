using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.SysCall;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace HamsterStudio.WallpaperEngine.Services;

public partial class DesktopWallpaperInfo : ObservableObject
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
    private IImageModelDimFilter _Filter = new ImageModelDimFilter() { Is4kOnly = true, IsMarkedOnly = true };

    [ObservableProperty]
    private DateTime _lastUpdateTime = DateTime.Now;

    [ObservableProperty]
    private DateTime _nextUpdateTime = DateTime.MaxValue;

    [ObservableProperty]
    private DesktopWallpaperPosition _position = DesktopWallpaperPosition.Center;

    public ICommand DropCommand => new RelayCommand<string[]>(data =>
    {
        CurrentWallpaper = data[0];
    });
    public ICommand RequestNewWallpapperCommand { get; }

    public DesktopWallpaperInfo(IDesktopWallpaper desktopWallpaper, uint mIdx)
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
            if(ChangeWallpaperTimer?.IsEnabled ?? false)
            {
                ChangeWallpaperTimer.Stop();
                ChangeWallpaperTimer.Start();
            }
        });
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AutoChangeWallpaper))
        {
            if (ChangeWallpaperTimer == null)
            {
                ChangeWallpaperTimer = new();
                ChangeWallpaperTimer.Interval = TimeSpan.FromMinutes(5);
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
        base.OnPropertyChanged(e);
    }

}
