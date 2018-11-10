using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Npgsql;

namespace XDOErrorDetector
{
    class Program
    {
        //public static String baseURL = @"C:\APM_Setup\htdocs\etri\js\test";
        public static String baseURL = @"C:\Users\KimDoHoon\Desktop\C++_Project\data";
        static void Main(string[] args)
        {
            // Search xdo file from baseURL
            var xdoFileReader = new xdoFileFinder(baseURL);
            List<String> xdoFileList = xdoFileReader.run();

            // XDO(v3.0.0.2) Read
            foreach (String xdoFile in xdoFileList)
            {
                XDO xdo = new XDO(xdoFile);
                String fullURL = xdo.url;
                String baseDirectory = new FileInfo(fullURL).Directory.FullName;

                List<String> imageFileList = new List<String>();

                foreach(String file in Directory.GetFiles(baseDirectory))  imageFileList.Add(file);
                String fileName = Path.GetFileName(fullURL);

                
                foreach (XDOMesh mesh in xdo.mesh)
                {
                    String imageName = mesh.imageName;
                    int status = 0;
                    foreach (String fullPath in imageFileList)
                    {
                        if(fullPath.Equals(baseDirectory + "\\" + imageName))
                        {
                            status = 1;
                            break;
                        } else if (fullPath.ToLower().Equals( (baseDirectory + "\\" + imageName).ToLower() ))
                        {
                            status = 2;
                            break;
                        }
                    }

                    switch (status)
                    {
                        case 0:
                            Console.WriteLine(baseDirectory + "\\" + imageName + ": image is not exist");
                            break;
                        case 1:
                            Console.WriteLine(baseDirectory + "\\" + imageName + ": image is exist");
                            break;
                        case 2:
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
                    using(var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "select * from " + info.Table;

                        using(var reader = cmd.ExecuteReader())
                        {
                            Console.WriteLine("table column 수 = {0} 개", reader.FieldCount);

                            while (reader.Read())
                            {
                                var data = new string[] {
                                    reader["name"].ToString(),
                                    reader["imageError"].ToString()
                                };
                                
                                foreach(var x in data)
                                {
                                    Console.Write(x + "\t");
                                }
                                Console.WriteLine();
                            }
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
}
