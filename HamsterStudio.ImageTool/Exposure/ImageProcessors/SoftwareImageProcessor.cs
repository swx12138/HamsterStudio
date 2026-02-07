using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HamsterStudio.ImageTool.Exposure.ImageProcessors;

// 软件实现
public class SoftwareImageProcessor : IImageProcessor
{
    public BitmapSource ApplyAdjustments(BitmapSource source, ImageAdjustments adjustments)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        // 创建临时位图进行处理
        var tempBitmap = new WriteableBitmap(source);
        int width = tempBitmap.PixelWidth;
        int height = tempBitmap.PixelHeight;
        int stride = width * 4;
        byte[] pixels = new byte[height * stride];

        // 复制原始像素数据
        tempBitmap.CopyPixels(pixels, stride, 0);

        // 应用调整
        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte b = pixels[i];
            byte g = pixels[i + 1];
            byte r = pixels[i + 2];
            byte a = pixels[i + 3];

            // 转换为浮点数以便处理
            float red = r / 255.0f;
            float green = g / 255.0f;
            float blue = b / 255.0f;

            // 应用曝光度
            if (adjustments.Exposure != 0)
            {
                float exposure = (float)Math.Pow(2, adjustments.Exposure);
                red *= exposure;
                green *= exposure;
                blue *= exposure;
            }

            // 转换为HSV进行饱和度调整
            float[] hsv = RGBtoHSV(red, green, blue); // 16.55% CPU laod

            // 应用饱和度
            if (adjustments.Saturation != 0)
            {
                float saturationAdjust = (float)(adjustments.Saturation / 100.0);
                hsv[1] = Math.Clamp(hsv[1] + saturationAdjust, 0, 1);
            }

            // 应用鲜艳度（智能饱和度）
            if (adjustments.Vibrance != 0)
            {
                float vibrance = (float)(adjustments.Vibrance / 100.0);
                float avg = (red + green + blue) / 3.0f;
                float maxDiff = Math.Max(Math.Abs(red - avg),
                                        Math.Max(Math.Abs(green - avg),
                                                Math.Abs(blue - avg)));

                if (maxDiff < 0.1f) // 只调整接近中性的颜色
                {
                    float saturationBoost = vibrance * 0.5f;
                    hsv[1] = Math.Clamp(hsv[1] + saturationBoost, 0, 1);
                }
            }

            // 转换回RGB
            float[] rgb = HSVtoRGB(hsv[0], hsv[1], hsv[2]);// 12.18% CPU laod
            red = rgb[0];
            green = rgb[1];
            blue = rgb[2];

            // 应用色温和色调
            if (adjustments.Temperature != 0 || adjustments.Tint != 0)
            {
                float temp = (float)(adjustments.Temperature / 100.0);
                float tint = (float)(adjustments.Tint / 100.0);

                // 色温（蓝色-黄色轴）
                blue = Math.Clamp(blue - temp * 0.5f, 0, 1);
                red = Math.Clamp(red + temp * 0.25f, 0, 1);

                // 色调（绿色-洋红轴）
                green = Math.Clamp(green - tint * 0.5f, 0, 1);
                red = Math.Clamp(red + tint * 0.25f, 0, 1);
            }

            // 应用色调调整
            ApplyToneAdjustments(adjustments, ref red, ref green, ref blue);

            // 钳制到有效范围
            red = Math.Clamp(red, 0, 1);
            green = Math.Clamp(green, 0, 1);
            blue = Math.Clamp(blue, 0, 1);

            // 写回像素
            pixels[i] = (byte)(blue * 255);
            pixels[i + 1] = (byte)(green * 255);
            pixels[i + 2] = (byte)(red * 255);
            pixels[i + 3] = a;
        }

        // 更新WriteableBitmap
        tempBitmap.WritePixels(new Int32Rect(0, 0, width, height),
                              pixels, stride, 0);

        // 显示处理后的图像
        return tempBitmap;

        // 更新直方图
        // UpdateHistogram();

    }

    public HistogramData CalculateHistogram(BitmapSource source)
    {
        throw new NotImplementedException();
    }

    #region 图像处理

    private static void ApplyToneAdjustments(ImageAdjustments adjustments, ref float r, ref float g, ref float b)
    {
        // 高光调整
        if (adjustments.Highlights > 0)
        {
            float highlightFactor = (float)(adjustments.Highlights / 100.0);
            float luminance = GetLuminance(r, g, b);

            if (luminance > 0.7f)
            {
                float blend = (luminance - 0.7f) / 0.3f;
                r = Lerp(r, 1.0f, blend * highlightFactor);
                g = Lerp(g, 1.0f, blend * highlightFactor);
                b = Lerp(b, 1.0f, blend * highlightFactor);
            }
        }
        else if (adjustments.Highlights < 0)
        {
            float highlightFactor = (float)(adjustments.Highlights / 100.0);
            r = Lerp(r, GetMidValue(r), -highlightFactor);
            g = Lerp(g, GetMidValue(g), -highlightFactor);
            b = Lerp(b, GetMidValue(b), -highlightFactor);
        }

        // 阴影调整
        if (adjustments.Shadows > 0)
        {
            float shadowFactor = (float)(adjustments.Shadows / 100.0);
            float luminance = GetLuminance(r, g, b);

            if (luminance < 0.3f)
            {
                float blend = 1.0f - luminance / 0.3f;
                r = Lerp(r, 0.0f, blend * shadowFactor);
                g = Lerp(g, 0.0f, blend * shadowFactor);
                b = Lerp(b, 0.0f, blend * shadowFactor);
            }
        }
        else if (adjustments.Shadows < 0)
        {
            float shadowFactor = (float)(adjustments.Shadows / 100.0);
            r = Lerp(r, GetMidValue(r), shadowFactor);
            g = Lerp(g, GetMidValue(g), shadowFactor);
            b = Lerp(b, GetMidValue(b), shadowFactor);
        }

    }

    public static float GetLuminance(float r, float g, float b)
    {
        return 0.299f * r + 0.587f * g + 0.114f * b;
    }

    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    public static float GetMidValue(float value)
    {
        return 0.18f; // 中间灰度值
    }

    public static float[] RGBtoHSV(float r, float g, float b)
    {
        float min = Math.Min(Math.Min(r, g), b);
        float max = Math.Max(Math.Max(r, g), b);
        float delta = max - min;

        float h = 0;
        float s = max == 0 ? 0 : delta / max;
        float v = max;

        if (delta != 0)
        {
            if (max == r)
                h = (g - b) / delta + (g < b ? 6 : 0);
            else if (max == g)
                h = (b - r) / delta + 2;
            else
                h = (r - g) / delta + 4;

            h /= 6;
        }

        return new float[] { h, s, v };
    }

    public static float[] HSVtoRGB(float h, float s, float v)
    {
        if (s == 0)
            return new float[] { v, v, v };

        int i = (int)(h * 6);
        float f = h * 6 - i;
        float p = v * (1 - s);
        float q = v * (1 - f * s);
        float t = v * (1 - (1 - f) * s);

        return i switch
        {
            0 => new float[] { v, t, p },
            1 => new float[] { q, v, p },
            2 => new float[] { p, v, t },
            3 => new float[] { p, q, v },
            4 => new float[] { t, p, v },
            _ => new float[] { v, p, q },
        };
    }

    #endregion

    #region 直方图

    private void UpdateHistogram(in Canvas HistogramCanvas, in BitmapSource AdjustedImage)
    {
        if (AdjustedImage == null) return;

        HistogramCanvas.Children.Clear();

        int width = (int)HistogramCanvas.ActualWidth;
        int height = (int)HistogramCanvas.ActualHeight;

        if (width <= 0 || height <= 0) return;

        int[] redHistogram = new int[256];
        int[] greenHistogram = new int[256];
        int[] blueHistogram = new int[256];
        int[] luminanceHistogram = new int[256];

        int pixelCount = AdjustedImage.PixelWidth * AdjustedImage.PixelHeight;
        int stride = AdjustedImage.PixelWidth * 4;
        byte[] pixels = new byte[pixelCount * 4];

        AdjustedImage.CopyPixels(pixels, stride, 0);

        // 计算直方图
        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte b = pixels[i];
            byte g = pixels[i + 1];
            byte r = pixels[i + 2];

            redHistogram[r]++;
            greenHistogram[g]++;
            blueHistogram[b]++;

            // 计算亮度
            int luminance = (int)(0.299 * r + 0.587 * g + 0.114 * b);
            luminanceHistogram[luminance]++;
        }

        // 归一化
        int maxCount = Math.Max(Math.Max(redHistogram.Max(), greenHistogram.Max()),
                               Math.Max(blueHistogram.Max(), luminanceHistogram.Max()));

        // 绘制直方图
        DrawHistogramChannel(HistogramCanvas, redHistogram, maxCount, width, height, Colors.Red, 0.5);
        DrawHistogramChannel(HistogramCanvas, greenHistogram, maxCount, width, height, Colors.Green, 0.5);
        DrawHistogramChannel(HistogramCanvas, blueHistogram, maxCount, width, height, Colors.Blue, 0.5);
        DrawHistogramChannel(HistogramCanvas, luminanceHistogram, maxCount, width, height, Colors.White, 0.3);
    }

    private void DrawHistogramChannel(Canvas HistogramCanvas, int[] histogram, int maxCount, int width, int height, Color color, double opacity)
    {
        double binWidth = width / 256.0;

        for (int i = 0; i < 256; i++)
        {
            double normalized = (double)histogram[i] / maxCount;
            double barHeight = normalized * (height - 20);

            if (barHeight < 1) barHeight = 1;

            System.Windows.Shapes.Rectangle bar = new System.Windows.Shapes.Rectangle
            {
                Width = binWidth,
                Height = barHeight,
                Fill = new SolidColorBrush(color) { Opacity = opacity },
                StrokeThickness = 0
            };

            Canvas.SetLeft(bar, i * binWidth);
            Canvas.SetBottom(bar, 0);

            HistogramCanvas.Children.Add(bar);
        }
    }
    #endregion
}