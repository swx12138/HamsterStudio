using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;
using System.Diagnostics;
using System.Net;

namespace HamsterStudio.Web.Strategies.Download;

// 多线程下载策略（按最大连接数）
public class ThreadedDownloadStrategy(int maxConnections) : RangeBasedDownloadStrategy(maxConnections)
{
    public override List<ChunkRange> CalculateChunks(long totalSize)
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