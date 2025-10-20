using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Strategies;
using HamsterStudio.Web.Strategies.Download;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;
using System.Net;

namespace HamsterStudio.Web.Services;

public class CommonDownloader(HttpClientProvider httpClientProvider)
{
    public async Task<bool> DownloadFileAsync(
        Uri uri,
        string destinationPath,
        IRequestStrategy requestStrategy,
        IHttpContentCopyStrategy contentCopyStrategy,
        IDownloadStrategy downloadStrategy)
    {
        ArgumentNullException.ThrowIfNull(requestStrategy);
        ArgumentNullException.ThrowIfNull(contentCopyStrategy);
        ArgumentNullException.ThrowIfNull(downloadStrategy);

        ArgumentException.ThrowIfNullOrEmpty(destinationPath, nameof(destinationPath));
        if (File.Exists(destinationPath))
        {
            Logger.Shared.Information($"File already exists at {destinationPath}. Skipped.");
            return true;
        }

        requestStrategy ??= new AuthenticRequestStrategy(httpClientProvider.HttpClient);
        try
        {
            var result = await downloadStrategy.DownloadAsync(uri, requestStrategy, contentCopyStrategy);
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

    public async Task<bool> EasyDownloadFileAsync(Uri uri, string destinationPath, int trunckSize = 0, bool concurrent = false)
    {
        var requestStrategy = new AuthenticRequestStrategy(httpClientProvider.HttpClient);
        var copyStrategy = new DirectHttpContentCopyStrategy();
        var downloadStrategy = DownloadStrategyFactory.CreateStrategy(trunckSize, concurrent ? Environment.ProcessorCount : 1);
        return await DownloadFileAsync(uri, destinationPath, requestStrategy, copyStrategy, downloadStrategy);
    }
}
