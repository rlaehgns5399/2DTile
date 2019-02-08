using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
namespace XDOErrorDetectorUI
{
    class DirectoryFinder
    {
        string url;
        List<string> minmax = new List<string>();
        HashSet<string> folderSet = new HashSet<string>();
        int min, max;
        public DirectoryFinder(string url, int min, int max)
        {
            this.url = url;
            this.min = min;
            this.max = max;
        }
        public HashSet<string> run(EXT option)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            search(folderSet, this.url, option);
            watch.Stop();

            if(watch.ElapsedMilliseconds < 1000)
            {
                Console.WriteLine("[A]\tSearching Folder(" + folderSet.Count + "): " + watch.ElapsedMilliseconds.ToString() + "ms");
            }
            else
            {
                Console.WriteLine("[A]\tSearching Folder(" + folderSet.Count + "): " + watch.ElapsedMilliseconds / 1000.0 + "s");
            }
            return folderSet;
        }
        public void search(HashSet<string> set, string url, EXT option)
        {
            if(option == EXT.DAT)
            {
                try
                {
                    // Z (zoom level)
                    for (int i = min; i <= max; i++)
                    {
                        Console.WriteLine("[A]\tSearching Zoom Level " + i);
                        // Y, 더 이상 깊게 들어갈 이유가 없다.
                        foreach (var tempDirectoryName in Directory.EnumerateDirectories(Path.Combine(url, i.ToString()), "*", SearchOption.TopDirectoryOnly))
                        {
                            if (Directory.EnumerateFiles(tempDirectoryName, "*." + ((EXT)option).ToString(), SearchOption.TopDirectoryOnly).Count() > 0)
                            {
                                set.Add(tempDirectoryName);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.ToString());
                }
            }
            else if(option == EXT.XDO)
            {

            }
        }
        
    }
}
