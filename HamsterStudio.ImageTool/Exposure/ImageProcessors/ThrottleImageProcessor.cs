using Microsoft.Extensions.Logging;
using System.Windows.Media.Imaging;

namespace HamsterStudio.ImageTool.Exposure.ImageProcessors;

public class ThrottleImageProcessor(IImageProcessor innerProcessor, ILogger? logger = null) : IImageProcessor
{
    private DateTime _lastProcessTime = DateTime.MinValue;
    private readonly TimeSpan _throttleInterval = TimeSpan.FromMilliseconds(200);
    private CancellationTokenSource _applyCts = new();

    public async Task<BitmapSource?> ApplyAdjustmentsAsync(BitmapSource source, ImageAdjustments adjustments)
    {
        // 取消之前的任务
        _applyCts?.Cancel();
        _applyCts = new CancellationTokenSource();

        await Task.Delay(_throttleInterval, _applyCts.Token); // 去抖延迟
        if (!_applyCts.Token.IsCancellationRequested)
        {
            _lastProcessTime = DateTime.Now;
            return innerProcessor.ApplyAdjustments(source, adjustments);
        }

        return null;
    }

    public BitmapSource ApplyAdjustments(BitmapSource source, ImageAdjustments adjustments)
    {
        var now = DateTime.Now;
        if (now - _lastProcessTime < _throttleInterval)
        {
            return source;
        }
        _lastProcessTime = now;
        return innerProcessor.ApplyAdjustments(source, adjustments);
    }

    public HistogramData CalculateHistogram(BitmapSource source)
    {
        return innerProcessor.CalculateHistogram(source);
    }
}
