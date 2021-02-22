using System;

namespace Adv_FolderSize
{
    public static class ColorConsole
    {
        public static void Write(string contents, ConsoleColor color, bool asline = true)
        {
            var curcolor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            if (asline)
                Console.WriteLine(contents);
            else
                Console.Write(contents);
            Console.ForegroundColor = curcolor;
        }
    }
}
