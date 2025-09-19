using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.Services;

public class StaticFileReflactor
{
    public string BaseDirectory { get; private set; }
    public string BaseAddress { get; private set; }

    public StaticFileReflactor(DirectoryInfo directoryInfo, string baseAddr)
    {
        ArgumentNullException.ThrowIfNull(directoryInfo);
        if (!directoryInfo.Exists)
        {
            throw new DirectoryNotFoundException();
        }

        BaseDirectory = directoryInfo.FullName;

        if (baseAddr.EndsWith('/'))
        {
            BaseAddress = baseAddr;
        }
        else
        {
            BaseAddress = baseAddr + '/';
        }
    }

    public string? GetStaticFilePath(string filePath)
    {
        if (filePath == null) { return null; }
        if (!File.Exists(filePath)) { return null; }

        var relPath = Path.GetRelativePath(BaseDirectory, filePath);
        if (relPath == filePath) { return null; }

        return BaseAddress + relPath;
    }

}
