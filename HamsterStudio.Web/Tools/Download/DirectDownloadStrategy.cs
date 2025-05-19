using HamsterStudio.Web.Interfaces;
using System.Net;

namespace HamsterStudio.Web.Tools.Download;

// 具体实现层
public class DirectDownloadStrategy(HttpClientFactory httpClientFactory) : IDownloadStrategy // 简单实现
{
    public async Task<DownloadResult> DownloadAsync(DownloadRequest request)
    {
        var client = httpClientFactory.CreateHttpClient(request.Headers);
        var stream = await client.GetStreamAsync(request.Url);
        using var memStream = new MemoryStream();
        await stream.CopyToAsync(memStream);
        return new DownloadResult(
            Data: memStream.ToArray(),
            StatusCode: HttpStatusCode.OK
        );
    }
}
