using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Strategies.Request;
using System.Net;

namespace HamsterStudio.Web.Strategies.Download;

// 具体实现层
public class DirectDownloadStrategy : IDownloadStrategy // 简单实现
{
    public async Task<DownloadResult> DownloadAsync(DownloadRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        
        using var response = await request.RequestStrategy.GetResponseAsync(request.Url);
        response.EnsureSuccessStatusCode();

        var data = await request.ContentCopyStrategy.ToByteArrayCopy(response.Content);
        return new DownloadResult(data, HttpStatusCode.OK);
    }
}
