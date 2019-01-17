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
        HashSet<string> folderSet = new HashSet<string>();
        public DirectoryFinder(string url)
        {
            this.url = url;
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
                foreach (string tempDirectoryName in Directory.GetDirectories(url))
                {
                    if (Directory.GetFiles(tempDirectoryName, "*." + ((EXT)option).ToString(), SearchOption.TopDirectoryOnly).Length > 0)
                    {
                        set.Add(tempDirectoryName);
                    }
                    search(set, tempDirectoryName, option);
                }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }
        }
        
    }
}
