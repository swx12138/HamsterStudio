using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Strategies;
using HamsterStudio.Web.Strategies.Download;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;
using System.Diagnostics;
using System.Net;

namespace HamsterStudio.Web.Services;

public enum DownloadStatus
{
    Success, Exists, Failed
}

public class CommonDownloader(HttpClientProvider httpClientProvider)
{
    public async Task<DownloadStatus> DownloadFileAsync(
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
            Logger.Shared.Information($"文件 {destinationPath} 已存在.");
            return DownloadStatus.Exists;
        }

        requestStrategy ??= new AuthenticRequestStrategy(httpClientProvider.HttpClient);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await downloadStrategy.DownloadAsync(uri, requestStrategy, contentCopyStrategy);
            if (result.StatusCode != HttpStatusCode.OK)
            {
                ShellApi.OpenBrowser(uri.AbsoluteUri);
                Logger.Shared.Error($"Failed to download file: {result.StatusCode} - {result.ErrorMessage}");
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
            Logger.Shared.Information($"下载文件 {Path.GetFileName(destinationPath)} 成功，文件大小{fileSizeStr}，平均速度{bytePerSecondStr}/s.");

            return DownloadStatus.Success;
        }
        catch (Exception ex)
        {
            Logger.Shared.Warning($"Error downloading file: {ex.Message}");
            Logger.Shared.Debug(ex);
            return DownloadStatus.Failed;
        }
    }

    public async Task<DownloadStatus> EasyDownloadFileAsync(Uri uri, string destinationPath, int trunckSize = 0, bool concurrent = false)
    {
        var requestStrategy = new AuthenticRequestStrategy(httpClientProvider.HttpClient);
        var copyStrategy = new FileStreamHttpContentCopyStrategy();
        var downloadStrategy = DownloadStrategyFactory.CreateStrategy(trunckSize, concurrent ? Environment.ProcessorCount : 1);
        return await DownloadFileAsync(uri, destinationPath, requestStrategy, copyStrategy, downloadStrategy);
    }

}
