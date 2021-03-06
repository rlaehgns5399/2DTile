﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace QuadTreeUI
{
    class RayCasting
    {
        List<string> result = new List<string>();
        private List<Point> point_set;
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
            // Ray Casting algorithm
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

        private double ccw(Vector2 a, Vector2 b)
        {
            return a.cross(b);
        }
        private double ccw(Vector2 p, Vector2 a, Vector2 b)
        {
            return ccw(a.minus(p), b.minus(p));
        }
        private bool sementIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            double ab = ccw(a, b, c) * ccw(a, b, d);
            double cd = ccw(c, d, a) * ccw(c, d, b);
            if (ab == 0 && cd == 0)
            {
                if (b.islow(a))
                {
                    Vector2 temp = new Vector2(a.x, a.y);
                    a = b;
                    b = temp;
                }
                if (d.islow(c))
                {
                    Vector2 temp = new Vector2(c.x, c.y);
                    c = d;
                    d = temp;
                }
                return !(b.islow(c) || d.islow(a));
            }

            return ab <= 0 && cd <= 0;
        }
        private bool sementIntersectIterator(Vector2 p1, Vector2 p2, int n, int m, Point[] tile_point_set)
        {
            if (!sementIntersect(p1, p2, new Vector2(tile_point_set[n]), new Vector2(tile_point_set[m]))) return false;
            return true;
        }
        private bool isIncorrectTile(int count, double[,] points)
        {
            if(count == 4)
            {
                Point[] tile_point_set = {
                                    new Point(points[0, 0], points[0, 1]),
                                    new Point(points[1, 0], points[1, 1]),
                                    new Point(points[2, 0], points[2, 1]),
                                    new Point(points[3, 0], points[3, 1])
                };

                for (int p = 0; p < point_set.Count; p++)
                {
                    Point p1, p2;
                    if (p < point_set.Count - 1)
                    {
                        p1 = point_set[p];
                        p2 = point_set[p + 1];
                    }
                    else
                    {
                        p1 = point_set[p];
                        p2 = point_set[0];
                    }


                    if (sementIntersectIterator(new Vector2(p1), new Vector2(p2), 0, 1, tile_point_set) == true
                            || sementIntersectIterator(new Vector2(p1), new Vector2(p2), 1, 2, tile_point_set) == true
                            || sementIntersectIterator(new Vector2(p1), new Vector2(p2), 2, 3, tile_point_set) == true
                            || sementIntersectIterator(new Vector2(p1), new Vector2(p2), 3, 0, tile_point_set) == true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private bool isImperfectTile(double[,] points)
        {
                Point[] tile_point_set = {
                                    new Point(points[0, 0], points[0, 1]),
                                    new Point(points[1, 0], points[1, 1]),
                                    new Point(points[2, 0], points[2, 1]),
                                    new Point(points[3, 0], points[3, 1])
                };

                for (int p = 0; p < point_set.Count; p++)
                {
                    Point p1, p2;
                    if (p < point_set.Count - 1)
                    {
                        p1 = point_set[p];
                        p2 = point_set[p + 1];
                    }
                    else
                    {
                        p1 = point_set[p];
                        p2 = point_set[0];
                    }


                    if (sementIntersectIterator(new Vector2(p1), new Vector2(p2), 0, 1, tile_point_set) == true
                            || sementIntersectIterator(new Vector2(p1), new Vector2(p2), 1, 2, tile_point_set) == true
                            || sementIntersectIterator(new Vector2(p1), new Vector2(p2), 2, 3, tile_point_set) == true
                            || sementIntersectIterator(new Vector2(p1), new Vector2(p2), 3, 0, tile_point_set) == true)
                    {
                        return true;
                    }
                }
            return false;
        }
        private void Find_Perfect_Level_0_Tile(Queue<TileItem> q)
        {
            // Level 0: Brute force
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
                    double[,] points = { { j * Program.WINDOW_CONST, i * Program.WINDOW_CONST }, { (j + 1) * Program.WINDOW_CONST, i * Program.WINDOW_CONST }, { (j + 1) * Program.WINDOW_CONST, (i + 1) * Program.WINDOW_CONST }, { j * Program.WINDOW_CONST, (i + 1) * Program.WINDOW_CONST } };

                    int count = 0;
                    for (int t = 0; t < 4; t++)
                    {
                        Point point = new Point(points[t, 0], points[t, 1]);
                        if (pointInPolygon(point))
                            count++;
                    }

                    if(isIncorrectTile(count, points))
                    {
                        // 점 네개는 포함되어 있으나, 선이 겹쳐서 에러인 타일들
                        q.Enqueue(new TileItem((double)j, (double)i, 1));
                        q.Enqueue(new TileItem((double)j + 0.5, (double)i + 0.5, 1));
                        q.Enqueue(new TileItem((double)j + 0.5, (double)i, 1));
                        q.Enqueue(new TileItem((double)j, (double)i + 0.5, 1));
                        continue;
                    }
                    else if(count == 4)
                    {
                        // 완벽한 타일 검출
                        Brush brush = (Brush)Brushes.LawnGreen;
                        Form1.g.FillRectangle(brush, (j) * Program.WINDOW_CONST, 500 - (i + 1) * Program.WINDOW_CONST, Program.WINDOW_CONST, Program.WINDOW_CONST);
                        result.Add("L0_X" + j + "_Y" + i);
                        continue;
                    }
                    else if(isImperfectTile(points))
                    {
                        // 선분에 걸쳐진 타일들 검출
                        Brush brush = (Brush)Brushes.Azure;
                        q.Enqueue(new TileItem((double)j, (double)i, 1));
                        q.Enqueue(new TileItem((double)j+0.5, (double)i+0.5, 1));
                        q.Enqueue(new TileItem((double)j+0.5, (double)i, 1));
                        q.Enqueue(new TileItem((double)j, (double)i+0.5, 1));
                        Form1.g.FillRectangle(brush, (j) * Program.WINDOW_CONST, 500 - (i + 1) * Program.WINDOW_CONST, Program.WINDOW_CONST, Program.WINDOW_CONST);
                        continue;
                    }

                    /////////////////////////////////

                }
            }
        }
        private void Find_Perfect_Level_N_Tile(Queue<TileItem> q)
        {
            while(q.Count != 0)
            {
                TileItem item = q.Dequeue();
                if (item.depth > 9) continue;
                // item을 4개의 작은 item으로 분해?

                /* 
                     * points[0] = bottomLeft   i   ,j
                     * points[1] = bottomRight  i+1 ,j
                     * points[2] = topLeft      i   ,j+1
                     * points[3] = topRight     i+1 ,j+1
                     */
                double[,] points = { 
                    { item.x * Program.WINDOW_CONST, item.y * Program.WINDOW_CONST}, 
                    { (item.x + 1 / Math.Pow(2, item.depth)) * Program.WINDOW_CONST, item.y * Program.WINDOW_CONST}, 
                    { (item.x + 1 / Math.Pow(2, item.depth)) * Program.WINDOW_CONST, (item.y + 1 / Math.Pow(2, item.depth)) * Program.WINDOW_CONST}, 
                    { item.x * Program.WINDOW_CONST, (item.y + 1 / Math.Pow(2, item.depth)) * Program.WINDOW_CONST}
                };

                //Console.WriteLine(item.x + "," + item.y);
                //for (int qs = 0; qs < 4; qs++)
                //Console.WriteLine(points[qs, 0] + ", " + points[qs, 1]);

                int count = 0;
                for (int t = 0; t < 4; t++)
                {
                    Point point = new Point(points[t, 0], points[t, 1]);
                    if (pointInPolygon(point))
                        count++;
                }
                
                //Console.WriteLine(item.x + "," + item.y);
                if (isIncorrectTile(count, points))
                {
                    int newdepth = item.depth + 1;
                    q.Enqueue(new TileItem(item.x, item.y, newdepth));
                    q.Enqueue(new TileItem(item.x + 1 / Math.Pow(2, newdepth), item.y + 1 / Math.Pow(2, newdepth), newdepth));
                    q.Enqueue(new TileItem(item.x + 1 / Math.Pow(2, newdepth), item.y, newdepth));
                    q.Enqueue(new TileItem(item.x, item.y + 1 / Math.Pow(2, newdepth), newdepth));
                    // 점 네개는 포함되어 있으나, 선이 겹쳐서 에러인 타일들
                    //Brush localbrush = (Brush)Brushes.Pink;
                    //Form1.g.FillRectangle(localbrush, (j) * Program.WINDOW_CONST, 500 - (i + 1) * Program.WINDOW_CONST, Program.WINDOW_CONST, Program.WINDOW_CONST);
                    continue;
                }
                else if (count == 4)
                {
                    // 완벽한 타일 검출

                    Brush brush = (Brush)Brushes.Black;
                    switch(item.depth){
                        case 1:
                            brush = (Brush)Brushes.MistyRose;
                            break;
                        case 2:
                            brush = (Brush)Brushes.Red;
                            break;
                        case 3:
                            brush = (Brush)Brushes.Orange;
                            break;
                        case 4:
                            brush = (Brush)Brushes.Yellow;
                            break;
                        case 5:
                            brush = (Brush)Brushes.Green;
                            break;
                        case 6:
                            brush = (Brush)Brushes.Blue;
                            break;
                        case 7:
                            brush = (Brush)Brushes.Navy;
                            break;
                        case 8:
                            brush = (Brush)Brushes.Purple;
                            break;
                        case 9:
                            brush = (Brush)Brushes.MintCream;
                            break;
                        case 10:
                            brush = (Brush)Brushes.Brown;
                            break;
                        case 11:
                            brush = (Brush)Brushes.Silver;
                            break;
                        case 12:
                            brush = (Brush)Brushes.Gold;
                            break;
                        case 13:
                            brush = (Brush)Brushes.Orchid;
                            break;
                        case 14:
                            brush = (Brush)Brushes.Magenta;
                            break;
                        case 15:
                            brush = (Brush)Brushes.Khaki;
                            break;
                    }
                    Form1.g.FillRectangle(brush, (float)((item.x) * Program.WINDOW_CONST), 
                        500 - (float)((item.y + 1 / Math.Pow(2, item.depth)) * Program.WINDOW_CONST),
                        (float)(Program.WINDOW_CONST / Math.Pow(2, item.depth)),
                        (float)(Program.WINDOW_CONST / Math.Pow(2, item.depth)));
                    result.Add("L" + item.depth + "_X" + item.x + "_Y" + item.y);
                    continue;
                }
                else if (isImperfectTile(points))
                {
                    // 선분에 걸쳐진 타일들 검출
                    int newdepth = item.depth + 1;
                    q.Enqueue(new TileItem(item.x, item.y, newdepth));
                    q.Enqueue(new TileItem(item.x + 1 / Math.Pow(2, newdepth), item.y + 1 / Math.Pow(2, newdepth), newdepth));
                    q.Enqueue(new TileItem(item.x + 1 / Math.Pow(2, newdepth), item.y, newdepth));
                    q.Enqueue(new TileItem(item.x, item.y + 1 / Math.Pow(2, newdepth), newdepth));
                    continue;
                }
            }

            
        }
        public List<Quadtree> start(Quadtree[,] tile, Graphics g)
        {
            Queue<TileItem> q = new Queue<TileItem>();
            //int qindex = 1;
            //foreach(Point p in point_set)
            //{
            //    Console.WriteLine((qindex++) + ": (" + p.x + ", " + p.y + ")");
            //}

            
            Find_Perfect_Level_0_Tile(q);
            Find_Perfect_Level_N_Tile(q);
             
            

            return null;
        }
    }
}
