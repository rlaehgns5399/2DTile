using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XDOErrorDetectorUI
{
    public enum EXT
    {
        DAT=1,
        XDO=2
    }
    class FileFinder
    {
        
        private String url;
        public HashSet<String> fileList = new HashSet<String>();
        public FileFinder(String url)
        {
            this.url = url;
        }
        public HashSet<String> run(EXT option)
        {
            DirSearch(this.fileList, this.url, option);
            return this.fileList;
        }
        public void DirSearch(HashSet<String> list, String url, EXT option)
        {
            try
            {
                foreach (string xdoName in Directory.GetFiles(url, "*." + ((EXT)option).ToString()))
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
