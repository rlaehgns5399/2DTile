using System;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace XDOErrorDetectorUI
{
    public class GLTF
    {
        public JObject container;
        public ReadXDO xdo;
        public GLTF(ReadXDO xdo, String file_name, String file_path)
        {
            this.xdo = xdo;
            if (xdo.faceNum == 0) xdo.faceNum = 1;

            // Skeleton
            this.container = JObject.Parse(@"{
             }");

            JArray nodes = new JArray();
            JArray meshes = new JArray();
            JArray materials = new JArray();
            JArray textures = new JArray();
            JArray images = new JArray();
            JArray accessor = new JArray();
            JArray bufferViewElements = new JArray();
            JArray buffersElements = new JArray();
            JArray faceElements = new JArray();

            bool debug = false;

            for (int i = 0; i < xdo.faceNum; i++)
            {
                // make bin file
                FileStream fs = new FileStream(Path.Combine(file_path, file_name) + "_" + i + ".bin", FileMode.Create);
                BinaryWriter w = new BinaryWriter(fs);

                // bin - first step -> INDICES
                foreach (var t in xdo.mesh[i].list_indice)
                {
                    w.Write(t);
                }

                // 4 bytes padding
                int byte4_align_index = 0;
                if ((xdo.mesh[i].list_indice.Count * 2) % 4 != 0)
                {
                    byte4_align_index = (xdo.mesh[i].list_indice.Count * 2) % 4;
                    // 4 byte padding, uv list can be multiple of 2 not 4.
                    for (int padding_iterator = 0; padding_iterator < byte4_align_index; padding_iterator++) w.Write((byte)0);

                    if (debug) Console.WriteLine("IndexList: " + xdo.mesh[i].list_indice.Count * 2 + " + " + byte4_align_index + "bytes. aligned 4 bytes(" + (xdo.mesh[i].list_indice.Count * 2 + byte4_align_index) + ")");
                }

                // bin - second step -> Vertex(Position)
                foreach (var t in xdo.mesh[i].list_vertex)
                {
                    w.Write(t.x);
                    w.Write(t.y);
                    w.Write(t.z);
                }

                // bin - third step - Normals
                foreach (var t in xdo.mesh[i].list_normal_modifed)
                {

                    w.Write(t.x);
                    w.Write(t.y);
                    w.Write(t.z);
                }

                // bom - fourth step - Texture UV
                foreach (var t in xdo.mesh[i].list_texture)
                {
                    w.Write(t.x);
                    w.Write(t.y);
                }
                w.Close();

                // make gltf with xdo informations

                // define 'strict' asset

                faceElements.Add(JToken.FromObject(new
                {
                    Color = xdo.mesh[i].Color,
                    ImageLevel = xdo.mesh[i].ImageLevel
                }));


                // define [scenes] <= faceNum
                JArray scenes_nodes = new JArray();

                int[] node_list = new int[xdo.faceNum];
                for (int j = 0; j < xdo.faceNum; j++)
                {
                    node_list[j] = j;
                }

                JObject scenesToken = JObject.FromObject(new
                {
                    name = "Scenes",
                    nodes = node_list
                });

                scenes_nodes.Add(scenesToken);
                container["scenes"] = scenes_nodes; // strict
                container["scene"] = 0;             // always 0?



                // define Nodes
                // JArray nodes = new JArray();
                JObject aNode = JObject.FromObject(new
                {
                    mesh = i,
                    name = xdo.Key + "_Node" + i
                    // i dont implement Translatio matrix. implement in GLBSingleMesh.cs with bbox;

                    // matrix[4x4], translation[3], rotation[4], scale[3]
                });
                nodes.Add(aNode);

                // define meshes
                // JArray meshes = new JArray();

                JObject meshToken = JObject.FromObject(new
                {
                    name = "mesh_" + i + "_" + xdo.Key
                });

                JArray primitives = new JArray();

                JObject primitives_attr = JObject.FromObject(new
                {
                    POSITION = 1 + i * 4,
                    NORMAL = 2 + i * 4,
                    TEXCOORD_0 = 3 + i * 4
                });

                JToken indices_material = JToken.FromObject(new
                {
                    attributes = primitives_attr,
                    indices = 0 + i * 4,
                    material = i
                });
                primitives.Add(indices_material);
                meshToken["primitives"] = primitives;
                meshes.Add(meshToken);


                // define material
                // JArray materials = new JArray();
                float[] emissiveFactor = { 0.0f, 0.0f, 0.0f };
                var color = xdo.mesh[i].Color;
                float[] baseColorFactor = { color.r / 255.0f , color.g / 255.0f, color.b / 255.0f, color.a / 255.0f };
                JToken index = JToken.FromObject(new
                {
                    index = i,
                    texCoord = 0 // temporary
                });
                JToken baseColorTexture = JToken.FromObject(new
                {
                    baseColorTexture = index,
                    metallicFactor = 0.0,
                    roughnessFactor = 1.0,
                    baseColorFactor = baseColorFactor
                });
                JToken materialsElement = JToken.FromObject(new
                {
                    // emissiveFactor = emissiveFactor,
                    pbrMetallicRoughness = baseColorTexture
                });
                materials.Add(materialsElement);



                // define texture
                // JArray textures = new JArray();
                JToken textureElement = JToken.FromObject(new
                {
                    sampler = 0,
                    source = i
                });
                textures.Add(textureElement);


                // define samplers
                JArray samplers = new JArray();
                JToken samplerToken = JToken.FromObject(new
                {

                });
                samplers.Add(samplerToken);
                container["samplers"] = samplers;       // always empty

                // define images
                // JArray images = new JArray();
                JToken imageToken = JToken.FromObject(new
                {
                    uri = xdo.mesh[i].imageName
                });
                images.Add(imageToken);


                // define accessors
                // 0+4*i = index(indices)
                // 1+4*i = vertex(position)
                // 2+4*i = normal
                // 3+4*i = texture

                // JArray accessor = new JArray();

                ushort[] index_min = { xdo.mesh[i].indice_min };
                ushort[] index_max = { xdo.mesh[i].indice_max };


                /*  ComponentType           Type            Num of components
                 *  BYTE    1   5120        "SCALAR"        1
                 *  U_BYTE  1   5121        "VEC2"          2
                 *  SHORT   2   5122        "VEC3"          3
                 *  USHORT  2   5123        "VEC4"          4
                 *  UINT    4   5125        "MAT2"          4
                 *  FLOAT   4   5126        "MAT3"          9    
                 *                          "MAT4"          16
                 */
                JObject indexToken = JObject.FromObject(new
                {
                    bufferView = 0 + i * 4,
                    name = xdo.Key + "_indexes",
                    componentType = 5123,                   // WebGLConstants.USHORT = 5123
                    count = xdo.mesh[i].list_indice.Count,
                    min = index_min,
                    max = index_max,
                    type = "SCALAR"
                });

                float[] vertex_min = new float[3];
                float[] vertex_max = new float[3];

                vertex_min[0] = xdo.mesh[i].vertex_minX;
                vertex_min[1] = xdo.mesh[i].vertex_minY;
                vertex_min[2] = xdo.mesh[i].vertex_minZ;
                vertex_max[0] = xdo.mesh[i].vertex_maxX;
                vertex_max[1] = xdo.mesh[i].vertex_maxY;
                vertex_max[2] = xdo.mesh[i].vertex_maxZ;

                JObject vertexToken = JObject.FromObject(new
                {
                    bufferView = 1 + i * 4,
                    name = xdo.Key + "_positions",
                    componentType = 5126,                   // WebGLConstants.FLOAT = 5126
                    count = xdo.mesh[i].list_vertex.Count,
                    min = vertex_min,
                    max = vertex_max,
                    type = "VEC3"
                });

                float[] normal_min = { xdo.mesh[i].normal_minX, xdo.mesh[i].normal_minY, xdo.mesh[i].normal_minZ };
                float[] normal_max = { xdo.mesh[i].normal_maxX, xdo.mesh[i].normal_maxY, xdo.mesh[i].normal_maxZ };

                JObject normalToken = JObject.FromObject(new
                {
                    bufferView = 2 + i * 4,
                    name = xdo.Key + "_normals",
                    componentType = 5126,                   // WebGLConstants.FLOAT = 5126
                    count = xdo.mesh[i].list_normal_modifed.Count,
                    min = normal_min,
                    max = normal_max,
                    type = "VEC3"

                });

                float[] texture_min = { xdo.mesh[i].texture_minU, xdo.mesh[i].texture_minV };
                float[] texture_max = { xdo.mesh[i].texture_maxU, xdo.mesh[i].texture_maxV };

                JObject textureToken = JObject.FromObject(new
                {
                    bufferView = 3 + i * 4,
                    name = xdo.Key + "_textureUVs",
                    componentType = 5126,                   // WebGLConstants.FLOAT = 5126
                    count = xdo.mesh[i].list_texture.Count,
                    min = texture_min,
                    max = texture_max,
                    type = "VEC2"

                });

                accessor.Add(indexToken);
                accessor.Add(vertexToken);
                accessor.Add(normalToken);
                accessor.Add(textureToken);




                // define bufferViews
                // JArray bufferViewElements = new JArray();

                int ic = xdo.mesh[i].list_indice.Count;
                int vc = xdo.mesh[i].list_vertex.Count;
                int nc = xdo.mesh[i].list_normal_modifed.Count;
                int tc = xdo.mesh[i].list_texture.Count;
                JToken bufferViewArrayElementIndex = JToken.FromObject(new
                {
                    buffer = i,
                    byteLength = ic * 2,
                    byteOffset = 0,
                    target = 34963      // 34963 - array buffer
                });
                JToken bufferViewArrayElementVertex = JToken.FromObject(new
                {
                    buffer = i,
                    byteLength = vc * 4 * 3,
                    byteOffset = ic * 2 + byte4_align_index,
                    target = 34962      // 34962 - element buffer(VEC3, VEC2...)
                });
                JToken bufferViewArrayElementNormal = JToken.FromObject(new
                {
                    buffer = i,
                    byteLength = nc * 4 * 3,
                    byteOffset = vc * 4 * 3 + ic * 2 + byte4_align_index,
                    target = 34962      // 34962 - element buffer(VEC3, VEC2...)
                });
                JToken bufferViewArrayElementUV = JToken.FromObject(new
                {
                    buffer = i,
                    byteLength = tc * 4 * 2,
                    byteOffset = vc * 4 * 3 + ic * 2 + byte4_align_index + nc * 4 * 3,
                    target = 34962      // 34962 - element buffer(VEC3, VEC2...)
                });

                int totalbyteLength = vc * 4 * 3 + ic * 2 + byte4_align_index + nc * 4 * 3 + tc * 4 * 2;

                bufferViewElements.Add(bufferViewArrayElementIndex);
                bufferViewElements.Add(bufferViewArrayElementVertex);
                bufferViewElements.Add(bufferViewArrayElementNormal);
                bufferViewElements.Add(bufferViewArrayElementUV);


                // define buffers

                JToken buffersArrayElements = JToken.FromObject(new
                {
                    byteLength = totalbyteLength,
                    uri = file_name + "_" + i + ".bin"
                });
                buffersElements.Add(buffersArrayElements);

            }
            double[] bbox = { xdo.minX, xdo.minY, xdo.minZ, xdo.maxX, xdo.maxY, xdo.maxZ };
            JToken assets = JToken.FromObject(new
            {
                Type = xdo.XDOType,
                ObjectID = xdo.ObjectID,
                Key = xdo.Key,
                ObjBox = bbox,
                Altitude = xdo.altitude,
                FaceNum = xdo.faceNum,
                Face = faceElements
            });
            JToken assetToken = JToken.FromObject(new
            {
                generator = "ETRI XDO to glTF exporter",
                version = "2.0",
                extras = assets
            });
            container["asset"] = assetToken;        // strict


            container.Add("nodes", nodes);
            container.Add("meshes", meshes);
            container.Add("textures", textures);
            container.Add("materials", materials);
            container.Add("images", images);
            container.Add("bufferViews", bufferViewElements);
            container.Add("accessors", accessor);
            container.Add("buffers", buffersElements);
            //Console.WriteLine(container.ToString());

            StreamWriter sw = new StreamWriter(Path.Combine(file_path, file_name) + ".gltf");
            sw.Write(container.ToString());
            sw.Close();
        }
    }
}