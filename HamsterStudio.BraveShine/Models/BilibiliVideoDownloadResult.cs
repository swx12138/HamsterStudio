using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.BraveShine.Models;

public enum VideoDownlaodState
{
    Unknown, Succeed, Failed, Existed
}

public class BilibiliVideoDownloadResult
{
    public string VideoName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public VideoDownlaodState State { get; set; } = VideoDownlaodState.Unknown;
    public Exception? Exception { get; set; } = null;

}
