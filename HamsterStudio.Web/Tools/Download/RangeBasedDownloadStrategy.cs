using HamsterStudio.Web.Interfaces;
using System.Net.Http.Headers;
using System.Net;

namespace HamsterStudio.Web.Tools.Download;

// 公共基础类（提取重复逻辑）
public abstract class RangeBasedDownloadStrategy(HttpClientFactory httpClientFactory) : IDownloadStrategy
{
    protected record ChunkRange(long Start, long End);

    protected async Task<long> GetContentLengthAsync(Uri url, Dictionary<string, string> headers)
    {
        using var client = httpClientFactory.CreateHttpClient(headers);
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
        return response.Content.Headers.ContentLength ?? throw new NotSupportedException("Content-Length header missing");
    }

    protected async Task<byte[]> DownloadChunkAsync(Uri url, Dictionary<string, string> headers, ChunkRange range)
    {
        using var client = httpClientFactory.CreateHttpClient(headers);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Range = new RangeHeaderValue(range.Start, range.End);

        using var response = await client.SendAsync(request);
        if (response.StatusCode != HttpStatusCode.PartialContent)
            throw new HttpRequestException($"Unexpected status code: {response.StatusCode}");

        return await response.Content.ReadAsByteArrayAsync();
    }

    protected byte[] MergeChunks(IEnumerable<byte[]> chunks)
    {
        var merged = new List<byte>();
        foreach (var chunk in chunks)
        {
            merged.AddRange(chunk);
        }
        return merged.ToArray();
    }

    public abstract Task<DownloadResult> DownloadAsync(DownloadRequest request);
}
