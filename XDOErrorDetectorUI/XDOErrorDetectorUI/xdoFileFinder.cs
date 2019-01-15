using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XDOErrorDetectorUI
{
    class xdoFileFinder
    {
        private String url;
        public HashSet<String> fileList = new HashSet<String>();
        public xdoFileFinder(String url)
        {
            this.url = url;
        }
        public HashSet<String> run()
        {
            DirSearch(this.fileList, this.url);
            return this.fileList;
        }
        public void DirSearch(HashSet<String> list, String url)
        {
            try
            {
                    foreach (string xdoName in Directory.GetFiles(url, "*.xdo"))
                    {
                        list.Add(xdoName);
                    }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }
        }
    }
}
