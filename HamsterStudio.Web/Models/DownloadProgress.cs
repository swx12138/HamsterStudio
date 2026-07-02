namespace HamsterStudio.Web.Models;

/// <summary>
/// 下载进度报告，通过 IProgress&lt;DownloadProgress&gt; 回调传递
/// </summary>
public readonly record struct DownloadProgress(
    long BytesDownloaded,
    long TotalBytes,
    double SpeedBytesPerSecond
)
{
    public double Progress => TotalBytes > 0 ? (double)BytesDownloaded / TotalBytes : 0;

    public string SpeedText
    {
        get
        {
            if (SpeedBytesPerSecond <= 0) return "—";
            return Barefeet.FileSystem.FileSizeDescriptor.ToReadableFileSize((long)SpeedBytesPerSecond) + "/s";
        }
    }

    public string EtaText
    {
        get
        {
            if (SpeedBytesPerSecond <= 0 || TotalBytes <= 0) return "—";
            var remaining = TotalBytes - BytesDownloaded;
            if (remaining <= 0) return "0s";
            var seconds = remaining / SpeedBytesPerSecond;
            if (seconds < 60) return $"{seconds:F0}s";
            if (seconds < 3600) return $"{seconds / 60:F0}m{seconds % 60:F0}s";
            return $"{seconds / 3600:F0}h{(seconds % 3600) / 60:F0}m";
        }
    }
}
