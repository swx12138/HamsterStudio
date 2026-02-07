using System.Windows.Media.Imaging;

namespace HamsterStudio.ImageTool.Exposure;

public interface IImageProcessor
{
    BitmapSource ApplyAdjustments(BitmapSource source, ImageAdjustments adjustments);
    HistogramData CalculateHistogram(BitmapSource source);
}
