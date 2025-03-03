namespace HamsterStudio.Toolkits
{
    public static class StringUtils
    {
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
}
