using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace XDOErrorDetectorUI
{
    class WriteXDO
    {
        public WriteXDO(ReadXDO xdo, string option, string version)
        {
            BinaryWriter bw = null;
            if (option.Equals("backup"))
            {
                try
                {
                    File.Move(xdo.url, xdo.url + "." + option);
                }
                catch (Exception e)
                {

                }
            }
            bw = new BinaryWriter(File.Open(xdo.url, FileMode.Create));
            bw.Write(xdo.XDOType);
            bw.Write(xdo.ObjectID);
            bw.Write(xdo.KeyLen);
            if(xdo.KeyLen > 0) bw.Write(Encoding.UTF8.GetBytes(xdo.Key));
            bw.Write(xdo.minX);
            bw.Write(xdo.minY);
            bw.Write(xdo.minZ);
            bw.Write(xdo.maxX);
            bw.Write(xdo.maxY);
            bw.Write(xdo.maxZ);
            bw.Write(xdo.altitude);

            if(version == null) {
                if(xdo.XDOVersion == 1) // xdo version 3.0.0.1
                {
                    bw.Write(xdo.mesh[0].vertexCount);
                    for (int j = 0; j < xdo.mesh[0].vertexCount; j++)
                    {
                        bw.Write(xdo.mesh[0].list_vertex[j].x);
                        bw.Write(xdo.mesh[0].list_vertex[j].y);
                        bw.Write(xdo.mesh[0].list_vertex[j].z);

                        bw.Write(xdo.mesh[0].list_normal[j].x);
                        bw.Write(xdo.mesh[0].list_normal[j].y);
                        bw.Write(xdo.mesh[0].list_normal[j].z);

                        bw.Write(xdo.mesh[0].list_texture[j].x);
                        bw.Write(xdo.mesh[0].list_texture[j].y);
                    }
                    bw.Write(xdo.mesh[0].indexedCount);
                    for (int j = 0; j < xdo.mesh[0].indexedCount; j++)
                    {
                        bw.Write(xdo.mesh[0].list_indice[j]);
                    }
                    bw.Write(xdo.mesh[0].Color.r);
                    bw.Write(xdo.mesh[0].Color.g);
                    bw.Write(xdo.mesh[0].Color.b);
                    bw.Write(xdo.mesh[0].Color.a);
                    bw.Write(xdo.mesh[0].ImageLevel);
                    bw.Write(xdo.mesh[0].ImageNameLen);
                    if (xdo.mesh[0].ImageNameLen > 0) {
                        bw.Write(Encoding.UTF8.GetBytes(xdo.mesh[0].imageName));
                        bw.Write(xdo.mesh[0].nailLen);
                        bw.Write(xdo.mesh[0].nailImage);
                    }
                }
                else if (xdo.XDOVersion == 2)
                {
                    bw.Write((byte)xdo.mesh.Count);
                    for(int i = 0; i < xdo.mesh.Count; i++)
                    {
                        bw.Write(xdo.mesh[i].vertexCount);
                        for (int j = 0; j < xdo.mesh[i].vertexCount; j++)
                        {
                            bw.Write(xdo.mesh[i].list_vertex[j].x);
                            bw.Write(xdo.mesh[i].list_vertex[j].y);
                            bw.Write(xdo.mesh[i].list_vertex[j].z);

                            bw.Write(xdo.mesh[i].list_normal[j].x);
                            bw.Write(xdo.mesh[i].list_normal[j].y);
                            bw.Write(xdo.mesh[i].list_normal[j].z);

                            bw.Write(xdo.mesh[i].list_texture[j].x);
                            bw.Write(xdo.mesh[i].list_texture[j].y);
                        }
                        bw.Write(xdo.mesh[i].indexedCount);
                        for(int j = 0; j < xdo.mesh[i].indexedCount; j++)
                        {
                            bw.Write(xdo.mesh[i].list_indice[j]);
                        }
                        bw.Write(xdo.mesh[i].Color.r);
                        bw.Write(xdo.mesh[i].Color.g);
                        bw.Write(xdo.mesh[i].Color.b);
                        bw.Write(xdo.mesh[i].Color.a);
                        bw.Write(xdo.mesh[i].ImageLevel);
                        bw.Write(xdo.mesh[i].ImageNameLen);
                        if (xdo.mesh[i].ImageNameLen > 0)
                        {
                            bw.Write(Encoding.UTF8.GetBytes(xdo.mesh[i].imageName));
                            bw.Write(xdo.mesh[i].nailLen);
                            bw.Write(xdo.mesh[i].nailImage);
                        }
                    }
                }
            }
            else if(version == "3001")
            {
                bw.Write(xdo.mesh[0].vertexCount);
                for (int j = 0; j < xdo.mesh[0].vertexCount; j++)
                {
                    bw.Write(xdo.mesh[0].list_vertex[j].x);
                    bw.Write(xdo.mesh[0].list_vertex[j].y);
                    bw.Write(xdo.mesh[0].list_vertex[j].z);

                    bw.Write(xdo.mesh[0].list_normal[j].x);
                    bw.Write(xdo.mesh[0].list_normal[j].y);
                    bw.Write(xdo.mesh[0].list_normal[j].z);

                    bw.Write(xdo.mesh[0].list_texture[j].x);
                    bw.Write(xdo.mesh[0].list_texture[j].y);
                }
                bw.Write(xdo.mesh[0].indexedCount);
                for (int j = 0; j < xdo.mesh[0].indexedCount; j++)
                {
                    bw.Write(xdo.mesh[0].list_indice[j]);
                }
                bw.Write(xdo.mesh[0].Color.r);
                bw.Write(xdo.mesh[0].Color.g);
                bw.Write(xdo.mesh[0].Color.b);
                bw.Write(xdo.mesh[0].Color.a);
                bw.Write(xdo.mesh[0].ImageLevel);
                bw.Write(xdo.mesh[0].ImageNameLen);
                if (xdo.mesh[0].ImageNameLen > 0)
                {
                    bw.Write(Encoding.UTF8.GetBytes(xdo.mesh[0].imageName));
                    bw.Write(xdo.mesh[0].nailLen);
                    bw.Write(xdo.mesh[0].nailImage);
                }
            }
            else if(version == "3002")
            {
                bw.Write((byte)1);
                bw.Write(xdo.mesh[0].vertexCount);
                for (int j = 0; j < xdo.mesh[0].vertexCount; j++)
                {
                    bw.Write(xdo.mesh[0].list_vertex[j].x);
                    bw.Write(xdo.mesh[0].list_vertex[j].y);
                    bw.Write(xdo.mesh[0].list_vertex[j].z);

                    bw.Write(xdo.mesh[0].list_normal[j].x);
                    bw.Write(xdo.mesh[0].list_normal[j].y);
                    bw.Write(xdo.mesh[0].list_normal[j].z);

                    bw.Write(xdo.mesh[0].list_texture[j].x);
                    bw.Write(xdo.mesh[0].list_texture[j].y);
                }
                bw.Write(xdo.mesh[0].indexedCount);
                for (int j = 0; j < xdo.mesh[0].indexedCount; j++)
                {
                    bw.Write(xdo.mesh[0].list_indice[j]);
                }
                bw.Write(xdo.mesh[0].Color.r);
                bw.Write(xdo.mesh[0].Color.g);
                bw.Write(xdo.mesh[0].Color.b);
                bw.Write(xdo.mesh[0].Color.a);
                bw.Write(xdo.mesh[0].ImageLevel);
                bw.Write(xdo.mesh[0].ImageNameLen);
                if (xdo.mesh[0].ImageNameLen > 0)
                {
                    bw.Write(Encoding.UTF8.GetBytes(xdo.mesh[0].imageName));
                    bw.Write(xdo.mesh[0].nailLen);
                    bw.Write(xdo.mesh[0].nailImage);
                }
            }

            bw.Close();
        }
    }
}
