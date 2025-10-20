using System.Net.Http.Headers;
using System.Net;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Strategies.StreamCopy;

namespace HamsterStudio.Web.Strategies.Download;

// 公共基础类（提取重复逻辑）
public abstract class RangeBasedDownloadStrategy : IDownloadStrategy
{
    protected record ChunkRange(long Start, long End);

    protected async Task<long> GetContentLengthAsync(Uri url, IRequestStrategy requestStrategy)
    {
        using var message = new HttpRequestMessage(HttpMethod.Head, url);
        using var response = await requestStrategy.SendAsync(message);
        return response.Content.Headers.ContentLength ?? throw new NotSupportedException("Content-Length header missing");
    }

    protected async Task<Stream> DownloadChunkAsync(Uri url, ChunkRange range, IRequestStrategy requestStrategy, IHttpContentCopyStrategy copyStrategy)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Range = new RangeHeaderValue(range.Start, range.End);

        using var response = await requestStrategy.SendAsync(request);
        if (response.StatusCode != HttpStatusCode.PartialContent)
            throw new HttpRequestException($"Unexpected status code: {response.StatusCode}");

        return await copyStrategy.CopyToStream(response.Content);
    }

    public abstract Task<DownloadResult> DownloadAsync(
        Uri uri,
        IRequestStrategy requestStrategy,
        IHttpContentCopyStrategy contentCopyStrategy);

}
