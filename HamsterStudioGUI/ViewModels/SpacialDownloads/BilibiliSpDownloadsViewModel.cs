using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Bilibili.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public BilibiliSpDownloadsViewModel()
    {
        BangumiDownloadService = new Lazy<BangumiDownloadService>(() => App.ResloveService<BangumiDownloadService>());
        DownloadCommand = new AsyncRelayCommand(DownloadExecute);
    }

    public async Task DownloadExecute()
    {
        await BangumiDownloadService.Value.DownloadVideoByBvid(Bvid, -1, Cid);
    }

}
