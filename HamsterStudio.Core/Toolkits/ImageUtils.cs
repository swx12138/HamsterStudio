using HamsterStudio.Models;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HamsterStudio.Toolkits
{
    public static class ImageUtils
    {
        public static bool ScaleImage(string inputPath, double scale)
        {
            if (string.IsNullOrWhiteSpace(inputPath) || scale <= 0)
                return false;

            if (!IsImageFile(inputPath))
            {
                return false;
            }

            using var src = new Mat(inputPath, ImreadModes.Unchanged);
            using var dst = src.Resize(new Size(src.Width * scale, src.Height * scale));
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

            using var dst = src.Resize(new Size(src.Width * scale, src.Height * scale));
            dst.SaveImage(inputPath);
            return true;
        }

        private static readonly string[] KnownImageExts = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff" };

        public static ImageSource? LoadImageSource(string? path)
        {
            if (path == null) { return null; }
            if (string.IsNullOrWhiteSpace(path)) { return null; }

            if (!IsImageFile(path))
            {
                return null;
            }

            var bitmap = new BitmapImage();
            try
            {
                bitmap.BeginInit();
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // 关键：强制在初始化时加载                
                bitmap.UriSource = new Uri(path);
                bitmap.EndInit();
                bitmap.Freeze(); // 可选：冻结对象避免跨线程问题
                return bitmap;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"file @ {path}，。");
                Trace.TraceError(ex.Message);
                Trace.TraceError(ex.StackTrace);
                throw;
            }
        }

        public static bool IsImageFile(string path)
        {
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
            var processedMats = new List<Mat>();
            try
            {
                foreach (var imgInfo in validImages)
                {
                    using var srcMat = new Mat(imgInfo.Filename, ImreadModes.Unchanged);

                    // 应用旋转
                    using var rotatedMat = ApplyRotation(srcMat, imgInfo.RotateType);

                    // 应用翻转
                    ApplyFlip(rotatedMat, imgInfo.UpDownFlip, imgInfo.LeftRightFlip);

                    // 添加重复次数
                    for (int i = 0; i < imgInfo.RepeatCount; i++)
                    {
                        processedMats.Add(rotatedMat.Clone());
                    }
                }

                if (processedMats.Count == 0)
                    return new BitmapImage();


                // 步骤2：计算目标尺寸和缩放图片
                List<Mat> resizedMats = [];
                try
                {
                    if (uniform)
                    {
                        // 计算所有图片的平均高度
                        double avgHeight = processedMats.Average(m => m.Height);

                        // 统一缩放到平均高度
                        foreach (var mat in processedMats)
                        {
                            using (mat)
                            {
                                double scale = avgHeight / mat.Height;
                                var resizedMat = mat.Resize(new Size(mat.Width * scale, avgHeight));
                                resizedMats.Add(resizedMat);
                            }
                        }
                    }
                    else
                    {
                        resizedMats = processedMats;
                    }

                    // 步骤3：计算画布尺寸
                    int totalCount = resizedMats.Count;
                    int rows = (totalCount + columns - 1) / columns;
                    int cellWidth = resizedMats.Max(x => x.Width);
                    int cellHeight = resizedMats.Max(x => x.Height);
                    List<int> widths = [];

                    // 步骤4：绘制组合图像
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
                            var imageSize = new Size(bitmapSource.Width, bitmapSource.Height);
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

                    // 步骤5：渲染并清理资源
                    double totalWidth = widths.Max();
                    double totalHeight = rows * cellHeight;
                    var renderBitmap = new RenderTargetBitmap(
                        (int)totalWidth, (int)totalHeight, 96, 96, PixelFormats.Pbgra32);
                    renderBitmap.Render(drawingVisual);
                    return renderBitmap;
                }
                finally
                {
                    foreach (var mat in resizedMats)
                    {
                        mat.Dispose();
                    }
                }
            }
            finally
            {
                foreach (var mat in processedMats)
                {
                    mat.Dispose();
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
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fs);
            }
        }

        public enum ImageFormat
        {
            Png,
            Jpeg,
            Bmp,
            Gif
        }

    }

}