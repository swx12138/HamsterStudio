using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Strategies.StreamCopy;
using System.Diagnostics;
using System.Net;

namespace HamsterStudio.Web.Strategies.Download;

// 扩展点示例（符合OCP）
public class RetryableDownloadStrategy(IDownloadStrategy innerStrategy, int maxRetries = 3, TimeSpan? initialDelay = null) : IDownloadStrategy
{
    private readonly TimeSpan _initialDelay = initialDelay ?? TimeSpan.FromSeconds(1);
    private readonly Logger _logger = Logger.Shared;

    public async Task<DownloadResult> DownloadAsync(
        Uri uri,
        IRequestStrategy requestStrategy,
        IHttpContentCopyStrategy contentCopyStrategy)
    {
        int attempt = 0;
        Exception lastError = null;
        var totalStopwatch = Stopwatch.StartNew();

        while (attempt <= maxRetries)
        {
            var attemptStopwatch = Stopwatch.StartNew();
            try
            {
                _logger.Information($"Download attempt {attempt + 1}/{maxRetries + 1}");

                var result = await innerStrategy.DownloadAsync(uri, requestStrategy, contentCopyStrategy);

                if (result.StatusCode.IsSuccess())
                {
                    _logger.Information($"Download succeeded after {attempt} retries");
                    return result with { ElapsedTime = totalStopwatch.Elapsed };
                }

                if (!ShouldRetry(result.StatusCode))
                {
                    _logger.Warning($"Non-retriable status code: {result.StatusCode}");
                    return result with { ElapsedTime = totalStopwatch.Elapsed };
                }
            }
            catch (Exception ex) when (IsTransientError(ex))
            {
                lastError = ex;
                _logger.Warning($"Transient error occurred: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Non-retriable error: {ex.Message}");
                return new DownloadResult(
                   [],
                    HttpStatusCode.InternalServerError,
                    totalStopwatch.Elapsed,
                    ex.Message
                );
            }

            if (attempt < maxRetries)
            {
                var delay = CalculateBackoffDelay(attempt);
                _logger.Information($"Retrying in {delay.TotalSeconds}s...");
                await Task.Delay(delay);
            }

            attempt++;
            attemptStopwatch.Stop();
        }

        return new DownloadResult(
            [new MemoryStream()],
            HttpStatusCode.RequestTimeout,
            totalStopwatch.Elapsed,
            $"Max retry attempts exceeded. Last error: {lastError?.Message}"
        );
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