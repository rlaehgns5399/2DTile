using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XDOErrorDetectorUI
{
    public enum LOG
    {
        ERR_NOT_EXIST,
        WARN_CASE_INSENSITIVE,
        NOT_USED,
        XDO_LEVEL_ERROR,
        DUPLICATE_XDO,
        XDO_VERSION_ERROR
    }
    class LogItem
    {
        public LOG type { get; set; }
        public string filename { get; set; }
        public string xdoname { get; set; }
        public string detail { get; set; }
        public string found { get; set; }
        public string Y { get; set; }
        public string X { get; set; }
        public string level { get; set; }
    }
    class DATLogItem : LogItem
    {
        public string objCount { get; set; }
        public DATLogItem() { }
        public DATLogItem(string level, string y, string x, LOG type, string filename, string xdoname, string found, string objCount)
        {
            this.objCount = objCount;
            this.level = level;
            this.Y = y;
            this.X = x;
            this.type = type;
            this.filename = filename;
            this.xdoname = xdoname;
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
                case LOG.DUPLICATE_XDO:
                    this.detail = "DAT has XDO which are duplicated";
                    break;
            }
        }
    }
    class XDOLogItem : LogItem
    {

        public int facenum { get; set; }
        public string imgname { get; set; }
        public XDOLogItem()
        {

        }
        public XDOLogItem(string level, string y, string x, LOG type, string filename, int facenum, string imgname, string found)
        {
            this.level = level;
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
        public XDOLogItem(string level, string y, string x, LOG type, string filename, int facenum, string imgname, string found, List<int> lv_checker) : this(level, y, x, type, filename, facenum, imgname, found)
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
