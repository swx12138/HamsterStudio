using HamsterStudio.Barefeet.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.Models.FileMgmt;

public class FileModel(string filename)
{
    public string Filename { get; } = Path.GetFileName(filename);
    public string FullPath { get; } = Path.GetFullPath(filename);

    public FileStream? ReadFile()
    {
        if (!File.Exists(FullPath)) { return null; }

        try { return File.OpenRead(FullPath); }
        catch (Exception ex)
        {
            Logger.Shared.Error($"Read {filename} failed, {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    public async Task<bool> WriteFile(Stream data, FileMode mode)
    {
        if (!File.Exists(FullPath)) { return false; }

        try
        {
            using Stream stream = File.Open(FullPath, mode, FileAccess.Write, FileShare.Read);
            await data.CopyToAsync(stream);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Shared.Error($"Write {filename} failed, {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }

}

public class DirectoryModel(string directory)
{
    public string DisplayName { get; } = GetDirectoryName(directory);
    public string FullDirectory { get; } = Directory.GetParent(directory)?.FullName ?? throw new DirectoryNotFoundException(directory);

    public IReadOnlyCollection<FileModel> GetFiles() { throw new NotImplementedException(); }
    public FileModel CreateFile(string filename) { throw new NotImplementedException(); }

    public static string GetDirectoryName(string dir)
    {
        dir = dir.Trim().TrimEnd('/', '\\');
        return dir.Split("/").Last().Split("\\").Last();
    }
}

public class FileManager
{
    private Dictionary<int, string> registeredDir = [];

    private int GetToken()
    {
        throw new NotImplementedException();
    }

    public bool RegisterSubdir(string dirName, out int token)
    {
        throw new NotImplementedException();
    }

    public DirectoryModel GetDirectory(int token)
    {
        throw new NotImplementedException();
    }

}
