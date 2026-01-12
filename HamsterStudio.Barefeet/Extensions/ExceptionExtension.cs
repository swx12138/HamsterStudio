using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.Extensions
{
    public static class ExceptionExtension
    {
        public static string ToFullString(this Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            Exception? currentEx = ex;
            int level = 0;
            while (currentEx != null)
            {
                sb.AppendLine($"[Level {level}] {currentEx.GetType().FullName}: {currentEx.Message}");
                sb.AppendLine(currentEx.StackTrace ?? "No stack trace available.");
                sb.AppendLine();
                currentEx = currentEx.InnerException;
                level++;
            }
            return sb.ToString();
        }
    }
}
