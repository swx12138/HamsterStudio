using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.FileSystem;

public static class FileOffestDescriptor
{
    public static string ToReadableOffest(this long offest)
    {
        return "0x" + offest.ToString("X8");
    }
}
