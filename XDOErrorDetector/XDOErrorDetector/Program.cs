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
        public static String baseURL = @"C:\Users\KimDoHoon\Desktop\git\2DTile\XDOErrorDetector\data";
        enum STATUS{
            SUCCESS,
            ERR_NOT_EXIST_FILE,
            WARN_UPPER_LOWER_CASE,
            UNKNOWN_CODE
        }
        static STATUS getStatus(String fullPath, String path, int level=-1)
        {
            if(level == -1)
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
                for(int i = 0; i < level; i++)
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
            
            // XDO(v3.0.0.2) Read -> 이미지 집합 생성
            // 나중에 안쓰이는 그림 파일 찾을 때 쓰임
            foreach (var xdoFile in xdoFileList)
            {
                var xdo = new XDO(xdoFile);
                var baseDirectory = new FileInfo(xdo.url).Directory.FullName;

                var xdo_dbItem = new DBItem();
                

                foreach(var imgFile in Directory.GetFiles(baseDirectory))
                {
                    if (imgFile.ToLower().Contains(".jpg") || imgFile.ToLower().Contains(".png"))
                        imageSet.Add(imgFile);
                }
            }

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
                        // cmd.ExecuteNonQuery();
                    }
                    foreach (KeyValuePair<String, DBItem> key in hashMap)
                    {
                        using (var cmd = new NpgsqlCommand())
                        {
                            // Console.WriteLine(key.Key + "\n(" + key.Value.status_correct + ", " + key.Value.status_warning + ", " + key.Value.status_error + ")");

                            cmd.Connection = conn;
                            
                            cmd.CommandText = "insert into @table (\"name\",\"imageError\",\"imageSuccess\",\"imageWarning\") values(@name, @error, @success, @warning)";
                            cmd.Parameters.AddWithValue("table", info.Table);
                            cmd.Parameters.AddWithValue("name", key.Key);
                            cmd.Parameters.AddWithValue("error", key.Value.status_error);
                            cmd.Parameters.AddWithValue("success", key.Value.status_correct);
                            cmd.Parameters.AddWithValue("warning", key.Value.status_warning);
                            // Console.WriteLine(key.Key + "/" + key.Value.status_error + "/" + key.Value.status_correct + "/" + key.Value.status_warning);
                            // cmd.ExecuteNonQuery();
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
