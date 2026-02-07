using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace HamsterStudio.ImageTool.Exposure.ImageProcessors;

[StructLayout(LayoutKind.Sequential)]
public struct NativeImageAdjustments
{
    public double Exposure;
    public double Temperature;
    public double Tint;
    public double Highlights;
    public double Shadows;
    public double Whites;
    public double Blacks;
    public double Saturation;
    public double Vibrance;
}

// C++实现
public class CppImageProcessor(ILogger? logger = null) : IImageProcessor, IDisposable
{
    private nint _processorHandle = image_processor_create();

    const string DllPath = @"HamsterStudioNative64.dll";

    #region DLL Imports

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    private static extern nint image_processor_create();

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    private static extern void image_processor_destroy(nint processor);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool image_processor_process(
        nint processor,
        nint pixels,
        int width,
        int height,
        int stride,
        ref NativeImageAdjustments adjustments);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool image_processor_get_histogram(
        nint processor,
        nint pixels,
        int width,
        int height,
        int stride,
        [Out] int[] redHist,
        [Out] int[] greenHist,
        [Out] int[] blueHist,
        [Out] int[] luminanceHist);

    #endregion

    public bool ProcessImage(WriteableBitmap bitmap, ImageAdjustments adjustments)
    {
        if (_processorHandle == nint.Zero || bitmap == null)
            return false;

        bitmap.Lock();
        try
        {
            nint backBuffer = bitmap.BackBuffer;
            int stride = bitmap.BackBufferStride;

            var nativeAdj = new NativeImageAdjustments
            {
                Exposure = adjustments.Exposure,
                Temperature = adjustments.Temperature,
                Tint = adjustments.Tint,
                Highlights = adjustments.Highlights,
                Shadows = adjustments.Shadows,
                Whites = adjustments.Whites,
                Blacks = adjustments.Blacks,
                Saturation = adjustments.Saturation,
                Vibrance = adjustments.Vibrance
            };

            return image_processor_process(
                _processorHandle,
                backBuffer,
                bitmap.PixelWidth,
                bitmap.PixelHeight,
                stride,
                ref nativeAdj);
        }
        finally
        {
            bitmap.Unlock();
        }
    }

    public (int[] red, int[] green, int[] blue, int[] luminance) GetHistogram(WriteableBitmap bitmap)
    {
        int[] red = new int[256];
        int[] green = new int[256];
        int[] blue = new int[256];
        int[] luminance = new int[256];

        if (_processorHandle == nint.Zero || bitmap == null)
            return (red, green, blue, luminance);

        bitmap.Lock();
        try
        {
            nint backBuffer = bitmap.BackBuffer;
            int stride = bitmap.BackBufferStride;

            //image_processor_get_histogram(
            //    _processorHandle,
            //    backBuffer,
            //    bitmap.PixelWidth,
            //    bitmap.PixelHeight,
            //    stride,
            //    red,
            //    green,
            //    blue,
            //    luminance);
        }
        finally
        {
            bitmap.Unlock();
        }

        return (red, green, blue, luminance);
    }

    public void Dispose()
    {
        if (_processorHandle != nint.Zero)
        {
            image_processor_destroy(_processorHandle);
            _processorHandle = nint.Zero;
        }
        GC.SuppressFinalize(this);
    }

    ~CppImageProcessor()
    {
        Dispose();
    }

    public BitmapSource ApplyAdjustments(BitmapSource source, ImageAdjustments adjustments)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(adjustments);

        var tempBitmap = new WriteableBitmap(source);
        if (ProcessImage(tempBitmap, adjustments))
        {
            return tempBitmap;
        }

        // 使用Native版本计算直方图
        //var histogram = _nativeProcessor.GetHistogram(tempBitmap);
        //UpdateHistogramWithData(histogram);

        throw new InvalidOperationException("Failed to process image with native processor.");
    }

    public HistogramData CalculateHistogram(BitmapSource source)
    {
        throw new NotImplementedException();
    }
}
