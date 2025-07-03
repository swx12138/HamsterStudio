using HamsterStudio.Barefeet.Interfaces;
using HamstertFileInfo = HamsterStudio.Barefeet.FileSystem.HamstertFileInfo;

namespace HamsterStudio.Bilibili.Models;

public class BilibiliVideoDownloadResult
{
    public required HamstertFileInfo VideoDest { get; set; }
    public FileDownloadState State { get; set; } = FileDownloadState.Unknown;
    public Exception? Exception { get; set; } = null;

}
