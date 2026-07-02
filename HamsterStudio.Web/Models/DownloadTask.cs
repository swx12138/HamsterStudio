using CommunityToolkit.Mvvm.ComponentModel;

namespace HamsterStudio.Web.Models;

/// <summary>
/// 单个下载任务（叶子节点），对应一个文件的下载
/// </summary>
public partial class DownloadTask : ObservableObject
{
    // —— 标识 ——
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..12];

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private Uri? _url;

    [ObservableProperty]
    private string _savePath = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(SpeedText))]
    [NotifyPropertyChangedFor(nameof(EtaText))]
    private DownloadTaskStatus _status = DownloadTaskStatus.Queued;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Progress))]
    [NotifyPropertyChangedFor(nameof(SizeText))]
    [NotifyPropertyChangedFor(nameof(EtaText))]
    private long _totalBytes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Progress))]
    [NotifyPropertyChangedFor(nameof(SizeText))]
    [NotifyPropertyChangedFor(nameof(EtaText))]
    private long _downloadedBytes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpeedText))]
    [NotifyPropertyChangedFor(nameof(EtaText))]
    private double _speedBytesPerSecond;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private string? _errorMessage;

    [ObservableProperty]
    private int _retryCount;

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    [ObservableProperty]
    private long? _fileSize;

    public const int MaxRetryCount = 2;

    // —— 计算属性 ——
    public double Progress => TotalBytes > 0 ? (double)DownloadedBytes / TotalBytes : 0;

    public string SpeedText
    {
        get
        {
            if (Status != DownloadTaskStatus.Downloading || SpeedBytesPerSecond <= 0) return "—";
            return Barefeet.FileSystem.FileSizeDescriptor.ToReadableFileSize((long)SpeedBytesPerSecond) + "/s";
        }
    }

    public string EtaText
    {
        get
        {
            if (Status != DownloadTaskStatus.Downloading || SpeedBytesPerSecond <= 0 || TotalBytes <= 0)
                return "—";
            var remaining = TotalBytes - DownloadedBytes;
            if (remaining <= 0) return "0s";
            var seconds = remaining / SpeedBytesPerSecond;
            if (seconds < 60) return $"{seconds:F0}s";
            if (seconds < 3600) return $"{seconds / 60:F0}m{seconds % 60:F0}s";
            return $"{seconds / 3600:F0}h{(seconds % 3600) / 60:F0}m";
        }
    }

    public string SizeText
    {
        get
        {
            if (TotalBytes <= 0) return "—";
            var downloaded = Barefeet.FileSystem.FileSizeDescriptor.ToReadableFileSize(DownloadedBytes);
            var total = Barefeet.FileSystem.FileSizeDescriptor.ToReadableFileSize(TotalBytes);
            return $"{downloaded} / {total}";
        }
    }

    public string StatusText => Status switch
    {
        DownloadTaskStatus.Queued => "⏳ 排队",
        DownloadTaskStatus.Downloading => "↓ 下载中",
        DownloadTaskStatus.Paused => "⏸ 暂停",
        DownloadTaskStatus.Completed => "✓ 完成",
        DownloadTaskStatus.Failed => $"✗ {ErrorMessage ?? "失败"}",
        DownloadTaskStatus.Cancelled => "✗ 已取消",
        _ => "—",
    };

}
