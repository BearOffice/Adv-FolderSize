using System;

namespace Adv_FolderSize
{
    class Program
    {
        static void Main(string[] args)
        {
            var cui = new CUI();
            Console.WriteLine();

            while (true)
            {
                Console.Write("> ");
                cui.Input(Console.ReadLine());
                Console.WriteLine();
            }
        }
    }
}

