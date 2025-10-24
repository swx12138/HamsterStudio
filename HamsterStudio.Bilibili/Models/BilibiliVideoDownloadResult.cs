using HamsterStudio.Barefeet.Interfaces;
using HamsterStudio.Web.Services;
using HamstertFileInfo = HamsterStudio.Barefeet.FileSystem.HamstertFileInfo;

namespace HamsterStudio.Bilibili.Models;

public class BilibiliVideoDownloadResult
{
    public required HamstertFileInfo VideoDest { get; set; }
    public DownloadStatus State { get; set; } = DownloadStatus.Failed;
    public Exception? Exception { get; set; } = null;

}
