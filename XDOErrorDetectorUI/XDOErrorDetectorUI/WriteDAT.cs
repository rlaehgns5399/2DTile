using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XDOErrorDetectorUI
{
    class WriteDAT
    {
        public WriteDAT(ReadDAT dat)
        {
            this.run(dat);
        }
        private void run(ReadDAT dat)
        {
            var bw = new BinaryWriter(File.Open(dat.url + ".bak", FileMode.Create));
            bw.Write(dat.header.level);
            bw.Write(dat.header.IDX);
            bw.Write(dat.header.IDY);
            bw.Write((uint)dat.body.Count);

            for(int i = 0; i < dat.body.Count; i++)
            {
                bw.Write(dat.body[i].version[0]);
                bw.Write(dat.body[i].version[1]);
                bw.Write(dat.body[i].version[2]);
                bw.Write(dat.body[i].version[3]);
                bw.Write(dat.body[i].type);
                bw.Write(dat.body[i].KeyLen);
                if (dat.body[i].KeyLen > 0) bw.Write(Encoding.UTF8.GetBytes(dat.body[i].key));
                bw.Write(dat.body[i].centerPos_x);
                bw.Write(dat.body[i].centerPos_y);
                bw.Write(dat.body[i].altitude);
                bw.Write(dat.body[i].minX);
                bw.Write(dat.body[i].minY);
                bw.Write(dat.body[i].minZ);
                bw.Write(dat.body[i].maxX);
                bw.Write(dat.body[i].maxY);
                bw.Write(dat.body[i].maxZ);
                bw.Write(dat.body[i].ImgLevel);
                bw.Write(dat.body[i].dataFileLen);
                if (dat.body[i].dataFileLen > 0) bw.Write(Encoding.UTF8.GetBytes(dat.body[i].dataFile));
                bw.Write(dat.body[i].imgFileNameLen);
                if(dat.body[i].imgFileNameLen > 0) bw.Write(Encoding.UTF8.GetBytes(dat.body[i].imgFileName));
            }

            bw.Close();
        }
    }
}
