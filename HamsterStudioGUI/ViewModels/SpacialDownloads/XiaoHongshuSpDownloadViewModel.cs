using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.RedBook.Services;
using HamsterStudio.Web.Services;
using System.IO;
using System.Windows.Input;

namespace HamsterStudioGUI.ViewModels.SpacialDownloads;

public partial class XiaoHongshuSpDownloadViewModel : SpDownloadsViewModel
{
    [ObservableProperty]
    private string _imageToken = string.Empty;

    private Lazy<CommonDownloader> _CommonDownloader = new(() => App.ResloveService<CommonDownloader>());
    private Lazy<FileMgmt> _FileMgmt = new(() => App.ResloveService<FileMgmt>());
    private PreTokenCollector _PreTokenCollector = App.ResloveService<PreTokenCollector>();

    public IReadOnlyCollection<string> KnownTokenList { get; private set; }

    public ICommand TestCommand { get; }

    public XiaoHongshuSpDownloadViewModel()
    {
        _PreTokenCollector.TokenListChanged += _PreTokenCollector_TokenListChanged;
        KnownTokenList = _PreTokenCollector.GetTokens();
        TestCommand = new RelayCommand(() => _PreTokenCollector_TokenListChanged(null, null));
    }

    private void _PreTokenCollector_TokenListChanged(object? sender, EventArgs e)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            OnPropertyChanged(nameof(KnownTokenList));
        });
    }

    public override async Task DownloadExecute()
    {
        var processor = new NoteDetailProcessor(null, _FileMgmt.Value, _CommonDownloader.Value, Logger.Shared, true);
        var fileInfo = _FileMgmt.Value.GenerateImageFilenameLow("banned", 998, "XiaoHongshu_SpDownload", ImageToken, true);
        if (!Directory.Exists(fileInfo.Directory))
        {
            Directory.CreateDirectory(fileInfo.Directory);
        }

        var status = await processor.DownloadImageByToken(ImageToken, fileInfo, true);
        if (status != DownloadStatus.Failed)
        {
            Status = "下载完成";
            return;
        }
        foreach(var preToken in KnownTokenList )
        {
            status = await processor.DownloadImageByToken(preToken + ImageToken, fileInfo, true);
            if (status != DownloadStatus.Failed)
            {
                Status = "下载完成";
                return;
            }
        }
        Status = "下载失败";
    }
}
