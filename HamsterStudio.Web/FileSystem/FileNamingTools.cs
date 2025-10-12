using HamsterStudio.Barefeet.Extensions;

namespace HamsterStudio.Web.FileSystem;

public static class FileNamingTools
{
    public static string GetFilenameFromUrl(string url) => url.Split("?")[0].Split("@")[0].Split('/').Where(x => !x.IsNullOrEmpty()).Last();

    public static string FormatFilename(string name, string siteName, string title, string auther, int index = -1)
    {
        string filename = index >= 0 ?
            $"{title}_{index}_{siteName}_{auther}_{name}" :
            $"{title}_{siteName}_{auther}_{name}";
        return FileNameUtil.SanitizeFileName(filename);
    }

}
