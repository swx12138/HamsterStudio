using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;
using System.Diagnostics;
using System.Net;

namespace HamsterStudio.Web.Strategies.Download;

// 固定分块大小下载策略（自动计算线程数，最大并发数限制为处理器数量）
public class FixedChunkSizeDownloadStrategy(int chunkSize, int maxConnections) : RangeBasedDownloadStrategy
{
    public override async Task<DownloadResult> DownloadAsync(
        Uri uri,
        IRequestStrategy requestStrategy,
        IHttpContentCopyStrategy contentCopyStrategy)
    {
        ArgumentNullException.ThrowIfNull(requestStrategy);
        ArgumentNullException.ThrowIfNull(contentCopyStrategy);
        ArgumentOutOfRangeException.ThrowIfNegative(maxConnections);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            // 1. 获取文件总大小
            long fileSize = await GetContentLengthAsync(uri, requestStrategy);

            // 2. 计算分块（基于固定分块大小）
            var chunks = CalculateChunksBySize(fileSize, chunkSize);

            // 3. 限制最大并发数
            int maxConcurrency = Math.Min(chunks.Count, maxConnections);

            // 4. 创建并行下载任务
            var downloadTasks = chunks.Select(chunk =>
                DownloadChunkAsync(uri, chunk, requestStrategy, contentCopyStrategy))
                .ToList();

            // 5. 使用信号量限制并发数
            var throttler = new SemaphoreSlim(maxConcurrency);
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

    /// <summary>
    /// 根据固定分块大小计算分块范围
    /// </summary>
    private static List<ChunkRange> CalculateChunksBySize(long totalSize, long chunkSize)
    {
        var chunks = new List<ChunkRange>();

        for (long start = 0; start < totalSize; start += chunkSize)
        {
            long end = Math.Min(start + chunkSize - 1, totalSize - 1);
            chunks.Add(new ChunkRange(start, end));
        }

        return chunks;
    }

}