using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Constants
{
    public static class SystemConsts
    {
        public static string ApplicationName { get; } = "Hamster Studio";
        public static string ApplicationLogTag { get; } = "HamsterStudio";
        public static string ApplicationRegeditKey { get; } = "HamsterStudio";
        public static long MilisecondTimestamp => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    }
}
