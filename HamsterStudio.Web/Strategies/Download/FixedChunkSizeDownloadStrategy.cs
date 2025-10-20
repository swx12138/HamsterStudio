using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;
using System.Diagnostics;
using System.Net;

namespace HamsterStudio.Web.Strategies.Download;

// 固定分块大小下载策略（自动计算线程数，最大并发数限制为处理器数量）
public class FixedChunkSizeDownloadStrategy(int chunkSize, int maxConnections) : RangeBasedDownloadStrategy
{
    private static SemaphoreSlim throttler = new(Environment.ProcessorCount);

    public bool ShowTruncks { get; set; } = true;

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
            if (ShowTruncks)
            {
                var fileSizeStr = FileSizeDescriptor.ToReadableFileSize(fileSize);
                var chunkSizeStr = FileSizeDescriptor.ToReadableFileSize(chunkSize);
                var lastChunkSize = FileSizeDescriptor.ToReadableFileSize(chunks.Count > 0 ? (chunks[^1].End - chunks[^1].Start + 1) : 0);
                Logger.Shared.Information($"[分块下载] 文件大小: {fileSizeStr} ，分块大小: {chunkSizeStr} 字节，共 {chunks.Count} 块，最后一块大小{lastChunkSize}。");
#if DEBUG
                if(chunks.Count > 0)
                {
                    for (int i = 0; i < chunks.Count; i++)
                    {
                        var chunk = chunks[i];
                        var currentChunkSize = chunk.End - chunk.Start + 1;
                        var currentChunkSizeStr = FileSizeDescriptor.ToReadableFileSize(currentChunkSize);
                        Logger.Shared.Information($"  块 {i + 1}: 范围 [{chunk.Start}, {chunk.End}]，大小: {currentChunkSizeStr}");
                    }
                }
#endif
            }

            // 3. 限制最大并发数
            int maxConcurrency = Math.Min(chunks.Count, maxConnections);

            // 4. 创建并行下载任务
            var downloadTasks = chunks.Select(chunk =>
                DownloadChunkAsync(uri, chunk, requestStrategy, contentCopyStrategy))
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

    /// <summary>
    /// 根据固定分块大小计算分块范围
    /// </summary>
    private static List<ChunkRange> CalculateChunksBySize(long totalSize, long chunkSize)
    {
        var chunks = new List<ChunkRange>();

        if (totalSize <= 0 || chunkSize <= 0)
            return chunks;

        // 计算总chunk数
        long totalChunks = (totalSize + chunkSize - 1) / chunkSize;

        if (totalChunks < 2)
        {
            // 少于2个chunk，直接返回
            if (totalSize > 0)
            {
                chunks.Add(new ChunkRange(0, totalSize - 1));
            }
            return chunks;
        }

        // 计算前n-2个chunk
        long currentStart = 0;
        for (int i = 0; i < totalChunks - 2; i++)
        {
            long end = currentStart + chunkSize - 1;
            chunks.Add(new ChunkRange(currentStart, end));
            currentStart = end + 1;
        }

        // 计算剩余大小并均衡分配
        long remainingSize = totalSize - currentStart;

        if (remainingSize <= 0)
            return chunks;

        // 均衡分配最后两个chunk
        long firstPart = (remainingSize + 1) / 2;  // 向上取整
        long secondPart = remainingSize - firstPart;

        // 确保每个chunk至少有一个字节
        if (firstPart == 0 || secondPart == 0)
        {
            // 如果剩余大小很小，直接分成两个chunk
            firstPart = 1;
            secondPart = remainingSize - 1;
            if (secondPart < 1)
            {
                // 如果只有一个字节，只创建一个chunk
                chunks.Add(new ChunkRange(currentStart, totalSize - 1));
                return chunks;
            }
        }

        chunks.Add(new ChunkRange(currentStart, currentStart + firstPart - 1));
        chunks.Add(new ChunkRange(currentStart + firstPart, totalSize - 1));

        return chunks;
    }

}