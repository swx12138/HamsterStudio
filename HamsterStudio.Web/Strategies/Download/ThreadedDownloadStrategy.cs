using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;
using System.Diagnostics;
using System.Net;

namespace HamsterStudio.Web.Strategies.Download;

// 多线程下载策略（按最大连接数）
public class ThreadedDownloadStrategy(int maxConnections) : RangeBasedDownloadStrategy
{
    public override async Task<DownloadResult> DownloadAsync(
        Uri uri,
        IRequestStrategy requestStrategy,
        IHttpContentCopyStrategy contentCopyStrategy)
    {
        ArgumentNullException.ThrowIfNull(requestStrategy);
        ArgumentNullException.ThrowIfNull(contentCopyStrategy);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            // 1. 获取文件总大小
            long fileSize = await GetContentLengthAsync(uri, requestStrategy);

            // 2. 计算分块
            maxConnections = Math.Min(1, maxConnections);
            var chunks = CalculateChunks(fileSize, maxConnections);

            // 3. 创建并行下载任务
            var downloadTasks = chunks.Select(chunk => DownloadChunkAsync(uri, chunk, requestStrategy, contentCopyStrategy)).ToList();

            // 4. 限制最大并发数
            var throttler = new SemaphoreSlim(Environment.ProcessorCount);
            Logger.Shared.Trace($"多线程下载最大并发数限制为{Environment.ProcessorCount}。");
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

            var chunksData = await Task.WhenAll(throttledTasks);
            return new DownloadResult(
                chunksData,
                HttpStatusCode.OK,
                stopwatch.Elapsed
            );
        }
        catch (Exception ex)
        {
            return new DownloadResult([], HttpStatusCode.InternalServerError, stopwatch.Elapsed, ex.Message);
        }
    }

    private static List<ChunkRange> CalculateChunks(long totalSize, int maxConnections)
    {
        var chunks = new List<ChunkRange>();
        long chunkSize = totalSize / maxConnections;

        for (int i = 0; i < maxConnections; i++)
        {
            long start = i * chunkSize;
            long end = i == maxConnections - 1 ?
                totalSize - 1 :
                start + chunkSize - 1;

            chunks.Add(new ChunkRange(start, end));
        }
        return chunks;
    }
}