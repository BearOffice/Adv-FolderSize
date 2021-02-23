using System.Linq;

namespace Adv_FolderSize
{
    public static class StringExtension
    {
        public static string Repeat(this string str, int num)
            => string.Concat(Enumerable.Repeat(str, num));

        public static string LenLimit(this string str, int len, string abbr)
        {
            if (str.Length > len)
                return str[0..(len - 1)] + abbr;
            else
                return str;
        }
    }
}
