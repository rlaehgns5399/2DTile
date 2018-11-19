using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace QuadTreeUI
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
                if (subroutine.DEBUG == true) Console.WriteLine("두 직선은 평행하거나 같습니다.");
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
            if (subroutine.DEBUG == true) Console.WriteLine("범위 안에 교점은 없습니다.");
            return false;
        }
        private double ccw(Vector2 a, Vector2 b)
        {
            return a.cross(b);
        }
        private double ccw(Vector2 p, Vector2 a, Vector2 b)
        {
            return ccw(a.minus(p), b.minus(p));
        }
        public bool sementIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            double ab = ccw(a, b, c) * ccw(a, b, d);
            double cd = ccw(c, d, a) * ccw(c, d, b);

            if(ab == 0 && cd == 0)
            {
                if (b.islow(a))
                {
                    Vector2 temp = a;
                    a = b;
                    b = temp;
                }
                if (d.islow(c))
                {
                    Vector2 temp = c;
                    c = d;
                    d = c;
                }
                return !(b.islow(c) || d.islow(a));
            }
            return ab <= 0 && cd <= 0;
        }
        public List<Quadtree> start(Quadtree[,] tile, Graphics g)
        {
            List<Quadtree> result = new List<Quadtree>();
            // depth 0: brutal force
            if (this.depth == 0)
            {
                for (int i = 0; i < subroutine.ROW; i++)
                {
                    for (int j = 0; j < subroutine.COL; j++)
                    {
                        /* 
                         * points[0] = bottomLeft   i   ,j
                         * points[1] = bottomRight  i+1 ,j
                         * points[2] = topLeft      i   ,j+1
                         * points[3] = topRight     i+1 ,j+1
                         */
                        int[,] points = { { j * Program.WINDOW_CONST, i * Program.WINDOW_CONST }, { (j + 1) * Program.WINDOW_CONST, i * Program.WINDOW_CONST }, { j * Program.WINDOW_CONST, (i + 1) * Program.WINDOW_CONST }, { (j + 1) * Program.WINDOW_CONST, (i + 1) * Program.WINDOW_CONST } };

                        /*
                         * ray[0] = 0   , 0
                         * ray[1] = ROW , 0
                         * ray[2] = 0   , COL
                         * ray[3] = ROW , COL
                         */
                        int[,] ray_set = { { 0, 0 }, { subroutine.COL * Program.WINDOW_CONST, 0 }, { 0, subroutine.ROW * Program.WINDOW_CONST }, { subroutine.COL * Program.WINDOW_CONST, subroutine.ROW * Program.WINDOW_CONST } };
                        int collision = 0;

                        for (int t = 0; t < 1; t++)
                        {
                            Point ray = new Point(ray_set[t, 0], ray_set[t, 1]);
                            foreach (Line line in this.line_set)
                            {
                                Point rayend = new Point(points[t, 0], points[t, 1]);
                                Point p1 = line.first;
                                Point p2 = line.second;

                                Point intercept = new Point(double.MinValue, double.MinValue);
                                if (getInterceptPoint(ray, rayend, p1, p2, intercept))
                                {
                                    if (subroutine.DEBUG == true)
                                    {
                                        Console.WriteLine("선분 ray(({0},{1}) ~ ({2},{3}))와 선분(({4},{5}) ~ ({6},{7})) 사이에 교점",
                                            ray.x, ray.y, rayend.x, rayend.y, p1.x, p1.y, p2.x, p2.y);
                                    }
                                    collision++;
                                }
                            }
                        }

                        // if collision is odd, it is inside 
                        if (collision % 2 == 1)
                        {
                            result.Add(tile[i, j]);
                            Brush brush = (Brush)Brushes.LawnGreen;
                            Form1.g.FillRectangle(brush, (j) * Program.WINDOW_CONST, 500 - (i + 1)* Program.WINDOW_CONST,  Program.WINDOW_CONST, Program.WINDOW_CONST);
                            Console.WriteLine("Fill: " + i + ", " + j);
                        }
                    }
                }
            }

            return null;
        }
    }
}
