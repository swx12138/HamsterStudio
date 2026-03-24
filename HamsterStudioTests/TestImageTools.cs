using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;


namespace HamsterStudioTests;

public enum MergeDirection
{
    Horizontal, // 横向拼接
    Vertical    // 纵向拼接
}

public static class ImageMerger
{
    /// <summary>
    /// 拼接一组图片
    /// </summary>
    /// <param name="images">要拼接的图片集合</param>
    /// <param name="direction">拼接方向</param>
    /// <returns>拼接后的图片</returns>
    public static Image MergeImages(IEnumerable<Image> images, MergeDirection direction)
    {
        if (images == null || !images.Any())
            throw new ArgumentException("图片集合不能为空");

        var imageList = images.ToList();

        if (direction == MergeDirection.Horizontal)
        {
            return MergeHorizontally(imageList);
        }
        else
        {
            return MergeVertically(imageList);
        }
    }

    /// <summary>
    /// 横向拼接图片
    /// </summary>
    private static Image MergeHorizontally(List<Image> images)
    {
        // 找到最小高度
        int minHeight = images.Min(img => img.Height);

        // 计算总宽度
        int totalWidth = images.Sum(img =>
            (int)Math.Round(img.Width * (minHeight / (double)img.Height)));

        // 创建新画布
        var result = new Image<Rgba32>(totalWidth, minHeight);

        int currentX = 0;

        foreach (var img in images)
        {
            // 计算缩放后的宽度
            int newWidth = (int)Math.Round(img.Width * (minHeight / (double)img.Height));

            // 创建临时图像并调整大小
            var resizedImage = img.Clone(x => x.Resize(newWidth, minHeight));

            // 绘制到结果图像上
            result.Mutate(x => x.DrawImage(resizedImage, new Point(currentX, 0), 1f));

            currentX += newWidth;
        }

        return result;
    }

    /// <summary>
    /// 纵向拼接图片
    /// </summary>
    private static Image MergeVertically(List<Image> images)
    {
        // 找到最小宽度
        int minWidth = images.Min(img => img.Width);

        // 计算总高度
        int totalHeight = images.Sum(img =>
            (int)Math.Round(img.Height * (minWidth / (double)img.Width)));

        // 创建新画布
        var result = new Image<Rgba32>(minWidth, totalHeight);

        int currentY = 0;

        foreach (var img in images)
        {
            // 计算缩放后的高度
            int newHeight = (int)Math.Round(img.Height * (minWidth / (double)img.Width));

            // 创建临时图像并调整大小
            var resizedImage = img.Clone(x => x.Resize(minWidth, newHeight));

            // 绘制到结果图像上
            result.Mutate(x => x.DrawImage(resizedImage, new Point(0, currentY), 1f));

            currentY += newHeight;
        }

        return result;
    }
}


[TestClass]
public class TestImageTools
{
    [TestMethod]
    public void TestArwDecode()
    {
        //var srv = new DecodeImage();
        //var metas =  srv.LoadSonyRaw(@"C:\Users\collei\Documents\DSC03848\DSC03848.ARW");
        //Console.WriteLine(metas);
    }

    [TestMethod]
    public void TestConcateImages()
    {
        string[] files = new string[]
        {
            @"C:\Users\nv\Documents\MuMu共享文件夹\Screenshots\MuMu-20260206-231331-248.png",
            @"C:\Users\nv\Documents\MuMu共享文件夹\Screenshots\MuMu-20260206-231340-164.png",
            @"C:\Users\nv\Documents\MuMu共享文件夹\Screenshots\MuMu-20260206-231337-888.png",
            @"C:\Users\nv\Documents\MuMu共享文件夹\Screenshots\MuMu-20260206-231336-666.png",
        };

        var img = ImageMerger.MergeImages(files.Select(Image.Load), MergeDirection.Horizontal);
        img.Save(@"C:\Users\nv\Documents\MuMu共享文件夹\Screenshots\MuMu-20260206-231331-248__.png");

    }

}
