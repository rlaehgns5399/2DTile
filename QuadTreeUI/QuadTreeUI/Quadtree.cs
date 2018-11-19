using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace QuadTreeUI
{
    class Quadtree
    {
        private static double RATIO = 100;
        private double UNIT = 1 * RATIO / Math.Pow(2, 15);

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
        public Quadtree(Point bl, Point tr)
        {
            this.topRight = tr;
            this.bottomLeft = bl;

            n = null;

            Pen qpen = new Pen(Color.Red);

            topLeftTree = topRightTree = bottomLeftTree = bottomRightTree = null;
        }
        public Quadtree(Point bl, Point tr, Node node)
        {
            this.topRight = tr;
            this.bottomLeft = bl;

            n = node;

            Pen qpen = new Pen(Color.Red);
            
            Form1.g.DrawLine(qpen, (float)bl.x, (subroutine.ROW * Program.WINDOW_CONST) - (float)tr.y, (float)tr.x, (subroutine.ROW * Program.WINDOW_CONST) - (float)tr.y);
            Form1.g.DrawLine(qpen, (float)bl.x, (subroutine.ROW * Program.WINDOW_CONST) - (float)bl.y, (float)tr.x, (subroutine.ROW * Program.WINDOW_CONST) - (float)bl.y);
            Form1.g.DrawLine(qpen, (float)bl.x, (subroutine.ROW * Program.WINDOW_CONST) - (float)bl.y, (float)bl.x, (subroutine.ROW * Program.WINDOW_CONST) - (float)tr.y);
            Form1.g.DrawLine(qpen, (float)tr.x, (subroutine.ROW * Program.WINDOW_CONST) - (float)bl.y, (float)tr.x, (subroutine.ROW * Program.WINDOW_CONST) - (float)tr.y);

            topLeftTree = topRightTree = bottomLeftTree = bottomRightTree = null;
        }
        public void insert(Node node)
        {
            if (node == null) return;

            // this quadtree cannot adapt this node
            if (!inBoundary(node.pos)) return;
            
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
                        bottomLeftTree = new Quadtree(
                            new Point(bottomLeft.x, bottomLeft.y),
                            new Point((bottomLeft.x + topRight.x) / 2, (bottomLeft.y + topRight.y) / 2),
                            node
                        );
                    }
                    else
                    {
                        bottomLeftTree.insert(node);
                    }
                }
                else
                {
                    if (topLeftTree == null)
                    {
                        topLeftTree = new Quadtree(
                            new Point(bottomLeft.x, (bottomLeft.y + topRight.y) / 2),
                            new Point((bottomLeft.x + topRight.x) / 2, topRight.y),
                            node
                        );
                    }
                    else
                    {
                        topLeftTree.insert(node);
                    }
                }
            }
            else
            {
                if ((bottomLeft.y + topRight.y) / 2 >= node.pos.y)
                {
                    if (bottomRightTree == null)
                    {
                        bottomRightTree = new Quadtree(
                            new Point((bottomLeft.x + topRight.x) / 2, bottomLeft.y),
                            new Point(topRight.x, (bottomLeft.y + topRight.y) / 2),
                            node
                        );
                    }
                    else
                    {
                        bottomRightTree.insert(node);
                    }
                }
                else
                {
                    if (topRightTree == null)
                    {
                        if (subroutine.DEBUG == true) Console.WriteLine(node.pos.x + ", " + node.pos.y + ": [" + this.bottomLeft.x + "," + this.bottomLeft.y + "~" + this.topRight.x + "," + this.topRight.y + "] topRight");
                        topRightTree = new Quadtree(
                            new Point((bottomLeft.x + topRight.x) / 2, (bottomLeft.y + topRight.y) / 2),
                            new Point(topRight.x, topRight.y),
                            node
                        );
                    }
                    else
                    {
                        topRightTree.insert(node);
                    }
                }
            }
            if (this.n != null)
            {
                Node temp = new Node(new Point(n.pos.x, n.pos.y), 0);
                this.n = null;
                insert(temp);
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
            return (p.x >= bottomLeft.x && p.x < topRight.x && p.y >= bottomLeft.y && p.y < topRight.y);
        }
    }
}
