using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XDOErrorDetectorUI
{
    public class ReadDAT
    {
        public string url;
        public header header = new header();
        public List<Obj> body = new List<Obj>();
        public ReadDAT(string url)
        {
            this.url = url;
            BinaryReader br = new BinaryReader(File.Open(url, FileMode.Open));
            header.level = br.ReadUInt32();
            header.IDX = br.ReadUInt32();
            header.IDY = br.ReadUInt32();
            header.objCount = br.ReadUInt32();
            header.datFilename = new FileInfo(url).Name;
            for(uint i = 0; i < header.objCount; i++)
            {
                Obj item = new Obj();
                item.version = br.ReadBytes(4);
                item.type = br.ReadByte();
                item.KeyLen = br.ReadByte();
                if (item.KeyLen > 0)
                {
                    item.key = new string(br.ReadChars(item.KeyLen));
                }
                item.centerPos_x = br.ReadDouble();
                item.centerPos_y = br.ReadDouble();
                item.altitude = br.ReadSingle();
                item.minX = br.ReadDouble();
                item.minY = br.ReadDouble();
                item.minZ = br.ReadDouble();
                item.maxX = br.ReadDouble();
                item.maxY = br.ReadDouble();
                item.maxZ = br.ReadDouble();
                item.ImgLevel = br.ReadByte();

                item.dataFileLen = br.ReadByte();
                if(item.dataFileLen > 0)
                {
                    item.dataFile = new string(br.ReadChars(item.dataFileLen));
                }

                item.imgFileNameLen = br.ReadByte();
                if(item.imgFileNameLen > 0)
                {
                    item.imgFileName = new string(br.ReadChars(item.imgFileNameLen));
                }
                body.Add(item);
            }
            br.Close();
        }
    }

    public class header
    {
        public uint level;
        public uint IDX;
        public uint IDY;
        public uint objCount;
        public string datFilename;
    }
    public class Obj
    {
        public byte[] version;
        public byte type;
        public byte KeyLen;
        public string key;
        public double centerPos_x;
        public double centerPos_y;
        public float altitude;
        public double minX;
        public double minY;
        public double minZ;
        public double maxX;
        public double maxY;
        public double maxZ;
        public byte ImgLevel;
        public byte dataFileLen;
        public string dataFile;
        public byte imgFileNameLen;
        public string imgFileName;
    }
}
