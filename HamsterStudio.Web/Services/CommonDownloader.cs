using HamsterStudio.Barefeet;
using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.Web.Strategies;
using HamsterStudio.Web.Strategies.Download;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;

namespace HamsterStudio.Web.Services;

public enum DownloadStatus
{
    Success, Exists, Failed
}

public class CommonDownloader(HttpClientProvider httpClientProvider, ILogger<CommonDownloader> logger)
{
    public async Task<DownloadStatus> DownloadFileAsync(
        Uri uri,
        string destinationPath,
        IRequestStrategy? requestStrategy,
        IHttpContentCopyStrategy? contentCopyStrategy,
        IDownloadStrategy downloadStrategy,
        MediaShape? shape = null)
    {
        //ArgumentNullException.ThrowIfNull(requestStrategy);
        //ArgumentNullException.ThrowIfNull(contentCopyStrategy);
        ArgumentNullException.ThrowIfNull(downloadStrategy);

        ArgumentException.ThrowIfNullOrEmpty(destinationPath, nameof(destinationPath));
        if (File.Exists(destinationPath))
        {
            logger.LogInformation(formatExistsMessage(destinationPath, shape));
            return DownloadStatus.Exists;
        }

        requestStrategy ??= new AuthenticRequestStrategy(httpClientProvider.HttpClient);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            contentCopyStrategy ??= new FileStreamHttpContentCopyStrategy();
            var result = await downloadStrategy.DownloadAsync(uri, requestStrategy, contentCopyStrategy);
            if (result.StatusCode != HttpStatusCode.OK)
            {
                ShellApi.OpenBrowser(uri.AbsoluteUri);
                logger.LogError($"Failed to download file: {result.StatusCode} - {result.ErrorMessage}");
                return DownloadStatus.Failed;
            }

            using (var oFileStream = File.OpenWrite(destinationPath))
            {
                foreach (var stream in result.Data)
                {
                    if (stream.CanSeek)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                    }

                    await stream.CopyToAsync(oFileStream);
                    stream.Dispose();
                }
                await oFileStream.FlushAsync();
            }

            var bytePerSecond = (long)(result.TotalBytes / stopwatch.Elapsed.TotalSeconds);
            var bytePerSecondStr = FileSizeDescriptor.ToReadableFileSize(bytePerSecond);
            var fileSizeStr = FileSizeDescriptor.ToReadableFileSize(result.TotalBytes);

            logger.LogInformation(formatMessage(destinationPath, fileSizeStr, bytePerSecondStr, shape));

            return DownloadStatus.Success;
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Error downloading file: {ex.Message}");
            logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
            return DownloadStatus.Failed;
        }

        string formatMessage(string destinationPath, string fileSizeStr, string bytePerSecondStr, MediaShape? shape)
        {
            if (shape != null)
            {
                return $"下载文件 {Path.GetFileName(destinationPath)} 成功，文件大小{fileSizeStr}({shape.Width}*{shape.Height})，平均速度{bytePerSecondStr}/s.";
            }
            else
            {
                return $"下载文件 {Path.GetFileName(destinationPath)} 成功，文件大小{fileSizeStr}，平均速度{bytePerSecondStr}/s.";
            }
        }

        string formatExistsMessage(string destinationPath, MediaShape? shape)
        {
            if (shape != null)
            {

                return $"文件 {destinationPath} 已存在({shape.Width}*{shape.Height}).";
            }
            else
            {
                return $"文件 {destinationPath} 已存在.";
            }
        }

    }

    public async Task<DownloadStatus> EasyDownloadFileAsync(Uri uri, string destinationPath, int trunckSize = 0, bool concurrent = false, MediaShape? shape = null)
    {
        var downloadStrategy = DownloadStrategyFactory.CreateStrategy(trunckSize, concurrent ? Environment.ProcessorCount : 1);
        return await DownloadFileAsync(uri, destinationPath, null, null, downloadStrategy, shape);
    }

    public async Task<DownloadStatus> AuthenticatedDownloadFileAsync(Uri uri, string destinationPath, AuthenticRequestStrategy strategy, int trunckSize = 0, bool concurrent = false, MediaShape? shape = null)
    {
        var downloadStrategy = DownloadStrategyFactory.CreateStrategy(trunckSize, concurrent ? Environment.ProcessorCount : 1);
        return await DownloadFileAsync(uri, destinationPath, strategy, null, downloadStrategy, shape);
    }

}
