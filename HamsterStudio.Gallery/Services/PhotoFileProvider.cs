using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using System.IO;

namespace HamsterStudio.Gallery.Services;

internal class PhotoFileProvider : IFileProvider
{
    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        var dirCont = new DirectoryInfo(subpath);
        return new PhysicalDirectoryInfo(dirCont);
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        var fileInfo = new FileInfo(subpath);
        return new PhysicalFileInfo(fileInfo);
    }

    public IChangeToken Watch(string filter)
    {
        throw new NotImplementedException();
    }
}
