using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HamsterStudio.Web.Models;

/// <summary>
/// 下载任务包，包含一个或多个 DownloadTask。
/// 单文件下载就是一个只有 1 个 Task 的 Package。
/// </summary>
public class DownloadPackage : INotifyPropertyChanged
{
    // —— 标识 ——
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..12];
    public string Name { get; set; } = string.Empty;
    public string Source { get; init; } = "直链";

    // —— 子任务 ——
    public ObservableCollection<DownloadTask> Tasks { get; }

    // —— 元数据 ——
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public string? CoverUrl { get; set; }

    public DownloadPackage()
    {
        Tasks = [];
        Tasks.CollectionChanged += OnTasksCollectionChanged;
    }

    private void OnTasksCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (DownloadTask task in e.NewItems)
                task.PropertyChanged += OnTaskPropertyChanged;
        }
        if (e.OldItems != null)
        {
            foreach (DownloadTask task in e.OldItems)
                task.PropertyChanged -= OnTaskPropertyChanged;
        }
        NotifyAggregatedProperties();
    }

    private void OnTaskPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        NotifyAggregatedProperties();
    }

    private void NotifyAggregatedProperties()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(TotalBytes));
        OnPropertyChanged(nameof(DownloadedBytes));
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(SpeedText));
        OnPropertyChanged(nameof(EtaText));
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(ErrorSummary));
        OnPropertyChanged(nameof(IsSingleTask));
    }

    // —— 聚合状态（由子任务计算）——
    public DownloadPackageStatus Status
    {
        get
        {
            if (Tasks.Count == 0) return DownloadPackageStatus.Queued;

            bool hasQueued = false, hasDownloading = false, hasPaused = false;
            bool hasCompleted = false, hasFailed = false, hasCancelled = false;

            foreach (var t in Tasks)
            {
                switch (t.Status)
                {
                    case DownloadTaskStatus.Queued: hasQueued = true; break;
                    case DownloadTaskStatus.Downloading: hasDownloading = true; break;
                    case DownloadTaskStatus.Paused: hasPaused = true; break;
                    case DownloadTaskStatus.Completed: hasCompleted = true; break;
                    case DownloadTaskStatus.Failed: hasFailed = true; break;
                    case DownloadTaskStatus.Cancelled: hasCancelled = true; break;
                }
            }

            // 优先级：Downloading > Paused > PartialFailed > Failed/Cancelled > Completed > Queued
            if (hasDownloading) return DownloadPackageStatus.Downloading;
            if (hasPaused && !hasDownloading)
            {
                // 如果只有 paused + completed/failed → PartialFailed；否则 Paused
                return (hasCompleted || hasFailed) ? DownloadPackageStatus.PartialFailed : DownloadPackageStatus.Paused;
            }
            if (hasCompleted && (hasFailed || hasCancelled)) return DownloadPackageStatus.PartialFailed;
            if (hasFailed && !hasCompleted) return DownloadPackageStatus.Failed;
            if (hasCancelled && !hasCompleted && !hasFailed) return DownloadPackageStatus.Cancelled;
            if (hasCompleted && !hasFailed && !hasCancelled && !hasQueued && !hasDownloading && !hasPaused)
                return DownloadPackageStatus.Completed;
            return DownloadPackageStatus.Queued;
        }
    }

    public long TotalBytes
    {
        get
        {
            long sum = 0;
            foreach (var t in Tasks) sum += t.TotalBytes;
            return sum;
        }
    }

    public long DownloadedBytes
    {
        get
        {
            long sum = 0;
            foreach (var t in Tasks) sum += t.DownloadedBytes;
            return sum;
        }
    }

    public double Progress => TotalBytes > 0 ? (double)DownloadedBytes / TotalBytes : 0;

    public string SpeedText
    {
        get
        {
            double speed = 0;
            bool anyDownloading = false;
            foreach (var t in Tasks)
            {
                if (t.Status == DownloadTaskStatus.Downloading)
                {
                    speed += t.SpeedBytesPerSecond;
                    anyDownloading = true;
                }
            }
            if (!anyDownloading || speed <= 0) return "—";
            return Barefeet.FileSystem.FileSizeDescriptor.ToReadableFileSize((long)speed) + "/s";
        }
    }

    public string EtaText
    {
        get
        {
            double speed = 0;
            long remaining = 0;
            foreach (var t in Tasks)
            {
                if (t.Status == DownloadTaskStatus.Downloading)
                {
                    speed += t.SpeedBytesPerSecond;
                    remaining += t.TotalBytes - t.DownloadedBytes;
                }
                else if (t.Status == DownloadTaskStatus.Queued)
                {
                    remaining += t.TotalBytes;
                }
            }
            if (speed <= 0 || remaining <= 0) return "—";
            var seconds = remaining / speed;
            if (seconds < 60) return $"{seconds:F0}s";
            if (seconds < 3600) return $"{seconds / 60:F0}m{seconds % 60:F0}s";
            return $"{seconds / 3600:F0}h{(seconds % 3600) / 60:F0}m";
        }
    }

    public int CompletedCount
    {
        get
        {
            int count = 0;
            foreach (var t in Tasks)
                if (t.Status == DownloadTaskStatus.Completed) count++;
            return count;
        }
    }

    public int TotalCount => Tasks.Count;

    public string? ErrorSummary
    {
        get
        {
            var errors = Tasks.Where(t => t.Status == DownloadTaskStatus.Failed && t.ErrorMessage != null)
                              .Select(t => t.ErrorMessage)
                              .Distinct()
                              .ToList();
            return errors.Count > 0 ? string.Join("; ", errors) : null;
        }
    }

    public string StatusText => Status switch
    {
        DownloadPackageStatus.Queued => "⏳ 排队",
        DownloadPackageStatus.Downloading => $"↓ 下载中 {CompletedCount}/{TotalCount}",
        DownloadPackageStatus.Paused => "⏸ 暂停",
        DownloadPackageStatus.Completed => "✓ 完成",
        DownloadPackageStatus.PartialFailed => $"⚠ 部分失败 {CompletedCount}/{TotalCount}",
        DownloadPackageStatus.Failed => "✗ 失败",
        DownloadPackageStatus.Cancelled => "✗ 已取消",
        _ => "—",
    };

    public bool IsSingleTask => Tasks.Count <= 1;

    // ── INotifyPropertyChanged ──
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
