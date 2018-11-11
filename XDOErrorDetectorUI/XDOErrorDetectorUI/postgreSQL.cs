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
        public String baseURL = @"C:\Users\KimDoHoon\Desktop\C++_Project\data";
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

            Console.ReadLine();
        }
    }

    class DB
    {
        public String Host = "localhost";
        public String Username = "postgres";
        public String Password = "root";
        public String Database = "mydata";
        public String Table = "xdo";
    }

    class DBItem
    {
        public String fileName;
        public int status_correct, status_warning, status_error;

        public DBItem(String name)
        {
            this.fileName = name;
            this.status_correct = this.status_error = this.status_warning = 0;
        }
    }
}
