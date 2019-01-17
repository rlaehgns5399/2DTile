using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XDOErrorDetectorUI
{
    class DAT
    {
        public header header = new header();
        public List<Obj> body = new List<Obj>();
        public DAT(string url)
        {
            BinaryReader br = new BinaryReader(File.Open(url, FileMode.Open));
            header.level = br.ReadUInt32();
            header.IDX = br.ReadUInt32();
            header.IDY = br.ReadUInt32();
            header.objCount = br.ReadUInt32();

            for(uint i = 0; i < header.objCount; i++)
            {
                Obj item = new Obj();
                item.version = br.ReadBytes(4);
                item.type = br.ReadByte();
                var keyLen = br.ReadByte();
                if (keyLen > 0)
                {
                    item.key = new string(br.ReadChars(keyLen));
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

                var dataFileLen = br.ReadByte();
                if(dataFileLen > 0)
                {
                    item.dataFile = new string(br.ReadChars(dataFileLen));
                }

                var imgFileNameLen = br.ReadByte();
                if(imgFileNameLen > 0)
                {
                    item.imgFileName = new string(br.ReadChars(imgFileNameLen));
                }
                body.Add(item);
            }
        }
    }

    class header
    {
        public uint level;
        public uint IDX;
        public uint IDY;
        public uint objCount;
    }
    class Obj
    {
        public byte[] version;
        public byte type;
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
        public string dataFile;
        public string imgFileName;
    }
}
