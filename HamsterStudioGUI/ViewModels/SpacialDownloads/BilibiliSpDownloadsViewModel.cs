using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Bilibili.Services;
using Microsoft.Extensions.Logging;
using System.Windows.Input;

namespace HamsterStudioGUI.ViewModels.SpacialDownloads;

public partial class BilibiliSpDownloadsViewModel : KnownViewModel
{
    [ObservableProperty]
    private string _bvid = string.Empty;

    [ObservableProperty]
    private long _cid = 0;

    public ICommand DownloadCommand { get; }

    private Lazy<BangumiDownloadService> BangumiDownloadService { get; }

    public BilibiliSpDownloadsViewModel(ILogger<BilibiliSpDownloadsViewModel> logger) : base(logger)
    {
        BangumiDownloadService = new Lazy<BangumiDownloadService>(() => App.ResloveService<BangumiDownloadService>());
        DownloadCommand = new AsyncRelayCommand(DownloadExecute);
    }

    public async Task DownloadExecute()
    {
        await BangumiDownloadService.Value.DownloadVideoByBvid(Bvid, -1, Cid);
    }

}
