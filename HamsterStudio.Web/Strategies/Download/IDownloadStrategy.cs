using HamsterStudio.Web.DataModels;
using System.Net;

namespace HamsterStudio.Web.Strategies.Download;

public record DownloadResult(
    byte[] Data,
    HttpStatusCode StatusCode,
    TimeSpan ?ElapsedTime = null,
    string? ErrorMessage = null
);

// 核心抽象层（符合DIP）
public interface IDownloadStrategy // 策略模式接口（符合OCP）
{
    Task<DownloadResult> DownloadAsync(DownloadRequest request);
}
