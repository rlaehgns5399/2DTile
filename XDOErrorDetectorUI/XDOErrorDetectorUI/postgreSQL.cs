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
        public DB info;

        public string search(string table, string path)
        {
            // XDO 파일을 주어진 경로로 부터 모두 다 찾음
            var xdoFileReader = new xdoFileFinder(path);
            var xdoFileList = xdoFileReader.run();

            // DB에 저장할 hashMap 구성
            var hashMap = new Dictionary<string, DBItem>();
            var imageSet = new HashSet<string>();

            // XDO(v3.0.0.2) Read -> 이미지 집합 및 1 DBItem row 생성
            // 나중에 안쓰이는 그림 파일 찾을 때 쓰임
            // 비효율적으로 제작
            foreach (var xdoFile in xdoFileList)
            {
                var xdo = new XDO(xdoFile);
                var baseDirectory = new FileInfo(xdo.url).Directory.FullName;

                var xdo_dbItem = new DBItem();


                var xy = new FileInfo(xdoFile).Directory.Name;
                var name = xy.Split('_');
                xdo_dbItem.level = Int32.Parse(new FileInfo(xdoFile).Directory.Parent.Parent.Name);
                xdo_dbItem.X = name[0];
                xdo_dbItem.Y = name[1];
                xdo_dbItem.minifiedName = new FileInfo(xdoFile).Name;
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
            writeDBwithXDOLog(table, hashMap, imageSet);
            return writeDBwithXDOInfo(table, hashMap);
        }
        public void writeDBwithXDOLog(string table, Dictionary<string, DBItem> hashMap, HashSet<string> imageSet)
        {
            foreach(KeyValuePair<string, DBItem> key in hashMap)
            {
                var imageSet_forRemove = new HashSet<string>();
                var baseURL = new FileInfo(key.Key).Directory.FullName;
                for (var i = 0; i < key.Value.FaceNum; i++)
                {
                    var imgURL = baseURL + "\\" + key.Value.ImageName[i];
                    var count = 0;

                    int imageNum = key.Value.ImageLevel[i];

                    if (key.Value.ImageLevel[i] == 1)
                    {
                        // 기본 텍스쳐도 없는 경우는 1
                    }
                    else if(key.Value.ImageLevel[i] == 2)
                    {
                        // 기본 텍스쳐만 있는경우(레벨이 없는 이미지)
                    }
                    else if(key.Value.ImageLevel[i] > 2)
                    {
                        // 여러 레벨이 있는 텍스쳐라면 2를 빼줌
                        imageNum -= 2;
                    }
                    
                    for (var j = 1; j <= imageNum; j++)
                    {
                        var imgLodUrl = baseURL + "\\" + key.Value.ImageName[i].Replace(".", "_" + j + ".");
                        foreach (string imageSetElement in imageSet)
                        {
                            if (imgLodUrl.Equals(imageSetElement))
                            {
                                // 성공
                                // xdo level 체크를 위해 수를 세어야함.
                                imageSet_forRemove.Add(imageSetElement);
                                count++;
                                break;
                            }
                            else if (imgLodUrl.ToLower().Equals(imageSetElement.ToLower()))
                            {
                                // lower/upper Case 오류
                                // xdo level 체크를 위해 수를 세어야함
                                //Console.WriteLine("[Case] " + imgLodUrl);
                                imageSet_forRemove.Add(imageSetElement);
                                count++;
                                break;
                            }
                        }
                    }

                    //if (count != key.Value.ImageLevel[i])
                    //{
                        // if(갯수가 차이가 나면) level error
                        // Console.WriteLine(key.Key + ": " + key.Value.ImageName[i]);
                        // Console.WriteLine("조사된 갯수: " + count + "/" + imageNum);
                    //}
                }
                foreach(string t in imageSet_forRemove) {
                    imageSet.Remove(t);
                    // Console.WriteLine("[O] " + t);
                }
            }
            foreach(string remainImage in imageSet)
            {
                // Console.WriteLine("[X] " + remainImage);
                // 쓰이지 않는 것들 여기서 처리 
            }
        }
        public string writeDBwithXDOInfo(string table, Dictionary<string, DBItem> hashMap)
        {
            using (var conn = connection())
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
                            Object[] obj_container = {
                                key.Value.level,
                                key.Value.X,
                                key.Value.Y,
                                key.Value.minifiedName,
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

        public List<DBItem> loadTable(string table)
        {
            var list = new List<DBItem>();
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "select * from " + table;
                        
                        using(var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DBItem item = new DBItem();
                                item.level = int.Parse(reader["level"].ToString());
                                item.X = reader["X"].ToString();
                                item.Y = reader["Y"].ToString();
                                item.minifiedName = reader["fileName"].ToString();
                                item.ObjectID = int.Parse(reader["ObjectID"].ToString());
                                item.Key = reader["Key"].ToString();
                                item.ObjBox = new double[6] {
                                    double.Parse(reader["ObjBox_minX"].ToString()),
                                    double.Parse(reader["ObjBox_minY"].ToString()),
                                    double.Parse(reader["ObjBox_minZ"].ToString()),
                                    double.Parse(reader["ObjBox_maxX"].ToString()),
                                    double.Parse(reader["ObjBox_maxY"].ToString()),
                                    double.Parse(reader["ObjBox_maxZ"].ToString())
                                };
                                item.Altitude = float.Parse(reader["Altitude"].ToString());
                                item.FaceNum = int.Parse(reader["FaceNum"].ToString());
                                item.XDOVersion = int.Parse(reader["XDOVersion"].ToString());
                                item.VertexCount = ((int[])reader["VertexCount"]).ToList();
                                item.IndexedCount = ((int[])reader["IndexedCount"]).ToList();
                                item.ImageLevel = ((int[])reader["ImageLevel"]).ToList();
                                item.ImageName = ((string[])reader["ImageName"]).ToList();
                                list.Add(item);
                            }
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return list;
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
            return new NpgsqlConnection("Host=" + info.Host + ";Username=" + info.Username + ";Password=" + info.Password + ";Database=" + info.Database);
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
                    return "DB 접속 실패";
                }
            }
        }
    }
    class DB
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string Table { get; set; }
        
        public DB(string h, string u, string p, string d)
        {
            this.Host = h;
            this.Username = u;
            this.Password = p;
            this.Database = d;
        }
    }

    class DBItem
    {
        public string X;
        public string Y;
        public int level;
        public String fileName;
        public String minifiedName;
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
