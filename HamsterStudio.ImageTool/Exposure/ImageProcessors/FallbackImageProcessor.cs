using Microsoft.Extensions.Logging;
using System.Windows.Media.Imaging;

namespace HamsterStudio.ImageTool.Exposure.ImageProcessors;

public class FallbackImageProcessor(IImageProcessor innerProcessor, FallbackImageProcessor? next = null, ILogger? logger = null) : IImageProcessor
{
    public BitmapSource ApplyAdjustments(BitmapSource source, ImageAdjustments adjustments)
    {
        try
        {
            return innerProcessor.ApplyAdjustments(source, adjustments);
        }
        catch
        {
            if (next != null)
            {
                return next.ApplyAdjustments(source, adjustments);
            }
        }

        logger?.LogWarning($"Image processing failed in {innerProcessor.GetType().Name}. Falling back to next processor.");
        return source; // 如果所有处理器都失败，返回原图
    }

    public HistogramData CalculateHistogram(BitmapSource source)
    {
        // 这里可以实现一个简单的直方图计算算法，或者返回一个空的占位符
        return new HistogramData();
    }
}
