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
        header header = new header();
        List<Obj> body = new List<Obj>();
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
                br.ReadBytes(4);
                body.Add(item);
            }
        }
    }

    class header
    {
        public uint level { get; set; }
        public uint IDX { get; set; }
        public uint IDY { get; set; }
        public uint objCount { get; set; }
    }
    class Obj
    {
        public byte[] version { get; set; }
        public byte type { get; set; }
        public byte keyLen { get; set; }
        public string key { get; set; }
        public double centerPos_x { get; set; }
        public double centerPos_y { get; set; }
        public float altitude { get; set; }
        public double minX { get; set; }
        public double minY { get; set; }
        public double minZ { get; set; }
        public double maxX { get; set; }
        public double maxY { get; set; }
        public double maxZ { get; set; }
        public byte ImgLevel { get; set; }
        public byte dataFileLen { get; set; }
        public string dataFile { get; set; }
        public byte imgFileNameLen{ get; set; }
        public string imgFileName { get; set; }
    }
}
