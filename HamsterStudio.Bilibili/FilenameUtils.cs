using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web;
using HamsterStudio.Web.Utilities;

namespace HamsterStudio.Bilibili;

public static class FilenameUtils
{
    public static string StorageHome { get; set; } = Environment.CurrentDirectory;

    public static string GetFilenameFromUrl(string url) => url.Split("?")[0].Split("@")[0].Split('/').Where(x => !x.IsNullOrEmpty()).Last();

    public static string FormatImageFilename(string url, string bvid)
    {
        string filename = GetFilenameFromUrl(url);
        return $"{bvid}_bili_{filename}";
    }

    public static string FormatImageFilename(string url, string dynamicId, int idx)
    {
        string filename = GetFilenameFromUrl(url);
        return $"{dynamicId}_{idx}_bili_{filename}";
    }

    public static string GetImageFullPath(string filename)
    {
        filename = FileNameUtil.SanitizeFileName(filename);
        return Path.Combine(StorageHome, "Covers", filename);
    }

    public static string GetVideoFullPath(string filename)
    {
        filename = FileNameUtil.SanitizeFileName(filename);
        return Path.Combine(StorageHome, "Dash", filename);
    }

    public static async Task Downlaod(string url, string title)
    {
        string filename = FormatImageFilename(url, title);
        string fullPath = GetImageFullPath(filename);

        var stream = await FakeBrowser.CommonClient.GetStreamAsync(url);
        var result = await FileSaver.SaveFileToDisk(stream, fullPath);

        if (result == FileSaver.SaveFileResult.Existed)
        {
            Logger.Shared.Information(filename + " already exists, skip saving.");
        }
        else if (result == FileSaver.SaveFileResult.Succeed)
        {
            Logger.Shared.Information($"Saved cover to {fullPath} successfully.");
        }
        else
        {
            Logger.Shared.Error($"Saving {fullPath} failed.");
        }
    }

}
