using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Npgsql;

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
        public void update()
        {
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
        public int status_correct, status_warning, status_error;
        public DBItem()
        {

        }
        public DBItem(String name)
        {
            this.fileName = name;
            this.status_correct = this.status_error = this.status_warning = 0;
        }
    }
}
