using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XDOErrorDetectorUI
{
    class xdoDirectoryFinder
    {
        string url;
        HashSet<string> folderSet = new HashSet<string>();
        public xdoDirectoryFinder(string url)
        {
            this.url = url;
        }
        public HashSet<string> run()
        {
            search(folderSet, this.url);
            return folderSet;
        }
        public void search(HashSet<string> set, string url)
        {
            try
            {
                foreach (string tempDirectoryName in Directory.GetDirectories(url))
                {
                    if(Directory.GetFiles(tempDirectoryName, "*.xdo").Length > 0)
                    {
                        set.Add(tempDirectoryName);
                    }
                    search(set, tempDirectoryName);
                }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }
        }
        
    }
}
