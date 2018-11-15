using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadTree
{
    class Quadtree
    {
        // defines boundary
        Point topRight;
        Point bottomLeft;

        Node n;

        Quadtree topLeftTree;
        Quadtree topRightTree;
        Quadtree bottomLeftTree;
        Quadtree bottomRightTree;

        public Quadtree()
        {
            this.topRight = new Point(0, 0);
            this.bottomLeft = new Point(0, 0);

            n = null;

            topLeftTree = topRightTree = bottomLeftTree = bottomRightTree = null;
        }
        public Quadtree(Point tr, Point bl)
        {
            this.topRight = tr;
            this.bottomLeft = bl;

            n = null;

            topLeftTree = topRightTree = bottomLeftTree = bottomRightTree = null;
        }
        public void insert(Node n)
        {

        }
        public Node search(Point p)
        {
            return null;
        }
        public bool isBoundary(Point p)
        {
            return false;
        }
    }

    class Node
    {
        Point pos;
        int data;
        public Node(Point pos, int data)
        {
            this.pos = pos;
            this.data = data;
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
}
