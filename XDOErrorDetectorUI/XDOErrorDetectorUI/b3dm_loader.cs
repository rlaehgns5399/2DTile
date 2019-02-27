using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XDOErrorDetectorUI
{
    class b3dm_loader
    {
        public b3dm_loader()
        {
            this.run();
        }

        private void run()
        {
            String url = @"C:\Users\KimDoHoon\Downloads\";
            String filename = @"tile.b3dm";
            b3dm b3dm = new b3dm();
            BinaryReader br = new BinaryReader(File.Open(url + filename, FileMode.Open));

            // Read Start
            byte[] temp_byte = br.ReadBytes(4);

            b3dm.magic = System.Text.Encoding.UTF8.GetString(temp_byte, 0, temp_byte.Length);
            b3dm.version = br.ReadUInt32();
            b3dm.byteLength = br.ReadUInt32();
            b3dm.featureTableJSONByteLength = br.ReadUInt32();
            b3dm.featureTableBinaryByteLength = br.ReadUInt32();
            b3dm.batchTableJSONByteLength = br.ReadUInt32();
            b3dm.batchTableBinaryByteLength = br.ReadUInt32();
            Console.WriteLine("Magic: {0}\nVersion: {1}\nbyteLength: {2}\nfeatureTableJSONByteLength: {3}\nfeatureTableBinaryByteLength: {4}", b3dm.magic, b3dm.version, b3dm.byteLength, b3dm.featureTableJSONByteLength, b3dm.featureTableBinaryByteLength);
            Console.WriteLine("batchTableJSONByteLength: {0}\nbatchTableBinaryByteLength: {1}", b3dm.batchTableJSONByteLength, b3dm.batchTableBinaryByteLength);

            try
            {
                temp_byte = br.ReadBytes((int)b3dm.featureTableJSONByteLength);
                b3dm.featureTable.JSON = JObject.Parse(System.Text.Encoding.UTF8.GetString(temp_byte, 0, temp_byte.Length));

                temp_byte = br.ReadBytes((int)b3dm.featureTableBinaryByteLength);
                b3dm.featureTable.BinaryBody = temp_byte;

                temp_byte = br.ReadBytes((int)b3dm.batchTableJSONByteLength);
                b3dm.batchTable.JSON = JObject.Parse(System.Text.Encoding.UTF8.GetString(temp_byte, 0, temp_byte.Length));

                temp_byte = br.ReadBytes((int)b3dm.batchTableBinaryByteLength);
                b3dm.featureTable.BinaryBody = temp_byte;
            }
            catch
            {
                Console.WriteLine("! batchTableJson) 길이가 0인 배열을 파싱하려고 시도합니다.");
            }
            b3dm.featureTable.toString();
            b3dm.batchTable.toString();

            //////// binarygltf section
            Console.WriteLine("=============Binary Gltf Section===============");
            temp_byte = br.ReadBytes(4);
            b3dm.binaryGltf.magic = System.Text.Encoding.UTF8.GetString(temp_byte, 0, temp_byte.Length);
            b3dm.binaryGltf.version = br.ReadUInt32();
            b3dm.binaryGltf.length = br.ReadUInt32();
            Console.WriteLine("Magic: {0}\nVersion: {1}\nLength: {2}", b3dm.binaryGltf.magic, b3dm.binaryGltf.version, b3dm.binaryGltf.length);
            if (b3dm.binaryGltf.version == 1)
            {
                b3dm.binaryGltf.chunk_json = new Chunk();
                b3dm.binaryGltf.chunk_json.chunkLength = br.ReadUInt32();
                b3dm.binaryGltf.chunk_json.chunkType = br.ReadUInt32();
                temp_byte = br.ReadBytes((int)b3dm.binaryGltf.chunk_json.chunkLength);
                Console.WriteLine("chunkLength: {0}\nchunkType: {1}", b3dm.binaryGltf.chunk_json.chunkLength, b3dm.binaryGltf.chunk_json.chunkType);
                b3dm.binaryGltf.chunk_json.chunkJSONData = JObject.Parse(System.Text.Encoding.UTF8.GetString(temp_byte, 0, temp_byte.Length));

                int byte_ends = (int)b3dm.binaryGltf.length - ((int)b3dm.binaryGltf.chunk_json.chunkLength + 20);
                temp_byte = new Byte[byte_ends];
                int i = 0;
                while (i < byte_ends)
                {
                    temp_byte[i++] = br.ReadByte();
                }
                b3dm.binaryGltf.chunk_json.chunkBinaryData = temp_byte;
                b3dm.binaryGltf.chunk_json.toString();
                Console.WriteLine("binary data count: {0}", b3dm.binaryGltf.chunk_json.chunkBinaryData.Length);
            }
            else if (b3dm.binaryGltf.version == 2)
            {
                int byteoffset = 12;
                b3dm.binaryGltf.chunk_ver2 = new List<Chunk>();
                while (byteoffset < b3dm.binaryGltf.length)
                {
                    Chunk temp = new Chunk();
                    temp.chunkLength = br.ReadUInt32();
                    temp.chunkType = br.ReadUInt32();
                    byteoffset += 8;
                    temp_byte = br.ReadBytes((int)temp.chunkLength);
                    if (temp.chunkType == 0x4E4F53FA)
                    { // "JSON"
                        temp.chunkJSONData = JObject.Parse(System.Text.Encoding.UTF8.GetString(temp_byte, 0, temp_byte.Length));
                    }
                    else if (temp.chunkType == 0x004E4942)
                    {
                        temp.chunkBinaryData = temp_byte;
                    }
                    byteoffset += (int)temp.chunkLength;
                    b3dm.binaryGltf.chunk_ver2.Add(temp);
                }
                Console.WriteLine("chunk size: {0}\n", b3dm.binaryGltf.chunk_ver2.Count);
                for (int i = 0; i < b3dm.binaryGltf.chunk_ver2.Count; i++)
                {
                    String p = b3dm.binaryGltf.chunk_ver2.ElementAt(i).chunkJSONData != null ? b3dm.binaryGltf.chunk_ver2.ElementAt(i).chunkJSONData.ToString() : b3dm.binaryGltf.chunk_ver2.ElementAt(i).chunkBinaryData.ToString();
                    Console.WriteLine("chunk {0} - \nchunkLength: {1}\nchunkType: {2}\nchunkData: {3}", i, b3dm.binaryGltf.chunk_ver2.ElementAt(i).chunkLength, b3dm.binaryGltf.chunk_ver2.ElementAt(i).chunkType, p);
                }
            }
        }
    }

    class b3dm
    {
        internal string magic;
        internal uint version, byteLength, featureTableJSONByteLength, featureTableBinaryByteLength, batchTableJSONByteLength, batchTableBinaryByteLength;
        internal Table featureTable = new Table(), batchTable = new Table();
        internal GLB binaryGltf = new GLB();
    }
    class Table
    {
        internal JObject JSON;
        internal Byte[] BinaryBody;
        public void toString()
        {
            string p = this.JSON != null ? this.JSON.ToString() : "";
            string b = this.BinaryBody != null ? System.Text.Encoding.UTF8.GetString(BinaryBody, 0, BinaryBody.Length) : "";
            Console.WriteLine("JSON: " + p + "\nBinary: " + b);
        }
    }
    class GLB
    {
        internal string magic;  // todo make uint -> string
        internal uint version, length;
        internal Chunk chunk_json;
        internal List<Chunk> chunk_ver2;
    }
    class Chunk
    {
        internal uint chunkLength, chunkType;
        internal JObject chunkJSONData;
        internal Byte[] chunkBinaryData;
        public void toString()
        {
            string p = this.chunkJSONData != null ? this.chunkJSONData.ToString() : "";
            string b = this.chunkBinaryData != null ? System.Text.Encoding.UTF8.GetString(this.chunkBinaryData, 0, this.chunkBinaryData.Length) : "";

            Console.WriteLine("JSON: {0}", p);
            Console.WriteLine("Binary: {0}", b);
        }
    }
}

