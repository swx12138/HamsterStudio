using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.FileSystem;

public class HamstertFileInfo(string filename)
{
    public string Name { get; } = Path.GetFileName(filename);
    public string FullName { get; } = Path.GetFullPath(filename);
    public string Directory { get; } = Path.GetDirectoryName(filename) ?? Environment.CurrentDirectory;
}
