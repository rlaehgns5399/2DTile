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
        public RayCasting(List<Point> point_set, Quadtree[,] qtree, int depth)
        {
            this.point_set = point_set;
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
        public bool pointInPolygon(Point p)
        {
            int i = 0;
            int j = this.point_set.Count - 1;
            bool oddNodes = false;

            for(i = 0;i < this.point_set.Count; i++)
            {
                if( point_set[i].y < p.y && point_set[j].y >= p.y 
                    || point_set[j].y < p.y && point_set[i].y >= p.y){
                    if ( point_set[i].x + (p.y - point_set[i].y ) / (point_set[j].y - point_set[i].y) * (point_set[j].x - point_set[i].x) < p.x)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
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

                        int count = 0;
                        for (int t = 0; t < 4; t++)
                        {
                            Point point = new Point(points[t, 0], points[t, 1]);
                            if (pointInPolygon(point))
                                count++;
                            if (count == 4)
                            {
                                Brush brush = (Brush)Brushes.LawnGreen;
                                Form1.g.FillRectangle(brush, (j) * Program.WINDOW_CONST, 500 - (i + 1) * Program.WINDOW_CONST, Program.WINDOW_CONST, Program.WINDOW_CONST);
                                result.Add(tile[i, j]);
                            }
                        }
                        
                    }
                }
            }

            return null;
        }
    }
}
