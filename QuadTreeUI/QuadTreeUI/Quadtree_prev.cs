using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace QuadTreeUI
{
    class QuadtreePrev
    {

        private static double RATIO = 100;
        private double UNIT = 1 * RATIO / Math.Pow(2, 15);

        // defines boundary
        Point topRight;
        Point bottomLeft;

        Node n;

        QuadtreePrev topLeftTree;
        QuadtreePrev topRightTree;
        QuadtreePrev bottomLeftTree;
        QuadtreePrev bottomRightTree;

        public QuadtreePrev()
        {
            this.topRight = new Point(0, 0);
            this.bottomLeft = new Point(0, 0);

            n = null;

            topLeftTree = topRightTree = bottomLeftTree = bottomRightTree = null;
        }
        public QuadtreePrev(Point bl, Point tr)
        {
            this.topRight = tr;
            this.bottomLeft = bl;

            n = null;

            Pen qpen = new Pen(Color.Red);

            //Form1.g.DrawLine(qpen, (float)bl.y, 500 - (float)(tr.x + bl.x) / 2, (float)tr.y, 500 - (float)(tr.x + bl.x) / 2);
            //Form1.g.DrawLine(qpen, (float)(bl.y + tr.y) / 2, 500 - (float)bl.x, (float)(bl.y + tr.y) / 2, 500 - (float)tr.x);

            Form1.g.DrawLine(qpen, (float)(tr.x + bl.x) / 2, 500 - (float)bl.y, (float)(tr.x + bl.x) / 2, 500 - (float)tr.y);
            Form1.g.DrawLine(qpen, (float)bl.x, (float)(500 - bl.y + 500 - tr.y) / 2, (float)tr.x, (float)(500 - bl.y + 500 - tr.y) / 2);


            //Console.WriteLine("가로선: " + bl.x + "," + (500 - bl.y + 500 - tr.y) / 2 + " ~ " + tr.x + ", " + (500 - bl.y + 500 - tr.y) / 2);
            //Console.WriteLine("세로선: "+ ((tr.x + bl.x) / 2) + ", " + (500 - bl.y) + " ~ " + ((tr.x + bl.x) / 2) + ", " + (500 - tr.y));
            topLeftTree = topRightTree = bottomLeftTree = bottomRightTree = null;
        }
        public void insert(Node node)
        {
            if (node == null) return;

            // this quadtree cannot adapt this node
            if (!inBoundary(node.pos)) return;

            if (this.n == null) {
                this.n = node;
                return;
            }
            // if Minimum unit is reached, stop dividing
            if (Math.Abs(bottomLeft.x - topRight.x) <= UNIT && Math.Abs(bottomLeft.y - topRight.y) <= UNIT)
            {
                if (this.n == null)
                    this.n = node;
                return;
            }

            if ((bottomLeft.x + topRight.x) / 2 >= node.pos.x)
            {
                if ((bottomLeft.y + topRight.y) / 2 >= node.pos.y)
                {
                    if (bottomLeftTree == null)
                    {
                        if (subroutine.DEBUG == true) Console.WriteLine(node.pos.x + ", " + node.pos.y + ": [" + this.bottomLeft.x + "," + this.bottomLeft.y + "~" + this.topRight.x + "," + this.topRight.y + "] bottomLeft");
                        bottomLeftTree = new QuadtreePrev(
                            new Point(bottomLeft.x, bottomLeft.y),
                            new Point((bottomLeft.x + topRight.x) / 2, (bottomLeft.y + topRight.y) / 2)
                        );
                    }
                    bottomLeftTree.insert(node);
                }
                else
                {
                    if (topLeftTree == null)
                    {
                        if (subroutine.DEBUG == true) Console.WriteLine(node.pos.x + ", " + node.pos.y + ": [" + this.bottomLeft.x + "," + this.bottomLeft.y + "~" + this.topRight.x + "," + this.topRight.y + "] topLeft");
                        topLeftTree = new QuadtreePrev(
                            new Point(bottomLeft.x, (bottomLeft.y + topRight.y) / 2),
                            new Point((bottomLeft.x + topRight.x) / 2, topRight.y)
                        );
                    }
                    topLeftTree.insert(node);
                }
            }
            else
            {
                if ((bottomLeft.y + topRight.y) / 2 >= node.pos.y)
                {
                    if (bottomRightTree == null)
                    {
                        if (subroutine.DEBUG == true) Console.WriteLine(node.pos.x + ", " + node.pos.y + ": [" + this.bottomLeft.x + "," + this.bottomLeft.y + "~" + this.topRight.x + "," + this.topRight.y + "] bottomRight");
                        bottomRightTree = new QuadtreePrev(
                            new Point((bottomLeft.x + topRight.x) / 2, bottomLeft.y),
                            new Point(topRight.x, (bottomLeft.y + topRight.y) / 2)
                        );
                    }
                    bottomRightTree.insert(node);
                }
                else
                {
                    if (topRightTree == null)
                    {
                        if (subroutine.DEBUG == true) Console.WriteLine(node.pos.x + ", " + node.pos.y + ": [" + this.bottomLeft.x + "," + this.bottomLeft.y + "~" + this.topRight.x + "," + this.topRight.y + "] topRight");
                        topRightTree = new QuadtreePrev(
                            new Point((bottomLeft.x + topRight.x) / 2, (bottomLeft.y + topRight.y) / 2),
                            new Point(topRight.x, topRight.y)
                        );
                    }
                    topRightTree.insert(node);
                }
            }
        }
        public Node search(Point p)
        {
            if (!inBoundary(p)) return null;

            if (this.n != null) return this.n;

            if ((bottomLeft.x + topRight.x) / 2 >= p.x)
            {
                if ((bottomLeft.y + topRight.y) / 2 >= p.y)
                {
                    if (bottomLeftTree == null)
                        return null;
                    return bottomLeftTree.search(p);
                }
                else
                {
                    if (topLeftTree == null)
                        return null;
                    return topLeftTree.search(p);
                }
            }
            else
            {
                if ((bottomLeft.y + topRight.y) / 2 >= p.y)
                {
                    if (bottomRightTree == null)
                        return null;
                    return bottomRightTree.search(p);
                }
                else
                {
                    if (topRightTree == null)
                        return null;
                    return topRightTree.search(p);
                }
            }
        }
        public bool inBoundary(Point p)
        {
            return (p.x >= bottomLeft.x && p.x <= topRight.x && p.y >= bottomLeft.y && p.y <= topRight.y);
        }
    }

    class Node
    {
        public Point pos;
        public int data;
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
