using HamsterStudio.Web.Interfaces;
using System.Diagnostics;
using System.Net;

namespace HamsterStudio.Web.Tools.Download;

// 分块下载策略（按固定块大小）
public class ChunkedDownloadStrategy(HttpClientFactory httpClientFactory) : RangeBasedDownloadStrategy(httpClientFactory)
{
    public override async Task<DownloadResult> DownloadAsync(DownloadRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 1. 获取文件总大小
            long fileSize = await GetContentLengthAsync(request.Url, request.Headers);
            int chunkSize = request.ChunkSize ?? 1024 * 1024; // 默认1MB

            // 2. 计算分块
            var chunks = CalculateChunks(fileSize, chunkSize);

            // 3. 并行下载所有分块
            var downloadTasks = chunks.Select(chunk =>
                DownloadChunkAsync(request.Url, request.Headers, chunk)).ToList();

            var chunksData = await Task.WhenAll(downloadTasks);

            // 4. 合并数据
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
                [],
                HttpStatusCode.InternalServerError,
                stopwatch.Elapsed,
                ex.Message
            );
        }
    }

    private List<ChunkRange> CalculateChunks(long totalSize, int chunkSize)
    {
        var chunks = new List<ChunkRange>();
        for (long offset = 0; offset < totalSize; offset += chunkSize)
        {
            long end = Math.Min(offset + chunkSize - 1, totalSize - 1);
            chunks.Add(new ChunkRange(offset, end));
        }
        return chunks;
    }
}
