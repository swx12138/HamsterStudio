using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.Constants;

public static class FileConstants
{
    public const int FileSize_1M = 2 * 1024 * 1024;
    public const int FileSize_2M = 2 * FileSize_1M;
    public const int FileSize_4M = 2 * FileSize_2M;
    public const int FileSize_8M = 2 * FileSize_4M;
    public const int FileSize_16M = 2 * FileSize_8M;
    public const int FileSize_20M = 5 * FileSize_4M;
    public const int FileSize_32M = 2 * FileSize_16M;
    public const int FileSize_64M = 2 * FileSize_32M;
}
