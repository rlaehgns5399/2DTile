using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XDOErrorDetectorUI
{
    class LogItem
    {
        public LOG type { get; set; }
        public string filename { get; set; }
        public int facenum { get; set; }
        public string imgname { get; set; }
        public string detail { get; set; }
        public string found { get; set; }
        public string Y { get; set; }
        public string X { get; set; }
        public LogItem()
        {

        }
        public LogItem(string y, string x, LOG type, string filename, int facenum, string imgname, string found)
        {
            this.Y = y;
            this.X = x;
            this.type = type;
            this.filename = filename;
            this.facenum = facenum;
            this.imgname = imgname;
            this.found = found;
            switch (this.type)
            {
                case LOG.ERR_NOT_EXIST:
                    this.detail = "Not exist";
                    break;
                case LOG.NOT_USED:
                    this.detail = "Unused file";
                    break;
                case LOG.WARN_CASE_INSENSITIVE:
                    this.detail = "Case sensitive error";
                    break;
            }
        }
        public LogItem(string y, string x, LOG type, string filename, int facenum, string imgname, string found, List<int> lv_checker) : this(y, x, type, filename, facenum, imgname, found)
        {
            switch (this.type)
            {
                case LOG.XDO_LEVEL_ERROR:
                    string temp = "[" + string.Join(", ", lv_checker.ToArray()) + "]";
                    this.detail = "Level " + temp + " textures are not exist";
                    break;
            }
        }
    }
}
