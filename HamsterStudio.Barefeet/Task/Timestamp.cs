using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.Task;

public static class Timestamp
{
    public static long Now => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public static long NowMs => Now + DateTime.UtcNow.Millisecond;
}
