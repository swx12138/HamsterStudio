using HamsterStudio.Web.Models;

namespace HamsterStudio.Web.Services;

/// <summary>
/// 下载管理器接口，管理任务包的排队、执行、暂停、取消等生命周期
/// </summary>
public interface IDownloadManager
{
    /// <summary>提交一个任务包到下载队列，立即开始调度</summary>
    Task<DownloadPackage> EnqueueAsync(DownloadPackage package);

    /// <summary>快速下载单个文件（内部创建单 Task 的 Package）</summary>
    Task<DownloadPackage> EnqueueSingleAsync(Uri url, string savePath, string? label = null, string source = "直链", string? coverUrl = null);

    /// <summary>暂停任务包内所有活跃任务</summary>
    void PausePackage(string packageId);

    /// <summary>恢复任务包内所有暂停的任务</summary>
    void ResumePackage(string packageId);

    /// <summary>取消任务包内所有任务</summary>
    void CancelPackage(string packageId);

    /// <summary>重试任务包内所有失败的任务</summary>
    void RetryFailedTasks(string packageId);

    /// <summary>暂停单个任务</summary>
    void PauseTask(string packageId, string taskId);

    /// <summary>恢复单个任务</summary>
    void ResumeTask(string packageId, string taskId);

    /// <summary>取消单个任务</summary>
    void CancelTask(string packageId, string taskId);

    /// <summary>重试单个失败的任务</summary>
    void RetryTask(string packageId, string taskId);

    /// <summary>暂停所有进行中的任务包</summary>
    void PauseAll();

    /// <summary>恢复所有暂停的任务包</summary>
    void ResumeAll();

    /// <summary>清除所有已完成/已取消/已失败的任务包</summary>
    void ClearCompleted();

    /// <summary>所有任务包（只读，用于 UI 绑定）</summary>
    System.Collections.ObjectModel.ObservableCollection<DownloadPackage> Packages { get; }
}
