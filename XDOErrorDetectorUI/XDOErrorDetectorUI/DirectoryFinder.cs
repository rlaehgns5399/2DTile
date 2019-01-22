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
        public DirectoryFinder(string url, int min, int max)
        {
            this.url = url;
            for(var i = min; i <= max; i++)
            {
                minmax.Add(i.ToString());
            }
        }
        public HashSet<string> run(EXT option)
        {
            search(folderSet, this.url, option);
            leaveSpecificLevel(folderSet);
            return folderSet;
        }
        public void leaveSpecificLevel(HashSet<string> set) {
            var forRemoveSet = new HashSet<string>();
            foreach(var folder in set)
            {
                var str = folder.Split('\\');
                bool flag = false;
                for(int i = 0; i < str.Length; i++)
                {
                    if (minmax.Contains(str[i]))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag) forRemoveSet.Add(folder);
            }

            foreach (var t in forRemoveSet)
                set.Remove(t);
        }
        public void search(HashSet<string> set, string url, EXT option)
        {
            try
            {
                foreach (var tempDirectoryName in Directory.GetDirectories(url))
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
