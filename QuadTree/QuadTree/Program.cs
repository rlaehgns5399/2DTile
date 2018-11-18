using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadTree
{
    class Program
    {
        public static bool DEBUG = true;
        private static List<Point> point_set = new List<Point>();
        private static List<Line> line_set = new List<Line>();
        public const int ROW = 5;
        public const int COL = 10;
        private const double RANDOM_CONST_X = 10;
        private const double RANDOM_CONST_Y = 5;

        public static double minX = Double.MaxValue;
        public static double minY = Double.MaxValue;
        public static double maxX = Double.MinValue;
        public static double maxY = Double.MinValue;
        public static Quadtree[,] tile = new Quadtree[ROW, COL];

        static void Main(string[] args)
        {
            MakePoint(point_set, 3);
            MakeLine(line_set, point_set);

            // now we have lv 0 tile (5x10) (width & height = 1)
            for(int i = 0; i < ROW; i++)
            {
                for(int j = 0; j < COL; j++)
                {
                    tile[i,j] = new Quadtree(new Point(i, j), new Point(i+1, j+1));
                }
            }

            RayCasting caster = new RayCasting(line_set, tile, 0);
            List<Quadtree> lv0_tile = caster.start(tile); 


            foreach (Point p in point_set)
            {
                tile[(int)Math.Floor(p.x), (int)Math.Floor(p.y)].insert(new Node(p, 0));
            }
            //foreach (Line s in line_set)
            //{
            //    Console.WriteLine("(" + s.first.x + ", " + s.first.y + ") ~ (" + s.second.x + ", " + s.second.y + ")");
            //}

            Console.ReadKey();
        }

        private static void MakePoint(List<Point> set, int n)
        {
            for(int i = 0; i < n; i++)
            {
                double x = new Random(Guid.NewGuid().GetHashCode()).NextDouble() * RANDOM_CONST_X;
                double y = new Random(Guid.NewGuid().GetHashCode()).NextDouble() * RANDOM_CONST_Y;
                if (x <= minX) minX = x;
                if (x >= maxX) maxX = x;
                if (y <= minY) minY = y;
                if (y >= maxY) maxY = y;
                set.Add(new QuadTree.Point(x, y));
            }
        }

        private static void MakeLine(List<Line> lineset, List<Point> set)
        {
            for(int i = 0; i < set.Count; i++)
            {
                if (i != set.Count - 1) {
                    lineset.Add(new Line(set[i], set[i + 1]));
                }
                // make circular
                else 
                {
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
