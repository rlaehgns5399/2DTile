using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadTreeUI
{
    class Vector2
    {
        double x, y;
        public Vector2(double _x, double _y)
        {
            this.x = _x;
            this.y = _y;
        }

        public double cross(Vector2 other)
        {
            return x * other.y - y * other.x;
        }

        public Vector2 mul(double r)
        {
            return new Vector2(x * r, y * r);
        }

        public Vector2 add(Vector2 other)
        {
            return new Vector2(x + other.x, y + other.y);
        }

        public Vector2 minus(Vector2 other)
        {
            return new Vector2(x - other.x, y - other.y);
        }

        public bool eq(Vector2 other)
        {
            return x == other.x && y == other.y;
        }
        public bool islow(Vector2 other){
            return x < other.x && y < other.y;
         }

    }
}
