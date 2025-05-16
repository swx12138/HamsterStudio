using HamsterStudio.Barefeet.Logging;
using System.Text.RegularExpressions;

namespace HamsterStudio.Barefeet.Extensions;

public static class FileNameUtil
{
    // 非法字符正则表达式（包含空格和点）
    private static readonly HashSet<char> InvalidChars = new() {
        ':','/','\\','*','?','"','<','>','|',
    };

    // 保留名称列表（不区分大小写）
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    /// <summary>
    /// 将字符串转换为有效文件名
    /// </summary>
    public static string SanitizeFileName(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "untitled";

        // 1. 替换非法字符
        string sanitized = new([.. input.Where(c => !InvalidChars.Contains(c))]);            

        // 2. 处理保留名称
        if (ReservedNames.Contains(sanitized))
            sanitized += "_";

        // 3. 修剪首尾空格和点
        sanitized = sanitized.Trim().TrimEnd('.');

        // 4. 处理空结果或全点的情况
        if (string.IsNullOrEmpty(sanitized))
            throw new ArgumentException("Invalid file name: all characters are invalid.");

        // 5. 截断至255字符
        if (sanitized.Length > 255)
        {
            Logger.Shared.Warning($"File name too long, truncating to 255 characters: {sanitized}");
            sanitized = sanitized[..255];
            Logger.Shared.Information($"Truncated to {sanitized}");
        }

        return sanitized;
    }

    public static string Filename(this string str)
    {
        string[] part = str.Split("?");
        part = part[0].Split("/");
        if (part.Length == 1)
        {
            part = part[0].Split("\\");
        }
        return part.Last();
    }

    public static string Stem(this string str)
    {
        string[] part = str.Filename().Split(".");
        return part[0];
    }

}