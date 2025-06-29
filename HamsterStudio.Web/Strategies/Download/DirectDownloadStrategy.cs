using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Tools;
using System.Net;

namespace HamsterStudio.Web.Strategies.Download;

// 具体实现层
public class DirectDownloadStrategy : IDownloadStrategy // 简单实现
{
    public async Task<DownloadResult> DownloadAsync(DownloadRequest request)
    {
        using var response = await request.RequestStrategy.GetResponseAsync(request.Url);
        using var memStream = new MemoryStream();
        await response.Content.CopyToAsync(memStream);
        return new DownloadResult(
            Data: memStream.ToArray(),
            StatusCode: HttpStatusCode.OK
        );
    }
}
