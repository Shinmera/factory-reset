using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
    public struct RectangleF
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;
        public Rectangle GetRectangle()
        {
            return new Rectangle((int)X, (int)(Y), (int)Math.Ceiling(Width), (int)Math.Ceiling(Height));
        }

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public RectangleF(Vector2 center, Vector2 size)
        {
            X = center.X - size.X;
            Y = center.Y - size.Y;
            Width = size.X*2;
            Height = size.Y*2;
        }

        public float Bottom
        {
            get
            {
                return Y;
            }
            set
            {
                Y = value;
            }
        }

        public float Top
        {
            get
            {
                return Y + Height;
            }
            set
            {
                Height = value - Y;
            }
        }

        public float Left
        {
            get
            {
                return X;
            }
            set
            {
                X = value;
            }
        }

        public float Right
        {
            get
            {
                return X + Width;
            }
            set
            {
                Width = value - X;
            }
        }

        public Vector2 Center
        {
            get
            {
                return new Vector2(X + Width / 2, Y + Height / 2);
            }
        }

        public bool Intersects(RectangleF target)
        {
            return X < target.X + target.Width &&
                X + Width > target.X &&
                Y < target.Y + target.Height &&
                Y + Height > target.Y;
        }

        public bool Contains(Vector2 point)
        {
            return point.X >= X
                && point.X < X + Width
                && point.Y >= Y
                && point.Y < Y + Height;
        }

        public List<Vector2> ToPolygon()
        {
            List<Vector2> res = new List<Vector2>(4);
            res.Add(new Vector2(X, Y));
            res.Add(new Vector2(X, Y + Height));
            res.Add(new Vector2(X + Width, Y + Height));
            res.Add(new Vector2(X + Width, Y));

            return res;
        }

    }
}
