using HamsterStudio.Barefeet.Interfaces;
using FileInfo = HamsterStudio.Barefeet.FileSystem.FileInfo;

namespace HamsterStudio.Bilibili.Models;

public class BilibiliVideoDownloadResult
{
    public required FileInfo VideoDest { get; set; }
    public FileDownloadState State { get; set; } = FileDownloadState.Unknown;
    public Exception? Exception { get; set; } = null;

}
