using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XDOErrorDetectorUI
{
    class XDODBItem
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

    class DATDBItem
    {
        public uint level;
        public uint idx;
        public uint idy;
        public uint objCount;
        public List<int> version = new List<int>();
        public List<string> key = new List<string>();
        public List<double[]> centerPos = new List<double[]>();
        public List<float> altitude = new List<float>();
        public List<double[]> box = new List<double[]>();
        public List<int> imgLevel = new List<int>();
        public List<string> dataFile = new List<string>();
        public List<string> imgFileName = new List<string>(); 
    }
}
