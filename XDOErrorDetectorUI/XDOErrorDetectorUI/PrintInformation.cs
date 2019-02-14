using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XDOErrorDetectorUI
{
    class PrintInformation
    {
        public PrintInformation(ReadXDO xdo)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("URL:\t[" + xdo.url + "]");
            Console.ResetColor();
            Console.WriteLine("TYPE:\t" + xdo.XDOType);
            Console.WriteLine("ObjId:\t" + xdo.ObjectID);
            Console.WriteLine("Key:\t" + xdo.Key + "(" + xdo.KeyLen + ")");
            Console.WriteLine("MinMax:\t");
            Console.WriteLine("\t\t" + xdo.minX + " ~ " + xdo.maxX);
            Console.WriteLine("\t\t" + xdo.minY + " ~ " + xdo.maxY);
            Console.WriteLine("\t\t" + xdo.minZ + " ~ " + xdo.maxZ);
            Console.WriteLine("Altitude:\t" + xdo.altitude);
            Console.WriteLine("FaceNum:\t" + xdo.faceNum);
            Console.WriteLine("XDOVersion:\t" + "3.0.0." + xdo.XDOVersion);

            bool redo = false;
            if (xdo.faceNum == 0)
            {
                xdo.faceNum = 1;
                redo = true;                
            }
            for (int i = 0; i < xdo.faceNum; i++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("=== Face: " + i + " ===");
                Console.ResetColor();
                Console.WriteLine("Vertex:\t" + xdo.mesh[i].vertexCount);
                Console.WriteLine("Indice:\t" + xdo.mesh[i].indexedCount);
                Console.WriteLine("Color:\t" + xdo.mesh[i].Color.r + "," + xdo.mesh[i].Color.g + "," + xdo.mesh[i].Color.b + "," + xdo.mesh[i].Color.a);
                Console.WriteLine("imgLv:\t" + xdo.mesh[i].ImageLevel);
                Console.WriteLine("imgName:\t" + xdo.mesh[i].imageName + "(" + xdo.mesh[i].ImageNameLen + ")");
                Console.WriteLine("NailImg:\t" + xdo.mesh[i].nailImage + "(" + xdo.mesh[i].nailLen + ")");
            }

            if (redo)
            {
                xdo.faceNum = 0;
            }
            Console.WriteLine();
        }
    }
}
