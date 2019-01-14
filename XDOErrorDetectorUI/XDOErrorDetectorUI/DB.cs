using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XDOErrorDetectorUI
{
    class DB
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string Table { get; set; }
        public string Port { get; set; }
        public DB(string h, string u, string p, string d, string port)
        {
            this.Port = port;
            this.Host = h;
            this.Username = u;
            this.Password = p;
            this.Database = d;
        }
    }
}
