using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
    struct RectangleF
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public Rectangle GetRectangle()
        {
            return new Rectangle((int)X, (int)Y, (int)Math.Ceiling(Width), (int)Math.Ceiling(Height));
        }

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float Bottom
        {
            get
            {
                return Y + Height;
            }
        }

        public float Top
        {
            get
            {
                return Y;
            }
        }

        public float Left
        {
            get
            {
                return X;
            }
        }

        public float Right
        {
            get
            {
                return X + Width;
            }
        }

        public bool Intersects(RectangleF target)
        {
            return X < target.X + target.Width &&
                X + Width > target.X &&
                Y < target.Y + target.Height &&
                Y + Height > target.Y;
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
