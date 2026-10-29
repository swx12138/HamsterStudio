using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Toolkits;
using HamsterStudioBridge.ImageModuleWrapper;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace HamsterStudio.ImageTool.ViewModels;

public partial class ImageViewerViewModel(ILogger<ImageViewerViewModel> logger) : KnownViewModel("ImageViewerViewModel", logger)
{
    [ObservableProperty]
    private ImageSource? hsvHistogramImage;

    [ObservableProperty]
    private ImageSource? rgbHistogramImage;

    /// <summary>
    /// 图片选择后异步计算 HSV（H-S 二维直方图色轮）并显示到 imageHistPreview
    /// </summary>
    public void UpdateHistogram(Uri? uri)
    {
        if (uri == null)
        {
            ClearHistogram();
            return;
        }

        var path = uri.IsFile ? uri.LocalPath : uri.OriginalString;
        _ = Task.Run(() => ComputeHistogram(path));
    }

    public void ClearHistogram()
    {
        HsvHistogramImage = null;
        RgbHistogramImage = null;
    }

    private void ComputeHistogram(string path)
    {
        try
        {
            using var src = ImageUtils.SafeLoadImage(path);

            using var hsv = new Mat();
            NativeImageUtils.CreateHSVHistogram(src.CvPtr, hsv.CvPtr, true, 500, 12);
            var hsvBitmap = BitmapSourceConverter.ToBitmapSource(hsv);
            hsvBitmap.Freeze();

            using var rgb = CreateRgbHistogram(src);
            var rgbBitmap = BitmapSourceConverter.ToBitmapSource(rgb);
            rgbBitmap.Freeze();

            Application.Current.Dispatcher.Invoke(() =>
            {
                HsvHistogramImage = hsvBitmap;
                RgbHistogramImage = rgbBitmap;
            });
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Compute histogram failed: {path}");
            Trace.TraceError(ex.Message);
            Trace.TraceError(ex.StackTrace);
            Application.Current.Dispatcher.Invoke(ClearHistogram);
        }
    }

    /// <summary>
    /// 常规 RGB + 亮度一维直方图（蓝/绿/红/灰四条曲线）
    /// </summary>
    private static Mat CreateRgbHistogram(Mat bgrSrc)
    {
        const int bins = 256;
        const int width = 512;
        const int height = 300;
        const int margin = 20;
        const int plotHeight = height - margin * 2;
        const float xScale = (float)(width - margin * 2) / bins;

        // 注意：不能加 using，返回值由调用方负责释放
        var plot = new Mat(height, width, MatType.CV_8UC3, Scalar.White);

        // 坐标轴
        Cv2.Line(plot, margin, margin, margin, height - margin, new Scalar(0, 0, 0), 1);
        Cv2.Line(plot, margin, height - margin, width - margin, height - margin, new Scalar(0, 0, 0), 1);

        Mat[] channels = bgrSrc.Split();
        try
        {
            using var gray = new Mat();
            Cv2.CvtColor(bgrSrc, gray, ColorConversionCodes.BGR2GRAY);

            (Mat Channel, Scalar Color)[] series =
            [
                (channels[0], new Scalar(255, 0, 0)),   // B
                (channels[1], new Scalar(0, 255, 0)),   // G
                (channels[2], new Scalar(0, 0, 255)),   // R
                (gray, new Scalar(128, 128, 128)),      // 亮度
            ];

            foreach (var (channel, color) in series)
            {
                using var hist = new Mat<float>();
                Cv2.CalcHist([channel], [0], null, hist, 1, new[] { bins }, new[] { new Rangef(0, bins) });
                Cv2.Normalize(hist, hist, 0, plotHeight, NormTypes.MinMax);

                for (int i = 1; i < bins; i++)
                {
                    int x0 = margin + (int)MathF.Round((i - 1) * xScale);
                    int y0 = height - margin - (int)MathF.Round(hist.Get<float>(i - 1));
                    int x1 = margin + (int)MathF.Round(i * xScale);
                    int y1 = height - margin - (int)MathF.Round(hist.Get<float>(i));
                    Cv2.Line(plot, x0, y0, x1, y1, color, 1, LineTypes.AntiAlias);
                }
            }
        }
        finally
        {
            foreach (var channel in channels)
            {
                channel.Dispose();
            }
        }

        return plot;
    }
}
