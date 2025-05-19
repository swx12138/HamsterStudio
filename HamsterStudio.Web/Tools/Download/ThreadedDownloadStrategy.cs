using HamsterStudio.Web.Interfaces;
using System.Diagnostics;
using System.Net;

namespace HamsterStudio.Web.Tools.Download;

// 多线程下载策略（按最大连接数）
public class ThreadedDownloadStrategy(HttpClientFactory httpClientFactory) : RangeBasedDownloadStrategy(httpClientFactory)
{
    public override async Task<DownloadResult> DownloadAsync(DownloadRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 1. 获取文件总大小
            long fileSize = await GetContentLengthAsync(request.Url, request.Headers);
            int maxConnections = request.MaxConnections ?? 4;

            // 2. 计算分块
            var chunks = CalculateChunks(fileSize, maxConnections);

            // 3. 创建并行下载任务
            var downloadTasks = chunks.Select(chunk =>
                DownloadChunkAsync(request.Url, request.Headers, chunk)).ToList();

            // 4. 限制最大并发数
            var throttler = new SemaphoreSlim(maxConnections);
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

            // 5. 合并数据
            var mergedData = MergeChunks(chunksData);

            return new DownloadResult(
                mergedData,
                HttpStatusCode.OK,
                stopwatch.Elapsed
            );
        }
        catch (Exception ex)
        {
            return new DownloadResult(
                Array.Empty<byte>(),
                HttpStatusCode.InternalServerError,
                stopwatch.Elapsed,
                ex.Message
            );
        }
    }

    private static List<ChunkRange> CalculateChunks(long totalSize, int maxConnections)
    {
        var chunks = new List<ChunkRange>();
        long chunkSize = totalSize / maxConnections;

        for (int i = 0; i < maxConnections; i++)
        {
            long start = i * chunkSize;
            long end = (i == maxConnections - 1) ?
                totalSize - 1 :
                start + chunkSize - 1;

            chunks.Add(new ChunkRange(start, end));
        }
        return chunks;
    }
}