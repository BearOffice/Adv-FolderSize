using System;

namespace Adv_FolderSize
{
    public static class ColorConsole
    {
        public static void Write(string contents, ConsoleColor color)
        {
            var curcolor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(contents);
            Console.ForegroundColor = curcolor;
        }

        public static void WriteLine(string contents, ConsoleColor color)
        {
            var curcolor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(contents);
            Console.ForegroundColor = curcolor;
        }
    }
}
