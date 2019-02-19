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
        XDO_VERSION_ERROR,
        DAT_CANNOT_PARSE_INVALID_XDONAME,
        DAT_CANNOT_PARSE_NOT_EXIST_DIRECTORY,
        XDO_CANNOT_FIND_PARENT_DAT,
        INVALID_FILENAME_IN_DIRECTORY,
        DAT_XDO_VERSION_MISMATCH,
        INVALID_XDO
    }
    public class LogItem
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
    public class DATLogItem : LogItem
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
                case LOG.DAT_CANNOT_PARSE_INVALID_XDONAME:
                    this.detail = "There are invalid XDO name";
                    break;
                case LOG.DAT_CANNOT_PARSE_NOT_EXIST_DIRECTORY:
                    this.detail = "Cannot find a directory at given DAT";
                    break;
                case LOG.INVALID_FILENAME_IN_DIRECTORY:
                    this.detail = "Invalid filename in folder(carriage return or line feed)";
                    break;
            }
        }
    }

    public class XDOLogItem : LogItem
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
                case LOG.INVALID_XDO:
                    this.detail = "This XDO is not 3.0.0.1 & 3.0.0.2, cannot parse";
                    break;
                case LOG.XDO_CANNOT_FIND_PARENT_DAT:
                    this.detail = "Y_X.dat is not exist";
                    break;
                case LOG.DAT_XDO_VERSION_MISMATCH:
                    this.detail = "DAT & XDO version are mismatch";
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
