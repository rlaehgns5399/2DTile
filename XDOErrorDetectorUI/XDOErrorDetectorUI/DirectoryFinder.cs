using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
            search(folderSet, this.url, option);
            return folderSet;
        }
        public void search(HashSet<string> set, string url, EXT option)
        {
            try
            {
                // Z (zoom level)
                for(int i = min; i <= max; i++)
                {
                    // Y
                    foreach(var tempDirectoryName in Directory.EnumerateDirectories(Path.Combine(url, i.ToString()), "*", SearchOption.TopDirectoryOnly))
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
        
    }
}
