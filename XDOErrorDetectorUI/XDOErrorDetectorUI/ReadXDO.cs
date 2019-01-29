using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;

namespace XDOErrorDetectorUI
{
    public class ReadXDO
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
        public bool isEnd;
        public ReadXDO(string url)
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
                this.faceNum = 1;
            }

            if (temp == 0)
            {
                this.XDOVersion = 1;
                br.BaseStream.Position -= 5;
                this.mesh.Add(new XDOMesh(br));
            }
            else
            {
                this.XDOVersion = 2;
                br.BaseStream.Position -= 4;
                for (int i = 0; i < this.faceNum; i++)
                {
                    this.mesh.Add(new XDOMesh(br));
                }
            }

            br.Close();
        }
        public ReadXDO(string url, int ver)
        {
            this.url = url;
            BinaryReader br = new BinaryReader(File.Open(this.url, FileMode.Open));
            try
            {
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

                if (ver == 3001)
                {
                    this.mesh.Add(new XDOMesh(br));
                }

                if (ver == 3002)
                {
                    this.faceNum = br.ReadByte();
                    for (int i = 0; i < this.faceNum; i++)
                    {
                        this.mesh.Add(new XDOMesh(br));
                    }
                }

                this.isEnd = br.BaseStream.Length == br.BaseStream.Position;
            }
            catch (Exception e)
            {
                this.isEnd = false;
            }
            br.Close();
        }
    }
    class XDOMesh
    {
        public List<Vector3> list_vertex = new List<Vector3>();
        public List<Vector3> list_normal = new List<Vector3>();
        public List<Vector3> list_normal_modifed = new List<Vector3>();
        public List<Vector2> list_texture = new List<Vector2>();
        public List<ushort> list_indice = new List<ushort>();

        public float vertex_minX, vertex_minY, vertex_minZ, vertex_maxX, vertex_maxY, vertex_maxZ;
        public float normal_minX, normal_minY, normal_minZ, normal_maxX, normal_maxY, normal_maxZ;
        public float texture_minU, texture_minV, texture_maxU, texture_maxV;
        public ushort indice_min, indice_max;

        public uint vertexCount;
        public uint indexedCount;
        public Color32 Color;
        public byte ImageLevel;
        public byte ImageNameLen;
        public String imageName;
        
        public XDOMesh(BinaryReader br)
        {
            // vertexCount
            this.vertexCount = br.ReadUInt32();
            // S3DVertex
            for (int i = 0; i < vertexCount; i++)
            {
                list_vertex.Add(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));

                var nx = br.ReadSingle();
                var ny = br.ReadSingle();
                var nz = br.ReadSingle();

                list_normal.Add(new Vector3(nx, ny, nz));

                if (nx == 0.0f && ny == 0.0f && nz == 0.0f)
                    nx = ny = nz = 0.0f;

                var n = Math.Abs((float)Math.Sqrt(nx * nx + ny * ny + nz * nz));

                nx /= n;
                ny /= n;
                nz /= n;

                list_normal_modifed.Add(new Vector3(nx, ny, nz));

                list_texture.Add(new Vector2(br.ReadSingle(), br.ReadSingle()));
            }
            // getting Min&Max XYZ | UV
            vertex_minX = list_vertex.Select(x => x[0]).Min();
            vertex_maxX = list_vertex.Select(x => x[0]).Max();
            vertex_minY = list_vertex.Select(x => x[1]).Min();
            vertex_maxY = list_vertex.Select(x => x[1]).Max();
            vertex_minZ = list_vertex.Select(x => x[2]).Min();
            vertex_maxZ = list_vertex.Select(x => x[2]).Max();

            normal_minX = list_normal_modifed.Select(x => x[0]).Min();
            normal_maxX = list_normal_modifed.Select(x => x[0]).Max();
            normal_minY = list_normal_modifed.Select(x => x[1]).Min();
            normal_maxY = list_normal_modifed.Select(x => x[1]).Max();
            normal_minZ = list_normal_modifed.Select(x => x[2]).Min();
            normal_maxZ = list_normal_modifed.Select(x => x[2]).Max();

            texture_minU = list_texture.Select(x => x[0]).Min();
            texture_maxU = list_texture.Select(x => x[0]).Max();
            texture_minV = list_texture.Select(x => x[1]).Min();
            texture_maxV = list_texture.Select(x => x[1]).Max();

            // indexedCount
            this.indexedCount = br.ReadUInt32();
            for (int i = 0; i < indexedCount; i++)
            {
                list_indice.Add(br.ReadUInt16());
            }

            // getting Min&Max indice
            indice_min = list_indice.Min();
            indice_max = list_indice.Max();

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
            if (this.ImageNameLen > 0)
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
