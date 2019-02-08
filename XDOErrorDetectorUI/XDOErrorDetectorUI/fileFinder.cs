using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
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
            var sw = new Stopwatch();
            sw.Restart();
            DirSearch(this.fileList, this.url, option);
            sw.Stop();
            if (sw.ElapsedMilliseconds < 1000) Console.WriteLine("[A]\tSearching File(" + this.fileList.Count + "): " + sw.ElapsedMilliseconds.ToString() + "ms\t" + this.url);
            if (sw.ElapsedMilliseconds >= 1000) Console.WriteLine("[A]\tSearching File(" + this.fileList.Count + "): " + sw.ElapsedMilliseconds / 1000.0 + "s\t" + this.url);
            return this.fileList;
        }
        public void DirSearch(HashSet<String> list, String url, EXT option)
        {
            try
            {
                foreach (string Name in Directory.EnumerateFiles(url, "*." + ((EXT)option).ToString()).Where(s => s.Contains("index.dat") == false))
                {
                    list.Add(Name);
                }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }
        }
    }
}
