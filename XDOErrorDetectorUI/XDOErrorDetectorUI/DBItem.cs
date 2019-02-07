using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XDOErrorDetectorUI
{
    public class XDODBItem
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

    public class DATDBItem
    {
        public int level;
        public int idx;
        public int idy;
        public int objCount;
        public string datFileName;
        public List<int> version = new List<int>();
        public List<string> key = new List<string>();
        public List<double> centerPos_X = new List<double>();
        public List<double> centerPos_Y = new List<double>();
        public List<float> altitude = new List<float>();
        public List<double> minX = new List<double>();
        public List<double> minY = new List<double>();
        public List<double> minZ = new List<double>();
        public List<double> maxX = new List<double>();
        public List<double> maxY = new List<double>();
        public List<double> maxZ = new List<double>();
        public List<int> imgLevel = new List<int>();
        public List<string> dataFile = new List<string>();
        public List<string> imgFileName = new List<string>(); 
    }
}
