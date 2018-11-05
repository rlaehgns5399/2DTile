using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace XDOErrorDetector
{
    class Program
    {
        static void Main(string[] args)
        {
            var info = new DB();
            using (var conn = new NpgsqlConnection("Host=" + info.Host + ";Username=" + info.Username + ";Password=" + info.Password + ";Database=" + info.Database))
            {
                try
                {
                    conn.Open();
                    using(var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "select * from xdo";

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
    }
}
