using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Npgsql;
using NpgsqlTypes;

namespace XDOErrorDetectorUI
{
    class postgreSQL
    {
        //public static String baseURL = @"C:\APM_Setup\htdocs\etri\js\test";
        public DB info;
        public String baseURL = @"C:\Users\KimDoHoon\Desktop\C++_Project\data";
        public Dictionary<String, DBItem> check()
        {
            Dictionary<String, DBItem> dic = new Dictionary<string, DBItem>();

            /*
            var info = new DB();
            using (var conn = new NpgsqlConnection("Host=" + info.Host + ";Username=" + info.Username + ";Password=" + info.Password + ";Database=" + info.Database))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "select * from " + info.Table;
                        
                        using(var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DBItem item = new DBItem();
                                item.fileName = reader["name"].ToString();
                                item.status_error = int.Parse(reader["imageError"].ToString());
                                item.status_correct = int.Parse(reader["imageSuccess"].ToString());
                                item.status_warning = int.Parse(reader["imageWarning"].ToString());

                                string header = reader["name"].ToString().Replace(baseURL + @"\", "");
                                dic.Add(header, item);
                            }
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            */

            return dic;
        }
        public string search(string table, string path)
        {
            // XDO 파일을 주어진 경로로 부터 모두 다 찾음
            var xdoFileReader = new xdoFileFinder(path);
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

            using(var conn = connection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;

                        cmd.CommandText = "INSERT INTO " + table + " (\"Level\", \"X\", \"Y\", \"fileName\", \"ObjectID\", \"Key\", \"ObjBox_minX\", \"ObjBox_minY\", \"ObjBox_minZ\", \"ObjBox_maxX\", \"ObjBox_maxY\", \"ObjBox_maxZ\", \"Altitude\", \"FaceNum\", \"XDOVersion\", \"VertexCount\", \"IndexedCount\", \"ImageLevel\", \"ImageName\") " +
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

                        foreach (KeyValuePair<String, DBItem> key in hashMap)
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
                            for (int i = 0; i < cmd.Parameters.Count; i++)
                                cmd.Parameters[i].Value = obj_container[i];
                            cmd.ExecuteNonQuery();
                        }
                        return "데이터 " + hashMap.Count + "개가 " + table + "에 삽입되었습니다.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return "검색 실패";
                }
            }
        }
        public void update()
        {
            /*
            // Search xdo file from baseURL
            var xdoFileReader = new xdoFileFinder(baseURL);
            List<String> xdoFileList = xdoFileReader.run();
            Dictionary<String, DBItem> hashMap = new Dictionary<String, DBItem>();
            // XDO(v3.0.0.2) Read
            foreach (String xdoFile in xdoFileList)
            {
                XDO xdo = new XDO(xdoFile);
                String fullURL = xdo.url;
                String baseDirectory = new FileInfo(fullURL).Directory.FullName;

                List<String> imageFileList = new List<String>();

                foreach (String file in Directory.GetFiles(baseDirectory)) imageFileList.Add(file);
                String fileName = Path.GetFileName(fullURL);

                // hashMap will be pushed
                hashMap.Add(xdoFile, new DBItem(xdoFile));

                foreach (XDOMesh mesh in xdo.mesh)
                {
                    String imageName = mesh.imageName;
                    int status = 0;
                    foreach (String fullPath in imageFileList)
                    {
                        if (fullPath.Equals(baseDirectory + "\\" + imageName))
                        {
                            status = 1;
                            break;
                        }
                        else if (fullPath.ToLower().Equals((baseDirectory + "\\" + imageName).ToLower()))
                        {
                            status = 2;
                            break;
                        }
                    }

                    switch (status)
                    {
                        case 0:
                            hashMap[xdoFile].status_error++;
                            Console.WriteLine(baseDirectory + "\\" + imageName + ": image is not exist");
                            break;
                        case 1:
                            hashMap[xdoFile].status_correct++;
                            Console.WriteLine(baseDirectory + "\\" + imageName + ": image is exist");
                            break;
                        case 2:
                            hashMap[xdoFile].status_warning++;
                            Console.WriteLine(baseDirectory + "\\" + imageName + ": image is exist but there is a warning about UPPER/LOWER case");
                            break;
                    }

                }

            

            }
            */

            // DB connect & write
            /*
            var info = new DB();
            using (var conn = new NpgsqlConnection("Host=" + info.Host + ";Username=" + info.Username + ";Password=" + info.Password + ";Database=" + info.Database))
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
                    foreach (KeyValuePair<String, DBItem> key in hashMap)
                    {
                        using (var cmd = new NpgsqlCommand())
                        {
                            // Console.WriteLine(key.Key + "\n(" + key.Value.status_correct + ", " + key.Value.status_warning + ", " + key.Value.status_error + ")");

                            cmd.Connection = conn;

                            cmd.CommandText = "insert into " + info.Table + "(\"name\",\"imageError\",\"imageSuccess\",\"imageWarning\") values(@name, @error, @success, @warning)";
                            cmd.Parameters.AddWithValue("name", key.Key);
                            cmd.Parameters.AddWithValue("error", key.Value.status_error);
                            cmd.Parameters.AddWithValue("success", key.Value.status_correct);
                            cmd.Parameters.AddWithValue("warning", key.Value.status_warning);
                            Console.WriteLine(key.Key + "/" + key.Value.status_error + "/" + key.Value.status_correct + "/" + key.Value.status_warning);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            */
        }

        public string createTable(string tablename)
        {
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "CREATE TABLE public." + tablename + "(" +
                            "\"Level\" integer," +
                            "\"X\" text," +
                            "\"Y\" text," +
                            "\"fileName\" text," +
                            "\"ObjectID\" integer," +
                            "\"Key\" text," +
                            "\"ObjBox_minX\" double precision," +
                            "\"ObjBox_minY\" double precision," +
                            "\"ObjBox_minZ\" double precision," +
                            "\"ObjBox_maxX\" double precision," +
                            "\"ObjBox_maxY\" double precision," +
                            "\"ObjBox_maxZ\" double precision," +
                            "\"Altitude\" real," +
                            "\"FaceNum\" integer," +
                            "\"XDOVersion\" integer," +
                            "\"VertexCount\" integer[]," +
                            "\"IndexedCount\" integer[]," +
                            "\"ImageLevel\" integer[]," +
                            "\"ImageName\" text[]" +
                            ")";
                        cmd.ExecuteNonQuery();
                        return "Table이 성공적으로 생성되었습니다.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return "Table을 생성시키지 못했습니다.";
                }
            }
        }
        public NpgsqlConnection connection()
        {
            return new NpgsqlConnection("Host=" + info.host + ";Username=" + info.username + ";Password=" + info.password + ";Database=" + info.database);
        }
        public string clearTable(string tablename)
        {
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "delete from " + tablename;
                        cmd.ExecuteNonQuery();
                        return "Table을 성공적으로 초기화하였습니다.";
                    }
                }
                catch (Exception e)
                {
                    return "초기화 실패";
                }
            }
        }
        public string deleteTable(string tablename)
        {
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "drop table " + tablename;
                        cmd.ExecuteNonQuery();
                        return "Table을 성공적으로 삭제하였습니다";
                    }
                }
                catch (Exception e)
                {
                    return "삭제 실패";
                }
            }
        }
        public string connect()
        {
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    return "성공적으로 연결되었습니다.";
                }
                catch (Exception e)
                {
                    return "DB 접속에 실패하였습니다.";
                }
            }
        }
    }
    class DB
    {
        private string Host;
        private string Username;
        private string Password;
        private string Database;
        private string Table;

        public string host
        {
            get
            {
                return this.Host;
            }
            set
            {
                this.Host = value;
            }
        }
        public string username
        {
            get
            {
                return this.Username;
            }
            set
            {
                this.Username = value;
            }
        }
        public string password
        {
            get
            {
                return this.Password;
            }
            set
            {
                this.Password = value;
            }
        }
        public string database
        {
            get
            {
                return this.Database;
            }
            set
            {
                this.Database = value;
            }
        }
        public string table
        {
            get
            {
                return this.Table;
            }
            set
            {
                this.Table = value;
            }
        }
        public DB(string h, string u, string p, string d)
        {
            this.host = h;
            this.username = u;
            this.password = p;
            this.database = d;
        }
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
