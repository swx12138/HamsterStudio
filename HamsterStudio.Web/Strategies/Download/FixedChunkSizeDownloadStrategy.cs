using HamsterStudio.Barefeet.FileSystem;

namespace HamsterStudio.Web.Strategies.Download;

// 固定分块大小下载策略
public class FixedChunkSizeDownloadStrategy(long chunkSize, int maxConnections) : RangeBasedDownloadStrategy(maxConnections)
{
    public string ChunkSizeInfo { get; } = $"默认分块大小{chunkSize.ToReadableFileSize()}。";
    public override string Info => base.Info + ChunkSizeInfo;

    /// <summary>
    /// 根据固定分块大小计算分块范围
    /// </summary>
    public override List<ChunkRange> CalculateChunks(long totalSize)
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

    protected override string FormatChunksInfoMessage(string fileSizeStr, string lastChunkSizeStr, long chunksCount)
    {
        var chunkSizeStr = FileSizeDescriptor.ToReadableFileSize(chunkSize);
        return base.FormatChunksInfoMessage(fileSizeStr, lastChunkSizeStr, chunksCount) + ChunkSizeInfo;
    }

}
