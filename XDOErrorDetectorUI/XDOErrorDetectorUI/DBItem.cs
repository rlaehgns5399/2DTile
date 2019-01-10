using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XDOErrorDetectorUI
{
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
