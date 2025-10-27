using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.MVVM;
using System.Windows.Input;

namespace HamsterStudioGUI.ViewModels.SpacialDownloads;

public abstract partial class SpDownloadsViewModel : ViewModel
{
    [ObservableProperty]
    private bool _isDownloading = false;

    [ObservableProperty]
    private string _status = string.Empty;

    public ICommand DownloadCommand { get; }

    public SpDownloadsViewModel()
    {
        DownloadCommand = new AsyncRelayCommand(DownloadExecute);
        Status = "初始化完成。";
    }

    public abstract Task DownloadExecute();
}
