using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Tools;
using System.Diagnostics;
using System.Net;

namespace HamsterStudio.Web.Strategies.Download;

// 多线程下载策略（按最大连接数）
public class ThreadedDownloadStrategy : RangeBasedDownloadStrategy
{
    public override async Task<DownloadResult> DownloadAsync(DownloadRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            // 1. 获取文件总大小
            long fileSize = await GetContentLengthAsync(request.Url, request.RequestStrategy);

            // 2. 计算分块
            var chunks = CalculateChunks(fileSize, request.MaxConnections);

            // 3. 创建并行下载任务
            var downloadTasks = chunks.Select(chunk => DownloadChunkAsync(request.Url, chunk, request.RequestStrategy)).ToList();

            // 4. 限制最大并发数
            var throttler = new SemaphoreSlim(request.MaxConnections);
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
            var mergedData = MergeChunks(chunksData);

            return new DownloadResult(
                mergedData,
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