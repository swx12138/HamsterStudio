using HamsterStudio.Web.Tools;
using System.Net;

namespace HamsterStudio.Web.Interfaces;

// 在DownloadResult中改用Stream类型
public record DownloadResult(
    byte[] Data,  // 替换byte[]
    HttpStatusCode StatusCode,
    TimeSpan ?ElapsedTime = null,
    string? ErrorMessage = null
);

// 核心抽象层（符合DIP）
public interface IDownloadStrategy // 策略模式接口（符合OCP）
{
    Task<DownloadResult> DownloadAsync(DownloadRequest request);
}
