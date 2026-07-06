using HamsterStudio.Barefeet.Constants;
using HamsterStudio.Web.Models;
using HamsterStudio.Web.Strategies;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace HamsterStudio.Web.Services;

/// <summary>
/// 下载管理器实现，负责下载任务包的调度、并发控制和生命周期管理。
/// 包级并发：默认同时下载 2 个任务包。
/// </summary>
public class DownloadManager(CommonDownloader downloader, ILogger<DownloadManager> logger,
                       int maxConcurrentPackages = DownloadConstants.DefaultMaxConcurrentPackages) : IDownloadManager
{

    // 包级并发控制
    private readonly SemaphoreSlim _packageSemaphore = new SemaphoreSlim(maxConcurrentPackages);

    // 取消令牌：按 taskId 管理
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _taskCts = new();

    // 暂停信号：按 taskId 管理，暂停时任务被取消但标记可恢复
    private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _pauseSignals = new();

    public ObservableCollection<DownloadPackage> Packages { get; } = [];

    // ──────────── 入队 ────────────

    public async Task<DownloadPackage> EnqueueAsync(DownloadPackage package)
    {
        Packages.Insert(0, package); // 最新的在最上面
        logger.LogInformation($"任务包入队: {package.Name} ({package.TotalCount} 个文件)");
        _ = ProcessPackageAsync(package); // 后台执行
        return await Task.FromResult(package);
    }

    public async Task<DownloadPackage> EnqueueSingleAsync(Uri url, string savePath,
        string? label = null, string source = "直链", string? coverUrl = null)
    {
        var task = new DownloadTask
        {
            FileName = Path.GetFileName(savePath),
            Label = label ?? "",
            Url = url,
            SavePath = savePath,
        };

        var package = new DownloadPackage
        {
            Name = task.FileName,
            Source = source,
            CoverUrl = coverUrl,
        };
        package.Tasks.Add(task);

        return await EnqueueAsync(package);
    }

    // ──────────── 包级操作 ────────────

    public void PausePackage(string packageId)
    {
        var pkg = FindPackage(packageId);
        if (pkg == null) return;

        foreach (var task in pkg.Tasks)
        {
            if (task.Status is DownloadTaskStatus.Downloading or DownloadTaskStatus.Queued)
            {
                PauseTaskInternal(packageId, task);
            }
        }
        logger.LogInformation($"任务包暂停: {pkg.Name}");
    }

    public void ResumePackage(string packageId)
    {
        var pkg = FindPackage(packageId);
        if (pkg == null) return;

        foreach (var task in pkg.Tasks)
        {
            if (task.Status == DownloadTaskStatus.Paused)
            {
                ResumeTaskInternal(packageId, pkg, task);
            }
        }
        logger.LogInformation($"任务包恢复: {pkg.Name}");
    }

    public void CancelPackage(string packageId)
    {
        var pkg = FindPackage(packageId);
        if (pkg == null) return;

        foreach (var task in pkg.Tasks)
        {
            CancelTaskInternal(task);
        }
        logger.LogInformation($"任务包取消: {pkg.Name}");
    }

    public void RetryFailedTasks(string packageId)
    {
        var pkg = FindPackage(packageId);
        if (pkg == null) return;

        foreach (var task in pkg.Tasks)
        {
            if (task.Status == DownloadTaskStatus.Failed)
            {
                RetryTaskInternal(packageId, pkg, task);
            }
        }
    }

    // ──────────── 任务级操作 ────────────

    public void PauseTask(string packageId, string taskId)
    {
        var task = FindTask(packageId, taskId);
        if (task is { Status: DownloadTaskStatus.Downloading or DownloadTaskStatus.Queued })
            PauseTaskInternal(packageId, task);
    }

    public void ResumeTask(string packageId, string taskId)
    {
        var pkg = FindPackage(packageId);
        var task = FindTask(packageId, taskId);
        if (pkg != null && task is { Status: DownloadTaskStatus.Paused })
            ResumeTaskInternal(packageId, pkg, task);
    }

    public void CancelTask(string packageId, string taskId)
    {
        var task = FindTask(packageId, taskId);
        if (task != null) CancelTaskInternal(task);
    }

    public void RetryTask(string packageId, string taskId)
    {
        var pkg = FindPackage(packageId);
        var task = FindTask(packageId, taskId);
        if (pkg != null && task is { Status: DownloadTaskStatus.Failed })
            RetryTaskInternal(packageId, pkg, task);
    }

    // ──────────── 全局操作 ────────────

    public void PauseAll()
    {
        foreach (var pkg in Packages)
            PausePackage(pkg.Id);
    }

    public void ResumeAll()
    {
        foreach (var pkg in Packages)
            ResumePackage(pkg.Id);
    }

    public void ClearCompleted()
    {
        var toRemove = Packages.Where(p =>
            p.Status is DownloadPackageStatus.Completed
                or DownloadPackageStatus.Failed
                or DownloadPackageStatus.Cancelled).ToList();
        foreach (var pkg in toRemove)
            Packages.Remove(pkg);
    }

    // ──────────── 内部：包处理 ────────────

    private async Task ProcessPackageAsync(DownloadPackage package)
    {
        await _packageSemaphore.WaitAsync();
        try
        {
            // 并发下载包内所有任务
            var tasks = package.Tasks.Select(task => ProcessTaskAsync(package, task)).ToArray();
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"任务包处理异常: {package.Name}");
        }
        finally
        {
            _packageSemaphore.Release();
        }
    }

    // ──────────── 内部：单任务下载 ────────────

    private async Task ProcessTaskAsync(DownloadPackage package, DownloadTask task)
    {
        if (task.Status == DownloadTaskStatus.Completed ||
            task.Status == DownloadTaskStatus.Cancelled)
            return;

        using var cts = new CancellationTokenSource();
        _taskCts[task.Id] = cts;

        try
        {
            var token = cts.Token;

            // 检查是否应跳过
            if (File.Exists(task.SavePath))
            {
                task.Status = DownloadTaskStatus.Completed;
                task.DownloadedBytes = task.TotalBytes > 0 ? task.TotalBytes : new FileInfo(task.SavePath).Length;
                return;
            }

            task.Status = DownloadTaskStatus.Downloading;
            task.RetryCount = 0;

            while (task.RetryCount <= DownloadTask.MaxRetryCount)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    var progress = new Progress<DownloadProgress>(p =>
                    {
                        task.DownloadedBytes = p.BytesDownloaded;
                        task.TotalBytes = p.TotalBytes;
                        task.SpeedBytesPerSecond = p.SpeedBytesPerSecond;
                    });

                    if (task.Url == null)
                        throw new InvalidOperationException("Task URL is null");

                    var downloadStrategy = DownloadStrategyFactory.CreateStrategy(
                        chunkSize: 0,
                        maxConnections: 1 // 单任务不并发分块
                    );

                    var status = await downloader.DownloadFileAsync(
                        task.Url,
                        task.SavePath,
                        requestStrategy: null,
                        contentCopyStrategy: null,
                        downloadStrategy,
                        shape: null,
                        progress: progress,
                        cancellationToken: token
                    );

                    if (status == DownloadStatus.Success || status == DownloadStatus.Exists)
                    {
                        task.Status = DownloadTaskStatus.Completed;
                        task.DownloadedBytes = task.TotalBytes;
                        //_logger.LogInformation($"任务完成: {task.FileName}");
                        return;
                    }

                    task.Status = DownloadTaskStatus.Failed;
                    task.ErrorMessage = $"下载失败: {status}";
                }
                catch (OperationCanceledException)
                {
                    // 被暂停或取消——不重试
                    return;
                }
                catch (Exception ex)
                {
                    task.RetryCount++;
                    task.ErrorMessage = ex.Message;
                    logger.LogWarning(ex, $"下载失败 ({task.RetryCount}/{DownloadTask.MaxRetryCount}): {task.FileName}");

                    if (task.RetryCount > DownloadTask.MaxRetryCount)
                    {
                        task.Status = DownloadTaskStatus.Failed;
                        return;
                    }

                    // 重试前短暂等待
                    await Task.Delay(1000, CancellationToken.None);
                }
            }
        }
        finally
        {
            _taskCts.TryRemove(task.Id, out _);
        }
    }

    // ──────────── 内部：暂停/恢复/取消实现 ────────────

    private void PauseTaskInternal(string packageId, DownloadTask task)
    {
        // 取消当前下载
        if (_taskCts.TryRemove(task.Id, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
        task.Status = DownloadTaskStatus.Paused;
    }

    private void ResumeTaskInternal(string packageId, DownloadPackage package, DownloadTask task)
    {
        task.Status = DownloadTaskStatus.Queued;
        task.ErrorMessage = null;
        _ = ProcessTaskAsync(package, task); // 重新进入下载流程
    }

    private void CancelTaskInternal(DownloadTask task)
    {
        if (_taskCts.TryRemove(task.Id, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
        task.Status = DownloadTaskStatus.Cancelled;
    }

    private void RetryTaskInternal(string packageId, DownloadPackage package, DownloadTask task)
    {
        task.Status = DownloadTaskStatus.Queued;
        task.ErrorMessage = null;
        task.RetryCount = 0;
        task.DownloadedBytes = 0;
        _ = ProcessTaskAsync(package, task);
    }

    // ──────────── 查找辅助 ────────────

    private DownloadPackage? FindPackage(string packageId)
    {
        return Packages.FirstOrDefault(p => p.Id == packageId);
    }

    private DownloadTask? FindTask(string packageId, string taskId)
    {
        return FindPackage(packageId)?.Tasks.FirstOrDefault(t => t.Id == taskId);
    }
}
