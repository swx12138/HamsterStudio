using HamsterStudio.Barefeet.Models;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HamsterStudio.Toolkits
{
    public struct ImageMetaInfo
    {
        [JsonPropertyName("width")]
        public long Width { get; init; }

        [JsonPropertyName("height")]
        public long Height { get; init; }

        [JsonPropertyName("type")]
        public string Type { get; init; }
    }

    public interface IImageMetaInfoReader
    {
        bool Accept(in byte[] header);
        ImageMetaInfo Read(in FileStream ifs);
    }

    public class ImageMetaInfoReadService
    {
        public List<IImageMetaInfoReader> ImageMetaInfoReaders = [];

        public ImageMetaInfo Read(in string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(Path.GetFullPath(path));
            }

            using FileStream ifs = File.OpenRead(path);
            var headerRaw = new byte[12];
            ifs.Read(headerRaw, 0, headerRaw.Length);
            foreach (var reader in ImageMetaInfoReaders)
            {
                if (reader.Accept(headerRaw))
                {
                    return reader.Read(ifs);
                }
            }

            throw new NotSupportedException("Not support file format");
        }

    }

    public class JpegImageMetaInfoReader : IImageMetaInfoReader
    {
        public bool Accept(in byte[] header)
        {
            ushort magic = BitConverter.ToUInt16(header.Take(2).Reverse().ToArray());
            return magic == 0xffd8;
        }

        public ImageMetaInfo Read(in FileStream ifs)
        {
            ifs.Seek(2, SeekOrigin.Begin);
            while (true)
            {
                // 读取两个字节
                var rawTag = new byte[2];
                if (0 == ifs.Read(rawTag, 0, rawTag.Length))
                {
                    break;
                }
                //rawTag = rawTag.Reverse().ToArray();

                if (rawTag[0] != 0xff)
                {
                    Console.WriteLine("Not a JPEG format.");
                    break;
                }
                var rawLen = new byte[2];
                ifs.Read(rawLen, 0, rawLen.Length);
                int length = BitConverter.ToUInt16(rawLen.Reverse().ToArray()) - 2; // 减2是因为表示长度的两个字节也在内
                if (rawTag[1] != 0xc0)
                {
                    //Console.WriteLine($"Unkown tag {rawTag[0]:x} {rawTag[1]:x},Skipped {length} bytes.");
                    ifs.Seek(length, SeekOrigin.Current);
                    continue;
                }

                var sofRaw = new byte[length];
                ifs.Read(sofRaw, 0, sofRaw.Length);

                byte sample = sofRaw[0];
                ushort height = BitConverter.ToUInt16(sofRaw.Skip(1).Take(2).Reverse().ToArray());
                ushort width = BitConverter.ToUInt16(sofRaw.Skip(3).Take(2).Reverse().ToArray());
                byte channel = sofRaw[5];

                //Console.WriteLine($"It's a jpeg image, {width}*{height} in size, with {channel} channels.");
                //break;
                return new() { Height = height, Width = width, Type = "Jpeg" };
            }
            throw new KeyNotFoundException("没有找到文件头信息，可能不是一个正确的JPEG文件。");
        }
    }

    public class PngImageMetaInfoReader : IImageMetaInfoReader
    {
        public bool Accept(in byte[] header)
        {
            var fullMagic = BitConverter.ToUInt64(header.Take(8).Reverse().ToArray());
            return fullMagic == 0x89504e470d0a1a0a;
        }

        public ImageMetaInfo Read(in FileStream ifs)
        {
            ifs.Seek(8, SeekOrigin.Begin);

            var blkSizeRaw = new byte[4];
            ifs.Read(blkSizeRaw, 0, blkSizeRaw.Length);


            var blkSize = ImageUtils.FromBigEndian(blkSizeRaw, 0);
            var blkData = new byte[blkSize];
            ifs.Read(blkData, 0, blkData.Length);

            var blkTypeCode = ImageUtils.FromBigEndian(blkData, 0);
            if (blkTypeCode == 0x49484452) // IHDR
            {
                var width = ImageUtils.FromBigEndian(blkData, 4);
                var height = ImageUtils.FromBigEndian(blkData, 8);
                var depth = blkData[12];
                return new() { Height = height, Width = width, Type = "Png" };
            }
            else
            {
                // 第一个Block一定是IHDR
                throw new FormatException("First block of png is not IHDR!!");
            }
        }
    }

    public class WebpImageMetaInfoReader : IImageMetaInfoReader
    {
        public bool Accept(in byte[] header)
        {
            UInt32 riffMagic = BitConverter.ToUInt32(header.Take(4).Reverse().ToArray());
            if (riffMagic != 0x52494646)
            {
                return false;
            }

            UInt32 webpMagic = BitConverter.ToUInt32(header.Skip(8).Take(4).Reverse().ToArray());
            if (webpMagic != 0x57454250)
            {
                return false;
            }

            return true;
        }

        public static int LiittleEndian12bit(in byte[] data)
        {
            return (data[2] << 16 | data[1] << 8 | data[0]);
        }

        public ImageMetaInfo Read(in FileStream ifs)
        {
            while (true)
            {
                var tagRaw = new byte[8];
                if (0 == ifs.Read(tagRaw, 0, tagRaw.Length))
                {
                    break;
                }

                var tag = BitConverter.ToUInt32(tagRaw.Take(4).Reverse().ToArray());
                var dataSize = BitConverter.ToUInt32(tagRaw.Skip(4).Take(4).ToArray());
                if (tag != 0x56503858) // VB8X
                {
                    ifs.Seek(dataSize, SeekOrigin.Current);
                    continue;
                }

                var data = new byte[dataSize];
                ifs.Read(data, 0, data.Length);

                var flags = data[0];
                var width = LiittleEndian12bit(data.Skip(4).Take(3).ToArray()) + 1;
                var height = LiittleEndian12bit(data.Skip(7).Take(3).ToArray()) + 1;
                return new() { Width = width, Height = height, Type = "Webp" };
            }
            throw new FormatException();
        }
    }

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

        public readonly static Uri Placeholder = new("https://i0.hdslb.com/bfs/live/new_room_cover/f4ebd63d8388bf3b1377756289e47d1e0f206ba6.jpg");

        public static BitmapImage LoadThumbnail(string? filePath, int maxWidth = 200)
        {
            if (filePath == null)
            {
                return new BitmapImage(Placeholder);
            }

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.DecodePixelWidth = maxWidth;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"加载缩略图失败: {ex.Message}\n{ex.StackTrace}");
                return new BitmapImage(Placeholder);
            }
        }

        public static ImageSource? LoadImageSource(string? path)
        {
            if (path == null) { return null; }
            if (string.IsNullOrWhiteSpace(path)) { return null; }

            if (!IsImageFile(path))
            {
                return new BitmapImage(Placeholder);
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
                return new BitmapImage(Placeholder);
            }
        }

        public static ImageSource? CreateImageSource(Stream stream)
        {
            if (stream == null) { return null; }
            var bitmap = new BitmapImage();
            try
            {
                bitmap.BeginInit();
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // 关键：强制在初始化时加载                
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze(); // 可选：冻结对象避免跨线程问题
                return bitmap;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                Trace.TraceError(ex.StackTrace);
                throw;
            }
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
            List<Mat> resizedMats = [.. ImageHelper.ProcessAndResizeMats(validImages, uniform)];
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

        public static class ImageHelper
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
    
    }
}