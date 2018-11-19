using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace QuadTreeUI
{
    class subroutine
    {
        public static bool DEBUG = true;
        private List<Point> point_set = new List<Point>();
        private List<Line> line_set = new List<Line>();
        public const int ROW = 5;
        public const int COL = 10;
        private const double RANDOM_CONST_X = 10;
        private const double RANDOM_CONST_Y = 5;

        public double minX = Double.MaxValue;
        public double minY = Double.MaxValue;
        public double maxX = Double.MinValue;
        public double maxY = Double.MinValue;
        public static Quadtree2[,] tile = new Quadtree2[ROW, COL];
        

        private int WINDOW_CONST = 100;
        public subroutine()
        {
            MakePoint(point_set, 2048);
            MakeLine(line_set, point_set);

            // now we have lv 0 tile (5x10) (width & height = 1)
            for (int i = 0; i < ROW; i++)
            {
                for (int j = 0; j < COL; j++)
                {
                    tile[i, j] = new Quadtree2(new QuadTreeUI.Point(j * WINDOW_CONST, i * WINDOW_CONST), new QuadTreeUI.Point((j + 1) * WINDOW_CONST, (i + 1) * WINDOW_CONST));
                    // Form1.g.DrawLine(Form1.pen, j * 100, 500 - i * 100, (j + 1) * 100, 500 - (i+1) * 100);
                }
            }

            //RayCasting caster = new RayCasting(line_set, tile, 0);
            //List<Quadtree> lv0_tile = caster.start(tile);


            foreach (QuadTreeUI.Point p in point_set)
            {
                tile[(int)Math.Floor(p.y/(double)WINDOW_CONST), (int)Math.Floor(p.x/(double)WINDOW_CONST)].insert(new Node(p, 0));
            }
            //foreach (Line s in line_set)
            //{
            //    Console.WriteLine("(" + s.first.x + ", " + s.first.y + ") ~ (" + s.second.x + ", " + s.second.y + ")");
            //}

            // Console.ReadKey();
        }

        private void MakePoint(List<Point> set, int n)
        {
            for (int i = 0; i < n; i++)
            {
                double x = new Random(Guid.NewGuid().GetHashCode()).NextDouble() * RANDOM_CONST_X * WINDOW_CONST;
                double y = new Random(Guid.NewGuid().GetHashCode()).NextDouble() * RANDOM_CONST_Y * WINDOW_CONST;
                if (x <= minX) minX = x;
                if (x >= maxX) maxX = x;
                if (y <= minY) minY = y;
                if (y >= maxY) maxY = y;
                set.Add(new Point(x, y));

                Console.WriteLine(x + ", " + y);
                int circlesize = 2;
                Brush brush = (Brush)Brushes.Blue;
                Form1.g.FillRectangle(brush, (float)x-circlesize/2, 500-(float)y-circlesize/2, circlesize, circlesize);
            }
        }

        private void MakeLine(List<Line> lineset, List<Point> set)
        {
            Pen p = new Pen(Color.Black, 1);
            for (int i = 0; i < set.Count; i++)
            {
                if (i != set.Count - 1)
                {
                    // Form1.g.DrawLine(p, (float)set[i].x, 500-(float)set[i].y, (float)set[i + 1].x, 500-(float)set[i + 1].y);
                    lineset.Add(new Line(set[i], set[i + 1]));
                }
                // make circular
                else
                {
                    // Form1.g.DrawLine(p, (float)set[i].x, 500-(float)set[i].y, (float)set[0].x, 500-(float)set[0].y);
                    lineset.Add(new Line(set[i], set[0]));
                }
            }
        }
    }



    class Line
    {
        public Point first, second;
        public Line(Point x, Point y)
        {
            this.first = x;
            this.second = y;
        }
    }
}
