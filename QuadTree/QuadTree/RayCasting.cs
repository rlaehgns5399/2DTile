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
        private List<Line> line_set;
        private Quadtree[,] qtree;
        private int depth;
        public RayCasting(List<Line> line_set, Quadtree[,] qtree, int depth)
        {
            this.line_set = line_set;
            this.qtree = qtree;
            this.depth = depth;
        }
        private bool getInterceptPoint(Point AP1, Point AP2, Point BP1, Point BP2)
        {
            double t, s;
            double under = (BP2.y - BP1.y) * (AP2.x - AP1.x) - (BP2.x - BP1.x) * (AP2.y - AP1.y);
            if (under == 0) return false;

            double _t = (BP2.x - BP1.x) * (AP1.y - BP1.y) - (BP2.y - BP1.y) * (AP1.x - BP1.x);
            double _s = (AP2.x - AP1.x) * (AP1.y - BP1.y) - (AP2.y - AP1.y) * (AP1.x - BP1.x);

            t = _t / under;
            s = _s / under;

            if (t < 0.0 || t > 1.0 || s < 0.0 || s > 1.0) return false;
            if (_t == 0 && _s == 0) return false;

            double x = AP1.x + t * (double)(AP2.x - AP1.x);
            double y = AP1.y + t * (double)(AP2.y - AP1.y);

            return true;
        }
        public List<Quadtree> start()
        {
            // depth 0: brutal force
            if(this.depth == 0)
            {
                for(int i = 0; i < Program.ROW; i++)
                {
                    for(int j = 0; j < Program.COL; j++)
                    {
                        /* 
                         * points[0] = bottomLeft   i   ,j
                         * points[1] = bottomRight  i+1 ,j
                         * points[2] = topLeft      i   ,j+1
                         * points[3] = topRight     i+1 ,j+1
                         */
                        int[,] points = { { i, j }, { i + 1, j }, { i, j + 1 }, { i + 1, j + 1 } };

                        /*
                         * ray[0] = 0   , 0
                         * ray[1] = ROW , 0
                         * ray[2] = 0   , COL
                         * ray[3] = ROW , COL
                         */
                        int[,] ray = { { 0, 0 }, { Program.ROW, 0 }, { 0, Program.COL }, { Program.ROW, Program.COL } };
                        int collision = 0;
                        
                        for(int t = 0; t < 4; t++)
                        {
                            int current_ray_x = ray[i, 0];
                            int current_ray_y = ray[i, 1];

                            foreach (Line line in this.line_set)
                            {
                                Point p1 = line.first;
                                Point p2 = line.second;

                                
                            }
                        }
                        
                    }
                }
            }

            return null;
        }
    }
}
