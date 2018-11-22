using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadTreeUI
{
    class TileItem
    {
        public double x;
        public double y;
        public int depth;
        public TileItem(double x, double y, int depth)
        {
            this.x = x;
            this.y = y;
            this.depth = depth;
        }
    }
}
