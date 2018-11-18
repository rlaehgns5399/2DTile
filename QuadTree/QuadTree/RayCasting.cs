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

        public bool getInterceptPoint(Point AP1, Point AP2, Point BP1, Point BP2, Point returnP)
        {
            double isParallel = (AP1.x - AP2.x) * (BP1.y - BP2.y) - (AP1.y - AP2.y) * (BP1.x - BP2.x);
            if (isParallel == 0)
            {
                if(Program.DEBUG == true) Console.WriteLine("두 직선은 평행하거나 같습니다.");
                return false;
            }

            double minx = Math.Min(AP1.x, AP2.x) < Math.Min(BP1.x, BP2.x) ? Math.Min(AP1.x, AP2.x) : Math.Min(BP1.x, BP2.x);
            double maxx = Math.Max(AP1.x, AP2.x) > Math.Max(BP1.x, BP2.x) ? Math.Max(AP1.x, AP2.x) : Math.Max(BP1.x, BP2.x);
            double miny = Math.Min(AP1.y, AP2.y) < Math.Min(BP1.y, BP2.y) ? Math.Min(AP1.y, AP2.y) : Math.Min(BP1.y, BP2.y);
            double maxy = Math.Max(AP1.y, AP2.y) > Math.Max(BP1.y, BP2.y) ? Math.Max(AP1.y, AP2.y) : Math.Max(BP1.y, BP2.y);

            double x = (AP1.x * AP2.y - AP1.y * AP2.x) * (BP1.x - BP2.x) - (AP1.x - AP2.x) * (BP1.x * BP2.y - BP1.y * BP2.x);
            x /= (isParallel);
            
            double y = (AP1.x * AP2.y - AP1.y * AP2.x) * (BP1.y - BP2.y) - (AP1.y - AP2.y) * (BP1.x * BP2.y - BP1.y * BP2.x);
            y /= (isParallel);

            if (x >= minx && x <= maxx && y >= miny && y <= maxy)
            {
                returnP.x = x;
                returnP.y = y;
                return true;
            }
            if (Program.DEBUG == true)  Console.WriteLine("범위 안에 교점은 없습니다.");
            return false;
        }
        public List<Quadtree> start(Quadtree[,] tile)
        {
            List<Quadtree> result = new List<Quadtree>();
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
                        int[,] ray_set = { { 0, 0 }, { Program.ROW, 0 }, { 0, Program.COL }, { Program.ROW, Program.COL } };
                        int collision = 0;
                        
                        for(int t = 0; t < 4; t++)
                        {
                            Point ray = new QuadTree.Point(ray_set[t, 0], ray_set[t, 1]);
                            foreach (Line line in this.line_set)
                            {
                                Point rayend = new Point(points[t, 0], points[t, 1]);
                                Point p1 = line.first;
                                Point p2 = line.second;

                                Point intercept = new Point(double.MinValue, double.MinValue);
                                if (getInterceptPoint(ray, rayend, p1, p2, intercept))
                                {
                                    if(Program.DEBUG == true)
                                    {
                                        Console.WriteLine("ray(({0},{1}) ~ ({2},{3}))와 직선(({4},{5}) ~ ({6},{7})) 사이에 교점({8},{9})이 발견되었습니다.",
                                            ray.x, ray.y, rayend.x, rayend.y, p1.x, p1.y, p2.x, p2.y, intercept.x, intercept.y);
                                    }
                                    collision++;
                                }
                            }
                        }

                        // if collision is odd, it is inside 
                        if (collision % 2 != 0)
                        {
                            result.Add(tile[i, j]);
                        }
                    }
                }
            }

            return null;
        }
    }
}
