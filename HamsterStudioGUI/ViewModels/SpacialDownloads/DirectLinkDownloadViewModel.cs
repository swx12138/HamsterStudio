using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Web.FileSystem;
using HamsterStudio.Web.Services;
using HamsterStudio.Web.Utilities;
using System.ComponentModel;
using System.IO;

namespace HamsterStudioGUI.ViewModels.SpacialDownloads;

public partial class DirectLinkDownloadViewModel : SpDownloadsViewModel
{
    [ObservableProperty]
    public string _url = string.Empty;

    [ObservableProperty]
    public string _fileName = string.Empty;

    private Lazy<CommonDownloader> _CommonDownloader = new(() => App.ResloveService<CommonDownloader>());
    private DirectoryMgmt DirectoryMgmt { get; } = App.ResloveService<DirectoryMgmt>();

    public override async Task DownloadExecute()
    {
        try
        {
            string fullPath = Path.Combine(DirectoryMgmt.StorageHome, FileName);
            await _CommonDownloader.Value.EasyDownloadFileAsync(UrlReslover.ResloveUrlProtocol(Url), fullPath);
        }
        catch (Exception ex)
        {
            Status = $"下载失败：{ex.Message}";
            Logger.Shared.Critical(ex);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Url))
        {
            FileName = FileNamingTools.GetFilenameFromUrl(Url);
        }
        base.OnPropertyChanged(e);
    }

}
