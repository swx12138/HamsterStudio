using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;
using Microsoft.Extensions.Logging;
using System.Net;

namespace HamsterStudio.Web.Strategies.Download;

// 扩展点示例（符合OCP）
public class RetryableDownloadStrategy(IDownloadStrategy innerStrategy, ILogger? logger = null, int maxRetries = 5, TimeSpan? initialDelay = null) : IDownloadStrategy
{
    private readonly TimeSpan _initialDelay = initialDelay ?? TimeSpan.FromSeconds(1);
    private int attempt = 0;

    public string Info => $"[可重试下载 {attempt}/{maxRetries}]";

    public async Task<DownloadResult> DownloadAsync(
        Uri uri,
        IRequestStrategy requestStrategy,
        IHttpContentCopyStrategy contentCopyStrategy)
    {
        Exception lastError = null;
        for (attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                if (innerStrategy is ChunkDownloadStrategy)
                {
                    logger?.LogTrace(Info + innerStrategy.Info);
                }
                else
                {
                    logger?.LogTrace(Info);
                }

                var result = await innerStrategy.DownloadAsync(uri, requestStrategy, contentCopyStrategy);
                if (result.StatusCode.IsSuccess())
                {
                    return result;
                }

                if (!ShouldRetry(result.StatusCode))
                {
                    logger?.LogWarning($"Non-retriable status code: {result.StatusCode}");
                    return result;
                }
            }
            catch (Exception ex) when (IsTransientError(ex))
            {
                lastError = ex;
                logger?.LogWarning($"Transient error occurred: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger?.LogError($"Non-retriable error: {ex.Message}");
                return new DownloadResult([], HttpStatusCode.InternalServerError, -1, ex.Message);
            }

            if (attempt < maxRetries)
            {
                var delay = CalculateBackoffDelay(attempt);
                logger?.LogInformation($"Retrying in {delay.TotalSeconds}s...");
                await Task.Delay(delay);
            }
        }

        return new DownloadResult([new MemoryStream()], HttpStatusCode.RequestTimeout, 0,
            $"Max retry attempts exceeded. Last error: {lastError?.Message}");
    }

    private static bool ShouldRetry(HttpStatusCode statusCode)
    {
        var retriableStatusCodes = new[]
        {
            HttpStatusCode.RequestTimeout,         // 408
            HttpStatusCode.TooManyRequests,       // 429
            HttpStatusCode.InternalServerError,    // 500
            HttpStatusCode.BadGateway,            // 502
            HttpStatusCode.ServiceUnavailable,    // 503
            HttpStatusCode.GatewayTimeout         // 504
        };
        return retriableStatusCodes.Contains(statusCode);
    }

    private static bool IsTransientError(Exception ex)
    {
        return ex is HttpRequestException
            || ex is TimeoutException
            || ex is TaskCanceledException;
    }

    private TimeSpan CalculateBackoffDelay(int attempt)
    {
        // 指数退避 + 随机抖动
        var jitter = new Random().Next(500, 1000);
        var delay = _initialDelay.TotalMilliseconds * Math.Pow(2, attempt) + jitter;
        return TimeSpan.FromMilliseconds(Math.Min(delay, 30000)); // 最大30秒
    }

}