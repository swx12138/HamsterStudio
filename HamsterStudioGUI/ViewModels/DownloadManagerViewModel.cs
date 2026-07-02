using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Web.Models;
using HamsterStudio.Web.Services;
using System.Windows.Data;

namespace HamsterStudioGUI.ViewModels;

/// <summary>
/// 下载管理面板的主 ViewModel
/// </summary>
public partial class DownloadManagerViewModel : ObservableObject
{
    private readonly IDownloadManager _downloadManager;

    public System.Collections.ObjectModel.ObservableCollection<DownloadPackage> Packages =>
        _downloadManager.Packages;

    [ObservableProperty]
    private DownloadPackageStatus? _filterStatus = null;

    public IEnumerable<DownloadPackage> FilteredPackages =>
        FilterStatus == null
            ? Packages
            : Packages.Where(p => p.Status == FilterStatus);

    public DownloadManagerViewModel(IDownloadManager downloadManager)
    {
        _downloadManager = downloadManager;
        // 允许后台线程（Web API）安全修改绑定到 UI 的集合
        BindingOperations.EnableCollectionSynchronization(
            _downloadManager.Packages, new object());
    }

    partial void OnFilterStatusChanged(DownloadPackageStatus? value)
    {
        OnPropertyChanged(nameof(FilteredPackages));
    }

    [RelayCommand]
    private void PauseAll() => _downloadManager.PauseAll();

    [RelayCommand]
    private void ResumeAll() => _downloadManager.ResumeAll();

    [RelayCommand]
    private void ClearCompleted()
    {
        _downloadManager.ClearCompleted();
        OnPropertyChanged(nameof(FilteredPackages));
    }

    public void RefreshFilter()
    {
        OnPropertyChanged(nameof(FilteredPackages));
    }

    // ── 包级操作（通过 CommandParameter 传递 packageId）──

    [RelayCommand]
    private void PausePackage(string packageId) => _downloadManager.PausePackage(packageId);

    [RelayCommand]
    private void ResumePackage(string packageId) => _downloadManager.ResumePackage(packageId);

    [RelayCommand]
    private void CancelPackage(string packageId) => _downloadManager.CancelPackage(packageId);

    [RelayCommand]
    private void RetryFailedTasks(string packageId) => _downloadManager.RetryFailedTasks(packageId);

    // ── 任务级操作 ──

    [RelayCommand]
    private void PauseTask(string param)
    {
        var (pkgId, taskId) = ParseCompositeParam(param);
        _downloadManager.PauseTask(pkgId, taskId);
    }

    [RelayCommand]
    private void ResumeTask(string param)
    {
        var (pkgId, taskId) = ParseCompositeParam(param);
        _downloadManager.ResumeTask(pkgId, taskId);
    }

    [RelayCommand]
    private void CancelTask(string param)
    {
        var (pkgId, taskId) = ParseCompositeParam(param);
        _downloadManager.CancelTask(pkgId, taskId);
    }

    [RelayCommand]
    private void RetryTask(string param)
    {
        var (pkgId, taskId) = ParseCompositeParam(param);
        _downloadManager.RetryTask(pkgId, taskId);
    }

    /// <summary>
    /// 解析复合参数 "packageId|taskId"
    /// </summary>
    private static (string packageId, string taskId) ParseCompositeParam(string param)
    {
        var parts = param.Split('|');
        return parts.Length == 2 ? (parts[0], parts[1]) : (param, param);
    }
}
