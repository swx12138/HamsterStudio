using HamsterStudio.Web.Interfaces;
using HamsterStudio.Web.Tools.Download;

namespace HamsterStudio.Web.Tools;

// 组合根层（符合SRP）
public class DownloadStrategyFactory // 工厂模式（符合DIP）
{
    private readonly HttpClientFactory _httpClientFactory = new();
    public IDownloadStrategy CreateStrategy(int? maxConnections = 1)
    {
        if (maxConnections > 1)
        {
            return new ThreadedDownloadStrategy(_httpClientFactory);
        }
        else
        {
            return new DirectDownloadStrategy(_httpClientFactory);
        }
    }
}
