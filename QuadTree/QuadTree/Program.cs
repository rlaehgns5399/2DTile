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
        private const int ROW = 5;
        private const int COL = 10;
        private const int RANDOM_CONST = 1000;
        static void Main(string[] args)
        {
            MakePoint(500);
            MakeLine();
        }

        private static void MakePoint(int n)
        {
            for(int i = 0; i < n; i++)
            {
                point_set.Add(new QuadTree.Point(new Random().NextDouble() * RANDOM_CONST, new Random().NextDouble() * RANDOM_CONST));   
            }
        }

        private static void MakeLine()
        {
            for(int i = 0; i < point_set.Count; i++)
            {
                if (i != point_set.Count - 1) {
                    line_set.Add(new Line(point_set[i], point_set[i + 1]));
                }
                // make circular
                else 
                {
                    line_set.Add(new Line(point_set[i], point_set[0]));
                }
            }
        }
    }

    class Point
    {
        public double x, y;
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
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
