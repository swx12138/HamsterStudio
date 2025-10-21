using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;
using System.Net;
using System.Net.Http.Headers;

namespace HamsterStudio.Web.Strategies.Download;

public record ChunkRange(long Start, long End)
{
    public long Length => End - Start + 1;
    public string Info => $"块起始：{Start.ToReadableOffest()}, 块结束：{End.ToReadableOffest()}]，总计大小{Length.ToReadableFileSize()}";
}

public class ChunkDownloadStrategy(ChunkRange range) : IDownloadStrategy
{
    public string Info => $"[单块下载] {range.Info}。";

    public async Task<DownloadResult> DownloadAsync(Uri uri, IRequestStrategy requestStrategy, IHttpContentCopyStrategy contentCopyStrategy)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Range = new RangeHeaderValue(range.Start, range.End);

        using var response = await requestStrategy.SendAsync(request);
        if (response.StatusCode != HttpStatusCode.PartialContent)
            throw new HttpRequestException($"Unexpected status code: {response.StatusCode}");

        var stream = await contentCopyStrategy.CopyToStream(response.Content);
        return new DownloadResult([stream], HttpStatusCode.OK, range.Length);
    }
}
