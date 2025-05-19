using HamsterStudio.Barefeet.Interfaces;

namespace HamsterStudio.BraveShine.Models;

public class BilibiliVideoDownloadResult
{
    public string VideoName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public FileDownlaodState State { get; set; } = FileDownlaodState.Unknown;
    public Exception? Exception { get; set; } = null;

}
