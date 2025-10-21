using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;
using System.Net;

namespace HamsterStudio.Web.Strategies.Download;

// 具体实现层
public class DirectDownloadStrategy : IDownloadStrategy // 简单实现
{
    public string Info => throw new NotImplementedException();

    public async Task<DownloadResult> DownloadAsync(
        Uri uri,
        IRequestStrategy requestStrategy,
        IHttpContentCopyStrategy contentCopyStrategy)
    {
        ArgumentNullException.ThrowIfNull(requestStrategy);
        ArgumentNullException.ThrowIfNull(contentCopyStrategy);

        using var response = await requestStrategy.GetResponseAsync(uri);
        response.EnsureSuccessStatusCode();

        var data = await contentCopyStrategy.CopyToStream(response.Content);
        return new DownloadResult([data], HttpStatusCode.OK, data.Length);
    }

}
