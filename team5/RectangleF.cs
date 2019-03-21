using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace team5
{
    class RectangleF
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public RectangleF()
        {

        }

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Intersects(RectangleF target)
        {
            return X < target.X + target.Width &&
                X + Width > target.X &&
                Y < target.Y + target.Height &&
                Y + Height > target.Y;
        }

    }
}
