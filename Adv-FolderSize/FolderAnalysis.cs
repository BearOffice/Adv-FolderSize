using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FolderAnalysisFSLib;

namespace Adv_FolderSize
{
    public class FolderAnalysis
    {
        private readonly FolderAnalysisBase _fABase;
        private readonly List<string> _selectableTree = new List<string>();
        private readonly List<string> _selectableList = new List<string>();

        public FolderAnalysis(string path)
            => _fABase = new FolderAnalysisBase(path);

        public async Task StartAnalysisAsync()
            => await Task.Run(() => _fABase.StartAnalysis());

        public bool RedirectTo(int idx)
        {
            if (idx >= 0 && idx < _selectableTree.Count)
                return _fABase.RedirectTo(_selectableTree[idx]);
            else
                return false;
        }

        public bool Back()
            => _fABase.Back();

        public string GetListElemPath(int idx)
        {
            if (idx >= 0 && idx < _selectableList.Count)
                return _selectableList[idx];
            else
                return null;
        }

        public string GetTreeDirPath(int idx)
        {
            if (idx >= 0 && idx < _selectableTree.Count)
                return _selectableTree[idx];
            else
                return null;
        }

        private static string SizeBar(long stand, long size, int length)
        {
            if (stand != 0)
                return "■".Repeat((int)(size * length / stand));
            else
                return "";
        }


        private static string DirAbbrevName(string path)
            => Path.DirectorySeparatorChar + Path.GetFileName(path);

        public void PrintDirTree(string measure, int depthLimt, int dirExpLimt, int fileExpLimt)
        {
            // Length = 4
            var diridx = "│   ";
            var sym = "├── ";
            var tabidx = "    ";

            var printline = _fABase.GetPrintableTree(depthLimt, dirExpLimt, fileExpLimt);

            _selectableTree.Clear();
            var idxcount = -1;
            var dirtopsize = _fABase.GetDirList(top: true, 1);
            var filetopsize = _fABase.GetFileList(top: true, 1);
            var dirsizestand = dirtopsize.Length != 0 ? dirtopsize[0].Item2 : 0;
            var filesizestand = filetopsize.Length != 0 ? filetopsize[0].Item2 : 0;

            foreach (var item in printline)
            {
                var type = item.Item1;
                var name = item.Item2;
                var size = item.Item3;
                var depth = item.Item4;

                switch (type)
                {
                    case "F":
                        Console.WriteLine(
                            $"{SizeBar(filesizestand, size, 16),16} {ByteMeasure.byteToString(measure, size),10} " +
                            $"{tabidx.Repeat(depth)}{sym}{name}");
                        break;
                    case "D":
                        ColorConsole.Write(
                            $"{SizeBar(dirsizestand, size, 16),16} {ByteMeasure.byteToString(measure, size),10} "
                            , ConsoleColor.Cyan);
                        Console.Write($"{diridx.Repeat(depth)}{sym}");

                        string print;
                        if (idxcount == -1)
                        {
                            print = name;
                            idxcount++;
                        }
                        else if (depth == 1)
                        {
                            _selectableTree.Add(name);
                            ColorConsole.Write($"{idxcount} ", ConsoleColor.DarkYellow);
                            print = DirAbbrevName(name);
                            idxcount++;
                        }
                        else
                        {
                            print = DirAbbrevName(name);
                        }
                        ColorConsole.WriteLine(print, ConsoleColor.Cyan);
                        break;
                    case "FH":
                        Console.Write($"{"",16} {"",-10} {tabidx.Repeat(depth)}{sym}");
                        ColorConsole.WriteLine($"... {name} files are hided", ConsoleColor.Yellow);
                        break;
                    case "DH":
                        Console.Write($"{"",16} {"",-10} {tabidx.Repeat(depth + 1)}{sym}");
                        ColorConsole.WriteLine($"... {name} directories are hided", ConsoleColor.Yellow);
                        break;
                    case "DF":
                        Console.Write($"{"",16} {"",-10} {tabidx.Repeat(depth + 1)}{sym}");
                        ColorConsole.WriteLine($"... {name} directories above this directory are folded", ConsoleColor.Yellow);
                        break;
                    default:
                        break;
                }
            }
        }

        public void PrintDirList(string measure = "AUTO", int num = 10)
        {
            _selectableList.Clear();
            var idxcount = 0;

            var dirlist = _fABase.GetDirList(top: true, num);
            var dirsizestand = dirlist.Length != 0 ? dirlist[0].Item2 : 0;

            foreach (var item in dirlist)
            {
                var name = item.Item1;
                var size = item.Item2;

                ColorConsole.Write($"{SizeBar(dirsizestand, size, 16),16} {ByteMeasure.byteToString(measure, size),10} ",
                    ConsoleColor.Cyan);
                ColorConsole.Write($"[{idxcount}] ", ConsoleColor.DarkYellow);
                ColorConsole.WriteLine(name, ConsoleColor.Cyan);

                _selectableList.Add(name);
                idxcount++;
            }
        }

        public void PrintFileList(string measure = "AUTO", int num = 10)
        {
            _selectableList.Clear();
            var idxcount = 0;

            var filelist = _fABase.GetFileList(top: true, num);
            var filesizestand = filelist.Length != 0 ? filelist[0].Item2 : 0;

            foreach (var item in filelist)
            {
                var name = item.Item1;
                var size = item.Item2;

                Console.Write($"{SizeBar(filesizestand, size, 16),16} {ByteMeasure.byteToString(measure, size),10} ");
                ColorConsole.Write($"[{idxcount}] ", ConsoleColor.DarkYellow);
                Console.WriteLine(name);

                _selectableList.Add(name);
                idxcount++;
            }
        }
    }
}
