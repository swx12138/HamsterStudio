namespace HamsterStudio.SinaWeibo.Services;

internal class FilenameFormatter
{
    private string indentifier = "weibo";
    private string home = @"D:\Code\HamsterStudio\Samples\Imgs\";

    public string Format(string showId, string userId, string filename, int idx = -1)
    {
        ArgumentException.ThrowIfNullOrEmpty(showId);
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentException.ThrowIfNullOrEmpty(filename);

        return idx >= 0 ?
            $"{showId}_{idx}_{indentifier}_{userId}_{filename}" :
            $"{showId}_{indentifier}_{userId}_{filename}";
    }

    public string GetFullPath(string filename)
    {
        ArgumentException.ThrowIfNullOrEmpty(filename);
        return Path.Combine(home, filename);
    }
}
