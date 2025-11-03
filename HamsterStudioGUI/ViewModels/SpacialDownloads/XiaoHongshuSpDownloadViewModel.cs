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

    private Lazy<NoteDownloadService> _Downloader = new(() => App.ResloveService<NoteDownloadService>());
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
        var resp = await _Downloader.Value.DownloadWithBaseTokens([ImageToken]);
        if (resp == null || resp.Data.StaticFiles.Length <= 0)
        {
            Status = "下载失败或无效的Token。";
        }
        else
        {
            Status = $"下载完成。";
        }
    }
}
