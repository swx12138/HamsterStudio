using HamsterStudio.Barefeet.Models;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HamsterStudio.Toolkits;

public static class ImageUtils
{
    // 大端字节序转uint（4字节）
    public static uint FromBigEndian(byte[] bytes, int offset)
    {
        return (uint)(bytes[offset] << 24 | bytes[offset + 1] << 16 | bytes[offset + 2] << 8 | bytes[offset + 3]);
    }

    public static UInt64 FromBigEndian64(byte[] bytes, int offest)
    {
        throw new NotImplementedException();
    }

    public static bool ScaleImage(string inputPath, double scale, string outputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath) || scale <= 0)
            return false;

        if (!IsImageFile(inputPath))
        {
            return false;
        }

        using var src = new Mat(inputPath, ImreadModes.Unchanged);
        using var dst = src.Resize(new OpenCvSharp.Size(src.Width * scale, src.Height * scale));
        dst.SaveImage(inputPath);
        return true;
    }

    public delegate double ScaleFunc(Mat mat);

    public static bool ScaleImage(string inputPath, ScaleFunc scaleFunc)
    {
        if (string.IsNullOrWhiteSpace(inputPath) || scaleFunc == null)
            return false;

        if (!IsImageFile(inputPath))
        {
            return false;
        }

        using var src = new Mat(inputPath, ImreadModes.Unchanged);
        double scale = scaleFunc(src);
        if (scale <= 0)
            return false;

        using var dst = src.Resize(new OpenCvSharp.Size(src.Width * scale, src.Height * scale));
        dst.SaveImage(inputPath);
        return true;
    }

    private static readonly string[] KnownImageExts = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff" };
    //private static readonly ReaderWriterLockSlim readerWriterLock = new();

    public static readonly Uri PlaceholderUri = new("https://i0.hdslb.com/bfs/live/new_room_cover/f4ebd63d8388bf3b1377756289e47d1e0f206ba6.jpg");
    public static readonly Lazy<ImageSource> Placeholder = new(() => new BitmapImage(PlaceholderUri));

    public static ImageSource? LoadImageSource(string filePath, uint decodePixWidth = 0)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Placeholder.Value;
        }

        if (!IsImageFile(filePath))
        {
            return Placeholder.Value;
        }

        //readerWriterLock.EnterWriteLock();
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // ✅ 加载到内存
            bitmap.DecodePixelWidth = (int)decodePixWidth;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze(); // ✅ 冻结
            return bitmap;
        }
        catch (Exception ex)
        {
            Trace.TraceError($"load file @ {filePath} failed，decode width is {decodePixWidth}。");
            Trace.TraceError(ex.Message);
            Trace.TraceError(ex.StackTrace);
        }
        finally
        {
            //readerWriterLock.ExitWriteLock();
        }

        return null;
    }

    public static bool IsImageFile(string path)
    {
        if (!File.Exists(path)) return false;
        return KnownImageExts.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    public static ImageSource CreateImageSource(this IReadOnlyCollection<ImageInfo> imageInfos, int columns, bool uniform = true)
    {
        ArgumentNullException.ThrowIfNull(imageInfos);

        columns = Math.Min(columns, imageInfos.Count);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);

        var validImages = imageInfos.Where(i => i.IsSelected).ToList();
        if (validImages.Count == 0)
            return new BitmapImage();

        // 步骤1：预处理所有图像（旋转/翻转/重复）
        List<Mat> resizedMats = [.. OpenCVHelper.ProcessAndResizeMats(validImages, uniform)];
        try
        {
            // 步骤3：计算画布尺寸
            int totalCount = resizedMats.Count;
            int rows = (totalCount + columns - 1) / columns;
            int cellWidth = resizedMats.Max(x => x.Width);
            int cellHeight = resizedMats.Max(x => x.Height);

            // 步骤4：绘制组合图像
            DrawingVisual drawingVisual = drawing(columns, resizedMats, cellHeight, out List<int> widths);

            // 步骤5：渲染
            return render(rows * cellHeight, widths, drawingVisual);
        }
        finally
        {
            foreach (var mat in resizedMats)
            {
                mat.Dispose();
            }
        }

        static ImageSource render(int totalHeight, List<int> widths, DrawingVisual drawingVisual)
        {
            double totalWidth = widths.Max();
            var renderBitmap = new RenderTargetBitmap(
                (int)totalWidth, (int)totalHeight, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(drawingVisual);
            return renderBitmap;
        }

        static DrawingVisual drawing(int columns, List<Mat> resizedMats, int cellHeight, out List<int> widths)
        {
            widths = [];
            var drawingVisual = new DrawingVisual();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                int left = 0;
                for (int i = 0; i < resizedMats.Count; i++)
                {
                    int row = i / columns;
                    int col = i % columns;

                    // 转换为 WPF 图像源
                    var bitmapSource = BitmapSourceConverter.ToBitmapSource(resizedMats[i]);

                    // 绘制逻辑（保持宽高比）
                    var imageSize = new OpenCvSharp.Size(bitmapSource.Width, bitmapSource.Height);
                    var renderRect = new System.Windows.Rect(left, row * cellHeight, imageSize.Width, imageSize.Height);
                    dc.DrawImage(bitmapSource, renderRect);

                    if (col == columns - 1)
                    {
                        widths.Add(left + imageSize.Width);
                        left = 0;
                    }
                    else
                    {
                        left += imageSize.Width;
                    }

                }
            }
            return drawingVisual;
        }

    }

    public static void SaveImageSource(this ImageSource source, string filePath, ImageFormat format = ImageFormat.Png)
    {
        if (source == null || string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("参数错误");

        // 创建目录（如果不存在）
        var directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 转换为 BitmapFrame
        BitmapFrame frame = source as BitmapFrame ?? BitmapFrame.Create((BitmapSource)source);

        // 选择编码器
        BitmapEncoder encoder = format switch
        {
            ImageFormat.Jpeg => new JpegBitmapEncoder { QualityLevel = 90 },
            ImageFormat.Bmp => new BmpBitmapEncoder(),
            ImageFormat.Gif => new GifBitmapEncoder(),
            _ => new PngBitmapEncoder() // 默认PNG
        };

        // 编码并保存
        encoder.Frames.Add(frame);

        using FileStream fs = new(filePath, FileMode.Create);
        encoder.Save(fs);
    }

    public enum ImageFormat
    {
        Png,
        Jpeg,
        Bmp,
        Gif
    }

}

public static class OpenCVHelper
{
    public static IEnumerable<Mat> ProcessAndResizeMats(List<ImageInfo> validImages, bool uniform)
    {
        // 如果不需要统一缩放，则直接处理图像
        if (!uniform)
        {
            return ProcessMats(validImages);
        }

        // 如果需要统一缩放，则先处理图像，然后并行调整大小
        var processedMats = ProcessMats(validImages).ToList();

        double avgHeight = processedMats.Average(m => m.Height);
        var result = new Mat[processedMats.Count];

        Parallel.For(0, processedMats.Count, i =>
        {
            using (var mat = processedMats[i])
            {
                double scale = avgHeight / mat.Height;
                var resizedMat = mat.Resize(new OpenCvSharp.Size(mat.Width * scale, avgHeight));
                result[i] = resizedMat.Clone(); // Clone to ensure the image is not disposed
            }
        });

        return result;
    }

    private static IEnumerable<Mat> ProcessMats(List<ImageInfo> validImages)
    {
        foreach (var imgInfo in validImages)
        {
            using var srcMat = new Mat(imgInfo.Filename, ImreadModes.Unchanged);

            // 应用旋转
            var rotatedMat = ApplyRotation(srcMat, imgInfo.RotateType);

            // 应用翻转
            ApplyFlip(rotatedMat, imgInfo.UpDownFlip, imgInfo.LeftRightFlip);

            // 添加重复次数
            for (int i = 0; i < imgInfo.RepeatCount; i++)
            {
                yield return rotatedMat.Clone();
            }
        }
    }

    // 旋转处理辅助方法
    private static Mat ApplyRotation(Mat src, ImageRotateType rotateType)
    {
        if (rotateType == ImageRotateType.R0)
            return src.Clone();

        RotateFlags flag = rotateType switch
        {
            ImageRotateType.R90 => RotateFlags.Rotate90Clockwise,
            ImageRotateType.R180 => RotateFlags.Rotate180,
            ImageRotateType.R270 => RotateFlags.Rotate90Counterclockwise,
            _ => RotateFlags.Rotate90Clockwise
        };

        Mat dst = new Mat();
        Cv2.Rotate(src, dst, flag);
        return dst;
    }

    // 翻转处理辅助方法
    private static void ApplyFlip(Mat mat, bool upDown, bool leftRight)
    {
        if (upDown && leftRight)
        {
            Cv2.Flip(mat, mat, FlipMode.XY);
        }
        else if (upDown)
        {
            Cv2.Flip(mat, mat, FlipMode.X);
        }
        else if (leftRight)
        {
            Cv2.Flip(mat, mat, FlipMode.Y);
        }
    }

}
