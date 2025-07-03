using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.FileSystem.Interfaces;

public class FilenameInfo
{
    public string Title { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
}

public interface IFilenameInfoParser
{
    FilenameInfo Parse(string filename);
}
