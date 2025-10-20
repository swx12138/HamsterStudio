namespace HamsterStudio.Barefeet.FileSystem;

public static class FileSizeDescriptor
{
    public const int FileSize_1M = 1024 * 1024;
    public const int FileSize_2M = 2 * FileSize_1M;
    public const int FileSize_4M = 2 * FileSize_2M;
    public const int FileSize_8M = 2 * FileSize_4M;
    public const int FileSize_16M = 2 * FileSize_8M;
    public const int FileSize_32M = 2 * FileSize_16M;
    public const int FileSize_64M = 2 * FileSize_32M;

    public static string ToReadableFileSize(long byteCount)
    {
        string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; // Longs run out around EB
        if (byteCount == 0)
            return "0" + suf[0];
        long bytes = Math.Abs(byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 2);
        return (Math.Sign(byteCount) * num).ToString() + suf[place];
    }
}
