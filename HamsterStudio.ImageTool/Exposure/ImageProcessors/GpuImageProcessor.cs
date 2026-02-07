using System.Windows.Media.Imaging;

namespace HamsterStudio.ImageTool.Exposure.ImageProcessors;

// GPU加速
public class GpuImageProcessor : IImageProcessor
{
    public BitmapSource ApplyAdjustments(BitmapSource source, ImageAdjustments adjustments)
    {
        throw new NotImplementedException();
    }

    public HistogramData CalculateHistogram(BitmapSource source)
    {
        throw new NotImplementedException();
    }
}
