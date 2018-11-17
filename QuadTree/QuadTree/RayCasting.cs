using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadTree
{
    class RayCasting
    {
        private List<Point> point_set;
        private Quadtree[,] qtree;
        private int depth;
        public RayCasting(List<Point> point_set, Quadtree[,] qtree, int depth)
        {
            this.point_set = point_set;
            this.qtree = qtree;
            this.depth = depth;
        }
        public List<Quadtree> start()
        {
            return null;
        }
    }
}
