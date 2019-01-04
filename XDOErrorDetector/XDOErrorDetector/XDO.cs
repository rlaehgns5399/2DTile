using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;

namespace XDOErrorDetector
{
    class XDO
    {
        public List<XDOMesh> mesh = new List<XDOMesh>();
        public String url;
        public byte XDOType;
        public uint ObjectID;
        public byte KeyLen;
        public string Key;
        public double minX, minY, minZ, maxX, maxY, maxZ;
        public float altitude;
        public byte faceNum;
        public int XDOVersion;
        public XDO(string url)
        {
            this.url = url;
            BinaryReader br = new BinaryReader(File.Open(this.url, FileMode.Open));


            // type
            this.XDOType = br.ReadByte();
            // ObjectID
            this.ObjectID = br.ReadUInt32();
            // KeyLen
            this.KeyLen = br.ReadByte();
            // Key
            this.Key = System.Text.Encoding.UTF8.GetString(br.ReadBytes(this.KeyLen));
            // ObjBox
            this.minX = br.ReadDouble();
            this.minY = br.ReadDouble();
            this.minZ = br.ReadDouble();
            this.maxX = br.ReadDouble();
            this.maxY = br.ReadDouble();
            this.maxZ = br.ReadDouble();
            // Altitude
            this.altitude = br.ReadSingle();
            // faceNum
            this.faceNum = br.ReadByte();

            byte[] versionChecker = br.ReadBytes(4);
            int isZero = (int)versionChecker[3];

            int temp = 0;
            if (isZero == 0)
            {
                temp = 1;
            }
            else
            {
                this.faceNum = 0;
            }

            if(temp == 0)
            {
                // Console.WriteLine("XDO version 3.0.0.1");
                br.BaseStream.Position -= 5;
                this.mesh.Add(new XDOMesh(br));
                this.XDOVersion = 1;
            }
            else
            {
                // Console.WriteLine("XDO version 3.0.0.2");
                br.BaseStream.Position -= 4;
                this.XDOVersion = 2;
                for (int i = 0; i < this.faceNum; i++)
                {
                    this.mesh.Add(new XDOMesh(br));
                }
            }

            br.Close();
        }
    }
    class XDOMesh
    {
        public List<Vector3> list_vertex = new List<Vector3>();
        public List<Vector3> list_normal = new List<Vector3>();
        public List<Vector2> list_texture = new List<Vector2>();

        public uint vertexCount;
        public uint indexedCount;
        Color32 Color;
        public byte ImageLevel;
        public byte ImageNameLen;
        public String imageName;

        public List<ushort> indexed = new List<ushort>();

        public XDOMesh(BinaryReader br)
        {
            // vertexCount
            this.vertexCount = br.ReadUInt32();
            // S3DVertex
            for (int i = 0; i < vertexCount; i++)
            {
                list_vertex.Add(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                list_normal.Add(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                list_texture.Add(new Vector2(br.ReadSingle(), br.ReadSingle()));
            }
            // indexedCount
            this.indexedCount = br.ReadUInt32();
            for(int i = 0; i < indexedCount; i++)
            {
                indexed.Add(br.ReadUInt16());
            }
            // Color
            var readColor = br.ReadUInt32();
            byte A = (byte)((readColor >> 24) & 0xFF);
            byte R = (byte)((readColor >> 16) & 0xFF);
            byte G = (byte)((readColor >> 8) & 0xFF);
            byte B = (byte)((readColor) & 0xFF);
            this.Color = new Color32(R, G, B, A);
            // ImageLevel
            this.ImageLevel = br.ReadByte();
            // ImageNameLen
            this.ImageNameLen = br.ReadByte();
            // ImageName
            if(this.ImageNameLen > 0)
            {
                this.imageName = Encoding.UTF8.GetString(br.ReadBytes(this.ImageNameLen));
                // for what?
                var nailLen = br.ReadUInt32();
                var image = br.ReadBytes((int)nailLen);
            }
            else
            {
                this.imageName = null;
            }
        }
    }
}
