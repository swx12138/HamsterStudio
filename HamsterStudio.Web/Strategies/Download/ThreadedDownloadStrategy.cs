namespace HamsterStudio.Web.Strategies.Download;

// 多线程下载策略（按最大连接数）
public class ThreadedDownloadStrategy(int maxConnections) : RangeBasedDownloadStrategy(maxConnections)
{
    public override List<ChunkRange> CalculateChunks(long totalSize)
    {
        var chunks = new List<ChunkRange>();
        long chunkSize = totalSize / MaxConnections;

        for (int i = 0; i < MaxConnections; i++)
        {
            long start = i * chunkSize;
            long end = i == MaxConnections - 1 ?
                totalSize - 1 :
                start + chunkSize - 1;

            chunks.Add(new ChunkRange(start, end));
        }
        return chunks;
    }

}