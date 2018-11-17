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
