using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;
using System;
using System.Net;

namespace HamsterStudio.Web.Strategies.Download;

// 公共基础类（提取重复逻辑）
public abstract class RangeBasedDownloadStrategy(int maxConnections) : IDownloadStrategy
{
    public static async Task<long> GetContentLengthAsync(Uri url, IRequestStrategy requestStrategy)
    {
        using var message = new HttpRequestMessage(HttpMethod.Head, url);
        using var response = await requestStrategy.SendAsync(message);
        var contentLength = response.Content.Headers.ContentLength ?? throw new NotSupportedException("Content-Length header missing");
        Logger.Shared.Trace($"远程文件大小: {FileSizeDescriptor.ToReadableFileSize(contentLength)}({contentLength} 字节)。");
        return contentLength;
    }

    private static readonly SemaphoreSlim throttler = new(Environment.ProcessorCount);

    public bool ShowInfo { get; set; } = true;
    public virtual string Info => $"[分块下载] 最大连接数{maxConnections}。";

    public abstract List<ChunkRange> CalculateChunks(long fileSize);

    public async Task<DownloadResult> DownloadAsync(
        Uri uri,
        IRequestStrategy requestStrategy,
        IHttpContentCopyStrategy contentCopyStrategy)
    {
        ArgumentNullException.ThrowIfNull(requestStrategy);
        ArgumentNullException.ThrowIfNull(contentCopyStrategy);
        ArgumentOutOfRangeException.ThrowIfNegative(maxConnections);

        try
        {
            // 1. 获取文件总大小
            long fileSize = await GetContentLengthAsync(uri, requestStrategy);

            // 2. 计算分块（基于固定分块大小）
            var chunks = CalculateChunks(fileSize);
            if (ShowInfo)
            {
                ShowChunksInfo(fileSize, chunks);
            }

            // 3. 限制最大并发数
            int maxConcurrency = Math.Min(chunks.Count, maxConnections);

            // 4. 创建并行下载任务
            var downloadTasks = chunks
                .Select(chunk => new ChunkDownloadStrategy(chunk))
                .Select(cds => new RetryableDownloadStrategy(cds))
                .Select(rds => rds.DownloadAsync(uri, requestStrategy, contentCopyStrategy))
                .ToList();

            // 5. 使用信号量限制并发数
            var throttledTasks = downloadTasks.Select(async task =>
            {
                await throttler.WaitAsync();
                try
                {
                    return await task;
                }
                finally
                {
                    throttler.Release();
                }
            });

            // 6. 等待所有任务完成并合并数据
            var chunksData = await Task.WhenAll(throttledTasks);
            if (chunksData.Any(x => x.StatusCode != HttpStatusCode.OK))
            {

                return new DownloadResult(
                    chunksData.Where(x => x.StatusCode == HttpStatusCode.OK).SelectMany(x => x.Data).ToArray(),
                    HttpStatusCode.PartialContent,
                     chunksData.Where(x => x.StatusCode == HttpStatusCode.OK).Sum(x => x.TotalBytes)
                );
            }
            else
            {
                return new DownloadResult(
                    chunksData.SelectMany(x => x.Data).ToArray(),
                    HttpStatusCode.OK,
                    fileSize
                );
            }
        }
        catch (Exception ex)
        {
            return new DownloadResult([], HttpStatusCode.InternalServerError, -1, ex.Message);
        }
    }

    public void ShowChunksInfo(long fileSize, List<ChunkRange> chunks)
    {
        var fileSizeStr = fileSize.ToReadableFileSize();
        var lastChunkSizeStr = FileSizeDescriptor.ToReadableFileSize(chunks.Count > 0 ? (chunks[^1].End - chunks[^1].Start + 1) : 0);
        var msg = FormatChunksInfoMessage(fileSizeStr, lastChunkSizeStr, chunks.Count);
        Logger.Shared.Information(msg);
    }

    protected virtual string FormatChunksInfoMessage(string fileSizeStr, string lastChunkSizeStr, long chunksCount)
    {
        return $"[分块信息] 文件大小: {fileSizeStr} ，共 {chunksCount} 块，最后一块大小{lastChunkSizeStr}。";
    }

}
