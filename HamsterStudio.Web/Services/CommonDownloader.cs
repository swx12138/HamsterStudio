using HamsterStudio.Barefeet.Constants;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.Strategies;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Tools;
using System.Net;

namespace HamsterStudio.Web.Services;

public class CommonDownloader(HttpClientProvider httpClientProvider)
{
    public async Task<bool> DownloadFileAsync(DownloadRequest request, string destinationPath)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        ArgumentException.ThrowIfNullOrEmpty(destinationPath, nameof(destinationPath));
        if (File.Exists(destinationPath))
        {
            Logger.Shared.Warning($"File already exists at {destinationPath}. Skipped.");
            return true;
        }

        var requestStrategy = request.RequestStrategy ?? new AuthenticRequestStrategy(httpClientProvider.HttpClient);
        var downloadStrategy = DownloadStrategyFactory.CreateStrategy(request.MaxConnections);
        try
        {
            var result = await downloadStrategy.DownloadAsync(request);
            if (result.StatusCode != HttpStatusCode.OK)
            {
                Logger.Shared.Error($"Failed to download file: {result.StatusCode} - {result.ErrorMessage}");
                return false;
            }

            await File.WriteAllBytesAsync(destinationPath, result.Data);
            Logger.Shared.Trace($"{Path.GetFileName(destinationPath)} 成功下载到 {destinationPath}.");

            return true;
        }
        catch (Exception ex)
        {
            Logger.Shared.Warning($"Error downloading file: {ex.Message}");
            Logger.Shared.Debug(ex);
            return false;
        }
    }

    public async Task<bool> EasyDownloadFileAsync(Uri uri, string destinationPath, bool concurrent = false)
    {
        var downloadRequest = new DownloadRequest(uri, new AuthenticRequestStrategy(httpClientProvider.HttpClient), concurrent ? 4 : 1);
        return await DownloadFileAsync(downloadRequest, destinationPath);
    }
}
