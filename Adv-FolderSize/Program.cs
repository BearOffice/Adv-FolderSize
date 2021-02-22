using System;
using System.IO;
using System.Diagnostics;
using TriggerLib;
using FolderAnalysisFSLib;

namespace Adv_FolderSize
{
    class Program
    {
        private const string _helpMsg =
@"Enter help(h) to get help message
Enter scan(s) [path] to scan all directories and files below the path specified
Enter tree(t) to print the scan\'s result
      Option: Measure            /m[auto|b|kb|mb|gb]  Default is auto
              Dir depth limit    /dp[number]          Default is 3
              Dir expand limit   /de[number]          Default is 3
              File expand limit  /fe[number]          Default is 3
Enter dirlist(d) to print the directories\' paths descending by size
      Option: Measure            /m[auto|b|kb|mb|gb]  Default is auto
              Number to display  /n[number]           Default is 10
Enter filelist(f) to print the files\' paths descending by size
      Option: Measure            /m[auto|b|kb|mb|gb]  Default is auto
              Number to display  /n[number]           Default is 10
Enter redirect(r) [index] to move into the specified tree
Enter back(b) to back to the previous tree
Enter open(o) [index] to open the folder or the file specified
Enter exit(e) to exit";

        static void Main(string[] args)
        {
            var fa = new FolderAnalysis("");
            var lastdisplayed = 0b0; // 0b0 -> tree  0b1 -> list

            Console.WriteLine("Message: Enter help(h) to get help message");
            ColorConsole.Write("Suggest command: scan(s) [path]", ConsoleColor.DarkGreen);
            while (true)
            {
                try
                {
                    Console.Write("> ");
                    var line = Console.ReadLine();
                    var result = LineInterpreter(line, out var opt);

                    switch (result)
                    {
                        case "HELP" or "H":
                            Console.WriteLine(_helpMsg);
                            break;
                        case "SCAN" or "S":
                            if (!Directory.Exists(opt[0]))
                                throw new DirectoryNotFoundException();

                            var trigger = new TriggerSource(300, () =>
                                Console.WriteLine("... This scaning operation will take several seconds"));

                            fa = new FolderAnalysis(opt[0]);
                            fa.StartAnalysisAsync().Wait();

                            trigger.Cancel();
                            Console.WriteLine("Scaning finished.");

                            ColorConsole.Write(
                                "Suggest command: tree(t) [/m /dp /de /fe] | dirlist(d) [/m /n] | filelist(f) [/m /n]",
                                ConsoleColor.DarkGreen);
                            break;
                        case "TREE" or "T":
                            fa.PrintDirTree(opt[0], int.Parse(opt[1]), int.Parse(opt[2]), int.Parse(opt[3]));

                            lastdisplayed = 0b0;
                            ColorConsole.Write(
                                "Suggest command: dirlist(d) [/m /n] | filelist(f) [/m /n] | redirect(r) [index]", 
                                ConsoleColor.DarkGreen);
                            break;
                        case "DIRLIST" or "D":
                            fa.PrintDirList(opt[0], int.Parse(opt[1]));

                            lastdisplayed = 0b1;
                            ColorConsole.Write(
                                "Suggest command: tree(t) [/m /dp /de /fe] | filelist(f) [/m /n]", ConsoleColor.DarkGreen);
                            break;
                        case "FILELIST" or "F":
                            fa.PrintFileList(opt[0], int.Parse(opt[1]));

                            lastdisplayed = 0b1;
                            ColorConsole.Write("Suggest command: tree(t) [/m /dp /de /fe] | dirlist(d) [/m /n]", 
                                ConsoleColor.DarkGreen);
                            break;
                        case "REDIRECT" or "R":
                            if (!fa.RedirectTo(int.Parse(opt[0])))
                                throw new Exception("The specified index does not exist.");

                            ColorConsole.Write("Suggest command: tree(t) [/m /dp /de /fe] ", ConsoleColor.DarkGreen);
                            break;
                        case "BACK" or "B":
                            if (!fa.Back())
                                throw new Exception("Can not go back anymore.");

                            ColorConsole.Write("Suggest command: tree(t) [/m /dp /de /fe] ", ConsoleColor.DarkGreen);
                            break;
                        case "OPEN" or "O":
                            var path = "";

                            if (lastdisplayed == 0b0)
                                path = fa.GetTreeDirPath(int.Parse(opt[0]));
                            else
                                path = fa.GetListElemPath(int.Parse(opt[0]));

                            if (path == null)
                                throw new Exception("The specified index does not exist.");

                            OpenExplorer(path);
                            break;
                        case "EXIT" or "E":
                            Environment.Exit(0);
                            break;
                        default:
                            throw new Exception("Invalid command.");
                    }
                }
                catch (NotAnalyzedYetException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    ColorConsole.Write("Suggest command: scan(s) [path]", ConsoleColor.DarkGreen);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Console.WriteLine();
            }
        }

        static string LineInterpreter(string line, out string[] options)
        {
            var splited = line.Trim().Split(' ');
            var cmd = splited[0].ToUpper();
            var args = string.Join(' ', splited[1..]);
            
            options = cmd switch
            {
                "HELP" or "H" or "BACK" or "B" or "EXIT" or "E" => Args.Interprete(new[] { new ArgSet() }, args),
                "SCAN" or "S" => Args.Interprete(new[] { new ArgSet { Default = null } }, args, remainder: true),
                "TREE" or "T" => Args.Interprete(new[] {
                    new ArgSet { Key = "/M" ,Default = "AUTO" },
                    new ArgSet { Key="/DP", Default = "3" },
                    new ArgSet { Key ="/DE", Default = "3" },
                    new ArgSet { Key="/FE", Default = "3" } }, args),
                "DIRLIST" or "D" => Args.Interprete(new[] {
                    new ArgSet { Key = "/M", Default = "AUTO" },
                    new ArgSet { Key = "/N", Default = "10" } }, args),
                "FILELIST" or "F" => Args.Interprete(new[] {
                    new ArgSet { Key = "/M", Default = "AUTO" },
                    new ArgSet { Key = "/N", Default = "10" } }, args),
                "REDIRECT" or "R" => Args.Interprete(new[] { new ArgSet { Default = null } }, args),
                "OPEN" or "O" => Args.Interprete(new[] { new ArgSet { Default = null } }, args),
                _ => null,
            };
            return options != null ? cmd : null;
        }

        static void OpenExplorer(string path)
        {
            var platform = Environment.OSVersion.Platform;

            if (platform == PlatformID.Win32NT)
                Process.Start("explorer.exe", "/select," + path);
            else if (platform == PlatformID.Unix)
                Process.Start("open", $"-R \"{path}\"");
            else
                throw new Exception("Unknown operating system.");
        }
    }
}

