using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadTree
{
    class Program
    {
        private static List<Point> point_set = new List<Point>();
        private static List<Line> line_set = new List<Line>();
        public const int ROW = 5;
        public const int COL = 10;
        private const int RANDOM_CONST = 1000;

        public Quadtree[,] tile = new Quadtree[ROW, COL];

        static void Main(string[] args)
        {
            MakePoint(point_set, 10);
            MakeLine(point_set);
            
            foreach(Line s in line_set)
            {
                Console.WriteLine("(" + s.first.x + ", " + s.first.y + ") ~ (" + s.second.x + ", " + s.second.y + ")");
            }
            Console.ReadKey();
        }

        private static void MakePoint(List<Point> set, int n)
        {
            for(int i = 0; i < n; i++)
            {
                double x = new Random(Guid.NewGuid().GetHashCode()).NextDouble() * RANDOM_CONST;
                double y = new Random(Guid.NewGuid().GetHashCode()).NextDouble() * RANDOM_CONST;
                set.Add(new QuadTree.Point(x, y));
            }
        }

        private static void MakeLine(List<Point> set)
        {
            for(int i = 0; i < set.Count; i++)
            {
                if (i != set.Count - 1) {
                    line_set.Add(new Line(set[i], set[i + 1]));
                }
                // make circular
                else 
                {
                    line_set.Add(new Line(set[i], set[0]));
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
