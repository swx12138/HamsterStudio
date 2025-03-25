using Android.Media;
using Java.IO;
using Application = Android.App.Application;
using Environment = Android.OS.Environment;
using File = Java.IO.File;

namespace HamsterStudioMaui.Platforms.Android.Utils;

public static class FileUtils
{
    public static string WriteFileToDCIM(string fileName, System.IO.Stream stream)
    {
        var hamsterDir = new File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDcim), "HamsterStudio");
        hamsterDir.Mkdirs();

        // 创建文件
        var file = new File(hamsterDir, fileName);

        using var fos = new FileOutputStream(file);
        using var bis = new BufferedInputStream(stream);
        fos.Write(bis.ReadAllBytes());

        return file.CanonicalPath;
    }

    public static void NotifyGalleryOfNewImage(string[] imagePaths)
    {
        MediaScannerConnection.ScanFile(
            Application.Context,
            imagePaths,
            null, null);
    }

}