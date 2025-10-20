using HamsterStudio.Web.Strategies.Download;

namespace HamsterStudio.Web.Strategies;

// 组合根层（符合SRP）
public static class DownloadStrategyFactory // 工厂模式（符合DIP）
{
    public static IDownloadStrategy CreateStrategy(int chunkSize, int maxConnections)
    {
        if (chunkSize > 0)
        {
            Console.WriteLine($"使用分块下载，块大小={chunkSize}， 最大并发数量={maxConnections}");
            return new FixedChunkSizeDownloadStrategy(chunkSize, maxConnections);
        }
        else if (maxConnections > 1)
        {
            Console.WriteLine($"使用多线程下载，最大并发数量={maxConnections}");
            return new ThreadedDownloadStrategy(maxConnections);
        }
        else
        {
            Console.WriteLine($"使用直接下载。");
            return new DirectDownloadStrategy();
        }
    }

}
