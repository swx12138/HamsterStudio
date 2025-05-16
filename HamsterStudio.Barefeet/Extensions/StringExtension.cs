using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.Extensions
{
    public static class StringExtension
    {
        public static bool IsNullOrEmpty(this string s)
        {
            return s == null || s.Length == 0;
        }

        public static int IndexOf5th(this string str, char ch)
        {
            int count = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == ch)
                {
                    count++;
                    if (count == 5)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

    }
}
