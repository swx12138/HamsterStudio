using HamsterStudio.Web.Interfaces;

namespace HamsterStudio.Web.Tools;

// 客户端使用（符合LSP）
public class Downloader(IDownloadStrategy strategy)
{
    public async Task<DownloadResult> ExecuteDownload(DownloadRequest request)
    {
        return await strategy.DownloadAsync(request);
    }
}
