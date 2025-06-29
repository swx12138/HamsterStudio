using HamsterStudio.Web.Strategies.Download;

namespace HamsterStudio.Web.Strategies;

// 组合根层（符合SRP）
public static class DownloadStrategyFactory // 工厂模式（符合DIP）
{
    public static IDownloadStrategy CreateStrategy(int? maxConnections = 1)
    {
        if (maxConnections > 1)
        {
            return new ThreadedDownloadStrategy();
        }
        else
        {
            return new DirectDownloadStrategy();
        }
    }
}
