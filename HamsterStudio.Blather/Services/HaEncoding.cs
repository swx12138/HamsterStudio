using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Encryption.Services;


public static class HaEncoding
{
    /// <summary>
    /// more https://symbl.cc/cn/unicode-table/#alchemical-symbols
    ///  and https://symbl.cc/cn/unicode-table/#egyptian-hieroglyphs
    /// </summary>
    private const ushort Ha = 0x54c8;

    public static char ToHa(char c)
    {
        if (char.IsAsciiLetterUpper(c))
        {
            return char.ConvertFromUtf32(c - 'A' + Ha + 1).First();
        }
        else if (char.IsDigit(c))
        {
            return char.ConvertFromUtf32(c - '0' + Ha + 26 + 1).First();
        }
        else if (char.IsAsciiLetterLower(c))
        {
            return char.ConvertFromUtf32(c - 'a' + Ha + 36 + 1).First();
        }
        else if (c == '/')
        {
            return char.ConvertFromUtf32(Ha + 63).First();
        }
        else if (c == '+')
        {
            return char.ConvertFromUtf32(Ha + 64).First();
        }
        else if (c == '=')
        {
            return char.ConvertFromUtf32(Ha).First();
        }
        throw new ArgumentOutOfRangeException(nameof(c));
    }

    public static string ToHa(string str)
    {
        return new string([.. str.Select(ToHa)]);
    }

    public static char FromHa(int codePoint)
    {
        int offset = codePoint - Ha;

        if (offset == 0)
            return '=';

        else if (offset <= 26)
            return (char)('A' + (offset - 1));

        else if (offset <= 36)
            return (char)('0' + (offset - 27));

        else if (offset <= 62)
            return (char)('a' + (offset - 37));

        else if (offset == 63)
            return '/';

        else if (offset == 64)
            return '+';

        throw new ArgumentOutOfRangeException(nameof(codePoint), $"字符【{char.ConvertFromUtf32(codePoint)}】不在 Ha 编码范围内");
    }

    public static string FromHa(string str)
    {
        return new string([.. str.Select(x => FromHa(x))]);
    }
}
