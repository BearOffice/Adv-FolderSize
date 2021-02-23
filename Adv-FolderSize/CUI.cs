using System;
using System.Diagnostics;
using System.IO;
using FolderAnalysisFSLib;
using TriggerLib;

namespace Adv_FolderSize
{
    public class CUI
    {
        private FolderAnalysis _fA = new FolderAnalysis("");
        private byte _lastDisplayed = 0b0; // 0b0 -> tree  0b1 -> list

        private const string _helpMsg =
@"Enter help(h) to get help message
Enter scan(s) [path] to scan all directories and files below the path specified
Enter tree(t) to print the scan\'s result
      Option: Measure            /m[auto|b|kb|mb|gb]  Default is auto
              Dir depth limit    /dp[number]          Default is 2
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

        private const string _cmdHelp = "help(h)";
        private const string _cmdScan = "scan(s) [path]";
        private const string _cmdTree = "tree(t) [/m /dp /de /fe]";
        private const string _cmdDirL = "dirlist(d)[/ m / n]";
        private const string _cmdFileL = "filelist(f) [/m /n]";
        private const string _cmdRed = "redirect(r) [index]";
        private const string _cmdBack = "back(b)";
        private const string _cmdOpen = "open(o)";

        public CUI()
        {
            Console.WriteLine("Message: Enter help(h) to get help message");
            WriteSuggest("scan(s) [path]");
        }

        public void Input(string line)
        {
            var result = LineInterpreter(line, out var opt);

            try
            {
                CmdSelect(result, opt);
            }
            catch (NotAnalyzedYetException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                WriteSuggest(_cmdScan);
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                WriteSuggest(_cmdTree, _cmdDirL, _cmdFileL);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                WriteSuggest(_cmdHelp);
            }
        }

        private void CmdSelect(string cmd, string[] opt)
        {
            switch (cmd)
            {
                case "HELP" or "H":
                    Console.WriteLine(_helpMsg);
                    break;
                case "SCAN" or "S":
                    if (!Directory.Exists(opt[0]))
                        throw new DirectoryNotFoundException();

                    var sw = new Stopwatch();
                    sw.Start();
                    var trigger = new TriggerSource(300, () =>
                        Console.WriteLine("... This scaning operation will take several seconds."));

                    _fA = new FolderAnalysis(opt[0]);
                    _fA.StartAnalysisAsync().Wait();

                    sw.Stop();
                    trigger.Cancel();
                    Console.WriteLine($"Scaning finished. Time cost: {Math.Round(sw.Elapsed.TotalSeconds, 2)}s.");

                    WriteSuggest(_cmdTree, _cmdDirL, _cmdFileL);
                    break;
                case "TREE" or "T":
                    _fA.PrintDirTree(opt[0], int.Parse(opt[1]), int.Parse(opt[2]), int.Parse(opt[3]));

                    _lastDisplayed = 0b0;
                    WriteSuggest(_cmdDirL, _cmdFileL, _cmdRed);
                    break;
                case "DIRLIST" or "D":
                    _fA.PrintDirList(opt[0], int.Parse(opt[1]));

                    _lastDisplayed = 0b1;
                    WriteSuggest(_cmdTree, _cmdFileL, _cmdOpen);
                    break;
                case "FILELIST" or "F":
                    _fA.PrintFileList(opt[0], int.Parse(opt[1]));

                    _lastDisplayed = 0b1;
                    WriteSuggest(_cmdTree, _cmdDirL, _cmdOpen);
                    break;
                case "REDIRECT" or "R":
                    if (!_fA.RedirectTo(int.Parse(opt[0])))
                        throw new IndexOutOfRangeException("The specified index does not exist.");

                    _fA.PrintDirTree("AUTO", 2, 3, 1);
                    WriteSuggest(_cmdTree, _cmdRed, _cmdBack);
                    break;
                case "BACK" or "B":
                    if (!_fA.Back())
                        throw new IndexOutOfRangeException("Can not go back anymore.");

                    _fA.PrintDirTree("AUTO", 2, 3, 1);
                    WriteSuggest(_cmdTree, _cmdRed, _cmdBack);
                    break;
                case "OPEN" or "O":
                    string path;
                    if (_lastDisplayed == 0b0)
                        path = _fA.GetTreeDirPath(int.Parse(opt[0]));
                    else
                        path = _fA.GetListElemPath(int.Parse(opt[0]));

                    if (path == null)
                        throw new IndexOutOfRangeException("The specified index does not exist.");

                    OpenExplorer(path);
                    break;
                case "EXIT" or "E":
                    Environment.Exit(0);
                    break;
                default:
                    throw new Exception("Invalid command.");
            }
        }

        private static void WriteSuggest(string sug, params string[] sugs)
        {
            var middlesym = " | ";

            string suggests;
            if (sugs.Length == 0)
                suggests = sug;
            else
                suggests = sug + middlesym + string.Join(middlesym, sugs);

            ColorConsole.WriteLine($"Suggest command: {suggests}", ConsoleColor.DarkGreen);
        }

        private static string LineInterpreter(string line, out string[] options)
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
                    new ArgSet { Key="/DP", Default = "2" },
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

        private static void OpenExplorer(string path)
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
