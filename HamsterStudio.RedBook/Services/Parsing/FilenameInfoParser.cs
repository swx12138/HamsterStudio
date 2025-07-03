using CommunityToolkit.HighPerformance;
using HamsterStudio.Barefeet.FileSystem.Interfaces;

namespace HamsterStudio.RedBook.Services.Parsing;

internal class FilenameInfoParser : IFilenameInfoParser
{
    public FilenameInfo Parse(string filename)
    {
        var parts = filename.Split('_').ToArray();
        var pos = Array.IndexOf(parts, "xhs");
        if (pos == -1) return new FilenameInfo
        {
            Title = string.Empty,
            Owner = string.Empty
        };
        return new FilenameInfo
        {
            Title = string.Join('_', parts[0..(pos == 1 ? 1 : pos - 1)]),
            Owner = parts[pos + 1]
        };
    }
}
