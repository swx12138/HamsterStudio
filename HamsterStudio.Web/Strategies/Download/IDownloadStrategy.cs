using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;
using System.Net;

namespace HamsterStudio.Web.Strategies.Download;

public record DownloadResult(
    Stream[] Data ,
    HttpStatusCode StatusCode,
    TimeSpan ?ElapsedTime = null,
    string? ErrorMessage = null
);

// 核心抽象层（符合DIP）
public interface IDownloadStrategy // 策略模式接口（符合OCP）
{
    Task<DownloadResult> DownloadAsync(
        Uri uri,
        IRequestStrategy requestStrategy,
        IHttpContentCopyStrategy contentCopyStrategy);
}
