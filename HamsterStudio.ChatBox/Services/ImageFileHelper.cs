using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HamsterStudio.ChatBox.Services;

/// <summary>图片附件工具：从磁盘路径读取、压缩并转为 base64，以及按需加载缩略图/完整图。</summary>
public static class ImageFileHelper
{
    public const int MaxDimension = 2000;

    /// <summary>读取图片并压缩后转为纯 base64（Ollama 使用）。</summary>
    public static string ReadAsBase64(string path)
    {
        var bytes = File.ReadAllBytes(path);
        TryDownscale(ref bytes, MaxDimension);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>读取图片并压缩后转为 data URL（OpenAI / DeepSeek 使用）。</summary>
    public static string ReadAsDataUrl(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var mime = GetMimeType(path);
        if (TryDownscale(ref bytes, MaxDimension))
            mime = "image/jpeg"; // 压缩后统一为 JPEG

        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }

    /// <summary>从路径加载缩略图（缩小解码，节省内存）。</summary>
    public static BitmapImage? LoadThumbnail(string path, int maxDim = 400)
    {
        try
        {
            // 先读取尺寸，不加载完整像素
            var decoder = BitmapDecoder.Create(new Uri(path, UriKind.Absolute), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
            var frame = decoder.Frames[0];

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            if (frame.PixelWidth >= frame.PixelHeight)
                image.DecodePixelWidth = maxDim;
            else
                image.DecodePixelHeight = maxDim;
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>从路径加载完整图片（放大查看用）。</summary>
    public static BitmapImage? LoadFullImage(string path)
    {
        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            return null;
        }
    }

    private static bool TryDownscale(ref byte[] bytes, int maxDim)
    {
        try
        {
            using var stream = new MemoryStream(bytes);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();

            // 尺寸未超限则不处理
            if (bitmap.PixelWidth <= maxDim && bitmap.PixelHeight <= maxDim)
                return false;

            var scale = Math.Min((double)maxDim / bitmap.PixelWidth, (double)maxDim / bitmap.PixelHeight);
            var resized = new TransformedBitmap(bitmap, new ScaleTransform(scale, scale));

            var encoder = new JpegBitmapEncoder { QualityLevel = 90 };
            encoder.Frames.Add(BitmapFrame.Create(resized));

            using var ms = new MemoryStream();
            encoder.Save(ms);
            bytes = ms.ToArray();
            return true;
        }
        catch
        {
            // 解码失败（如 WebP 等不支持的格式）时保持原样
            return false;
        }
    }

    private static string GetMimeType(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".bmp" => "image/bmp",
        _ => "image/jpeg",
    };
}
