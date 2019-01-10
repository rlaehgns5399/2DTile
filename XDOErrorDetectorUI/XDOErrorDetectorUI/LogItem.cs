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
        public LogItem(LOG type, string filename, int facenum, string imgname)
        {
            this.type = type;
            this.filename = filename;
            this.facenum = facenum;
            this.imgname = imgname;
        }
    }
}
