using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Npgsql;
using NpgsqlTypes;

namespace XDOErrorDetector
{

    class Program
    {
        //public static String baseURL = @"C:\APM_Setup\htdocs\etri\js\test";
        public static String baseURL = @"C:\Users\KimDoHoon\Desktop\git\2DTile\XDOErrorDetector\data";
        enum STATUS
        {
            SUCCESS,
            ERR_NOT_EXIST_FILE,
            WARN_UPPER_LOWER_CASE,
            UNKNOWN_CODE
        }
        static STATUS getStatus(String fullPath, String path, int level = -1)
        {
            if (level == -1)
            {
                if (fullPath.Equals(path))
                {
                    return STATUS.SUCCESS;
                }
                else if (fullPath.ToLower().Equals(path.ToLower()))
                {
                    return STATUS.WARN_UPPER_LOWER_CASE;
                }
            }
            else
            {
                for (int i = 0; i < level; i++)
                {

                }
            }
            return STATUS.UNKNOWN_CODE;
        }
        static void Main(string[] args)
        {
            // XDO 파일을 주어진 경로로 부터 모두 다 찾음
            var xdoFileReader = new xdoFileFinder(baseURL);
            var xdoFileList = xdoFileReader.run();

            // DB에 저장할 hashMap 구성
            var hashMap = new Dictionary<String, DBItem>();
            var imageSet = new HashSet<String>();

            // XDO(v3.0.0.2) Read -> 이미지 집합 및 1 DBItem row 생성
            // 나중에 안쓰이는 그림 파일 찾을 때 쓰임
            // 비효율적으로 제작
            foreach (var xdoFile in xdoFileList)
            {
                var xdo = new XDO(xdoFile);
                var baseDirectory = new FileInfo(xdo.url).Directory.FullName;

                var xdo_dbItem = new DBItem();

                xdo_dbItem.XDOVersion = xdo.XDOVersion;
                xdo_dbItem.fileName = xdoFile;
                xdo_dbItem.ObjectID = (int)xdo.ObjectID;
                xdo_dbItem.Key = xdo.Key;
                xdo_dbItem.ObjBox[0] = xdo.minX;
                xdo_dbItem.ObjBox[1] = xdo.minY;
                xdo_dbItem.ObjBox[2] = xdo.minZ;
                xdo_dbItem.ObjBox[3] = xdo.maxX;
                xdo_dbItem.ObjBox[4] = xdo.maxY;
                xdo_dbItem.ObjBox[5] = xdo.maxZ;
                xdo_dbItem.Altitude = xdo.altitude;

                if (xdo.XDOVersion == 1)
                {
                    xdo_dbItem.FaceNum = 1;
                }
                else
                {
                    xdo_dbItem.FaceNum = xdo.faceNum;
                }

                for (int i = 0; i < xdo.faceNum; i++)
                {
                    xdo_dbItem.VertexCount.Add((int)xdo.mesh[i].vertexCount);
                    xdo_dbItem.IndexedCount.Add((int)xdo.mesh[i].indexedCount);
                    xdo_dbItem.ImageLevel.Add((int)xdo.mesh[i].ImageLevel);
                    xdo_dbItem.ImageName.Add(xdo.mesh[i].imageName);
                }

                hashMap.Add(xdoFile, xdo_dbItem);

                foreach (var imgFile in Directory.GetFiles(baseDirectory))
                {
                    if (imgFile.ToLower().Contains(".jpg") || imgFile.ToLower().Contains(".png"))
                        imageSet.Add(imgFile);
                }
            }

            /*
            // XDO(3.0.0.2) Read
            foreach (String xdoFile in xdoFileList)
            {
                var xdo = new XDO(xdoFile);
                var baseDirectory = new FileInfo(xdo.url).Directory.FullName;

                List<String> imageFileList = new List<String>();
                foreach (var file in Directory.GetFiles(baseDirectory))
                {
                    if (file.ToLower().Contains(".jpg") || file.ToLower().Contains(".png"))
                    {
                        imageFileList.Add(file);
                    }
                }
                String fileName = Path.GetFileName(xdo.url);

                // hashMap will be pushed
                hashMap.Add(xdoFile, new DBItem(xdoFile));

                foreach (XDOMesh mesh in xdo.mesh)
                {
                    String path = null;
                    String imageName = mesh.imageName;
                    int imageLevel = mesh.ImageLevel;
                    STATUS status = STATUS.ERR_NOT_EXIST_FILE;
                    foreach (String fullPath in imageFileList)
                    {
                        path = baseDirectory + "\\" + imageName;
                        status = getStatus(fullPath, path);
                        switch (status)
                        {
                            case STATUS.ERR_NOT_EXIST_FILE:
                                hashMap[xdoFile].status_error++;
                                Console.WriteLine(path + ": image is not exist");
                                break;
                            case STATUS.SUCCESS:
                                hashMap[xdoFile].status_correct++;
                                Console.WriteLine(path + ": image is exist");
                                break;
                            case STATUS.WARN_UPPER_LOWER_CASE:
                                hashMap[xdoFile].status_warning++;
                                Console.WriteLine(path + ": image is exist but there is a warning about UPPER/LOWER case");
                                break;
                        }

                        for (int i = 0; i < imageLevel; i++)
                        {
                            path = baseDirectory + "\\" + imageName.Replace(".", "_" + i + ".");
                            status = getStatus(fullPath, path, imageLevel);

                            switch (status)
                            {
                                case STATUS.ERR_NOT_EXIST_FILE:
                                    hashMap[xdoFile].status_error++;
                                    Console.WriteLine(path + ": image is not exist");
                                    break;
                                case STATUS.SUCCESS:
                                    hashMap[xdoFile].status_correct++;
                                    Console.WriteLine(path + ": image is exist");
                                    break;
                                case STATUS.WARN_UPPER_LOWER_CASE:
                                    hashMap[xdoFile].status_warning++;
                                    Console.WriteLine(path + ": image is exist but there is a warning about UPPER/LOWER case");
                                    break;
                            }
                        }
                    }
                }


                

            }
            */
            // DB connect & write
            var info = new DB();
            var connString = String.Format("Host={0};Username={1};password={2};database={3}", info.Host, info.Username, info.Password, info.Database);
            using (var conn = new NpgsqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "delete from " + info.Table;
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new NpgsqlCommand())
                    {
                        // Console.WriteLine(key.Key + "\n(" + key.Value.status_correct + ", " + key.Value.status_warning + ", " + key.Value.status_error + ")");

                        cmd.Connection = conn;

                        cmd.CommandText = "INSERT INTO xdo2 (\"Level\", \"X\", \"Y\", \"fileName\", \"ObjectID\", \"Key\", \"ObjBox_minX\", \"ObjBox_minY\", \"ObjBox_minZ\", \"ObjBox_maxX\", \"ObjBox_maxY\", \"ObjBox_maxZ\", \"Altitude\", \"FaceNum\", \"XDOVersion\", \"VertexCount\", \"IndexedCount\", \"ImageLevel\", \"ImageName\") " +
                            @"VALUES(@Level, @X, @Y, @fileName, @ObjectID, @Key, @ObjBox_minX, @ObjBox_minY, @ObjBox_minZ, @ObjBox_maxX, @ObjBox_maxY, @ObjBox_maxZ, @Altitude, @FaceNum, @XDOVersion, @VertexCount, @IndexedCount, @ImageLevel, @ImageName)";
                        cmd.Parameters.Add(new NpgsqlParameter("Level", NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("X", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("Y", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("fileName", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("ObjectID", NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("Key", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("ObjBox_minX", NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("ObjBox_minY", NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("ObjBox_minZ", NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("ObjBox_maxX", NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("ObjBox_maxY", NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("ObjBox_maxZ", NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("Altitude", NpgsqlDbType.Real));
                        cmd.Parameters.Add(new NpgsqlParameter("FaceNum", NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("XDOVersion", NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("VertexCount", NpgsqlDbType.Array | NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("IndexedCount", NpgsqlDbType.Array | NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("ImageLevel", NpgsqlDbType.Array | NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("ImageName", NpgsqlDbType.Array | NpgsqlDbType.Text));

                        cmd.Prepare();

                        foreach(KeyValuePair < String, DBItem > key in hashMap)
                        {
                            var xy = new FileInfo(key.Key).Directory.Name;
                            var name = xy.Split('_');
                            Object[] obj_container = {
                                Int32.Parse(new FileInfo(key.Key).Directory.Parent.Parent.Name),
                                name[0],
                                name[1],
                                new FileInfo(key.Key).Name,
                                key.Value.ObjectID,
                                key.Value.Key,
                                key.Value.ObjBox[0],
                                key.Value.ObjBox[1],
                                key.Value.ObjBox[2],
                                key.Value.ObjBox[3],
                                key.Value.ObjBox[4],
                                key.Value.ObjBox[5],
                                key.Value.Altitude,
                                key.Value.FaceNum,
                                key.Value.XDOVersion,
                                key.Value.VertexCount.ToArray(),
                                key.Value.IndexedCount.ToArray(),
                                key.Value.ImageLevel.ToArray(),
                                key.Value.ImageName.ToArray()
                            };
                            for(int i = 0; i < cmd.Parameters.Count; i++)
                                cmd.Parameters[i].Value = obj_container[i];
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            Console.ReadLine();
        }
    }

    class DB
    {
        public String Host = "localhost";
        public String Username = "postgres";
        public String Password = "root";
        public String Database = "mydata";
        public String Table = "xdo2";
    }

    class DBItem
    {
        public String fileName;
        public int ObjectID;
        public String Key;
        public double[] ObjBox = new double[6];
        public float Altitude;
        public int FaceNum;
        public int XDOVersion;
        public List<int> VertexCount = new List<int>();
        public List<int> IndexedCount = new List<int>();
        public List<int> ImageLevel = new List<int>();
        public List<string> ImageName = new List<String>();
    }
}
