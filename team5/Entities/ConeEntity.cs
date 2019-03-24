using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5.Entities
{
    class ConeEntity : Entity
    {
        //Angles are specified counterclockwise.
        private float angle1;
        private float angle2;
        private float radius;
        private Vector2 ConePoint1;
        private Vector2 ConePoint2;
        private Vector2 Dir1;
        private Vector2 Dir2;
        private Vector3 ConeLine1;
        private Vector3 ConeLine2;
        private bool computedBB = false;

        private RectangleF BoundingBox;

        public float Radius {
            get {
                return radius;
            }
            set {
                radius = value;
                computedBB = false;
            }
        }

        public float Angle1
        {
            get
            {
                return angle1;
            }
            set
            {
                angle1 = value;
                computedBB = false;
            }
        }

        public float Angle2
        {
            get
            {
                return angle2;
            }
            set
            {
                angle2 = value;
                computedBB = false;
            }
        }

        public ConeEntity(Game1 game) : base(game)
        {
        }

        private void RecomputeBB()
        {
            var xvals = new List<float>();
            var yvals = new List<float>();

            xvals.Add(Position.X);
            yvals.Add(Position.Y);

            if(angle2 < angle1)
            {
                xvals.Add(Position.X + radius);
            }

            if((Math.PI > angle1 && Math.PI < angle2) || (angle2 < angle1 && Math.PI > angle1 || Math.PI < angle2))
            {
                xvals.Add(Position.X - radius);
            }

            if ((Math.PI * 0.5F > angle1 && Math.PI * 0.5F < angle2) || (angle2 < angle1 && Math.PI * 0.5F > angle1 || Math.PI * 0.5F < angle2))
            {
                yvals.Add(Position.Y - radius);
            }

            if ((Math.PI * 1.5F > angle1 && Math.PI * 1.5F < angle2) || (angle2 < angle1 && Math.PI * 1.5F > angle1 || Math.PI * 1.5F < angle2))
            {
                yvals.Add(Position.Y + radius);
            }


            xvals.Add(Position.X + radius * (float)Math.Cos(angle1));
            xvals.Add(Position.X + radius * (float)Math.Cos(angle2));

            yvals.Add(Position.Y + radius * (float)Math.Sin(angle1));
            yvals.Add(Position.Y + radius * (float)Math.Sin(angle2));

            BoundingBox.X = xvals.Min();
            BoundingBox.Y = yvals.Min();

            BoundingBox.Width = xvals.Max() - BoundingBox.X;
            BoundingBox.Height = yvals.Max() - BoundingBox.Y;

            ConePoint1 = new Vector2(radius * (float)Math.Cos(angle1), radius * (float)Math.Sin(angle1)) + Position;
            ConePoint2 = new Vector2(radius * (float)Math.Cos(angle2), radius * (float)Math.Sin(angle2)) + Position;

            ConeLine1 = Vector3.Cross(new Vector3(ConePoint1, 1), new Vector3(Position, 1));
            ConeLine2 = Vector3.Cross(new Vector3(ConePoint2, 1), new Vector3(Position, 1));

            Dir1 = ConePoint1 - Position;
            Dir1.Normalize();
            Dir2 = ConePoint2 - Position;
            Dir2.Normalize();

            computedBB = true;
        }


        public override RectangleF GetBoundingBox()
        {
            if (!computedBB)
            {
                RecomputeBB();
            }
            return BoundingBox;
        }

        public override bool Collide(Entity source, float timestep, out int direction, out float time)
        {
            direction = 0;
            time = -1;

            if (!(source is BoxEntity))
            {
                return false;
            }

            var boxSource = (BoxEntity)source;

            RectangleF motionBB;

            RectangleF sourceBB = source.GetBoundingBox();
            Vector2 sourceMotion = source.Velocity * timestep;

            var center = new Vector2((boxSource.GetBoundingBox().Left+boxSource.GetBoundingBox().Right)*0.5F,(boxSource.GetBoundingBox().Top + boxSource.GetBoundingBox().Bottom) * 0.5F);

            center -= Position;

            motionBB.X = sourceBB.X + (int)Math.Floor(Math.Min(0.0, sourceMotion.X));
            motionBB.Y = sourceBB.Y + (int)Math.Floor(Math.Min(0.0, sourceMotion.Y));
            motionBB.Width = sourceBB.Width + (int)Math.Ceiling(Math.Max(0.0, sourceMotion.X));
            motionBB.Height = sourceBB.Height + (int)Math.Ceiling(Math.Max(0.0, sourceMotion.Y));

            if (!motionBB.Intersects(GetBoundingBox()))
            {
                return false;
            }

            float centerRad = center.LengthSquared();

            if(centerRad < radius * radius)
            {
                float centerAngle = (float)Math.Atan2(center.Y, center.X);

                if ((centerAngle > angle1 && centerAngle < angle2) || (angle2 < angle1 && centerAngle > angle1 || centerAngle < angle2))
                {
                    return true;
                }
            }

            List<Vector2> polygon = boxSource.GetSweptAABBPolygon(timestep);

            for(int i = 0; i < polygon.Count; ++i)
            {
                int ind2 = i == polygon.Count - 1 ? i + 1 : 0;
                Vector3 line = Vector3.Cross(new Vector3(polygon[i], 1), new Vector3(polygon[ind2], 1));

                Vector3 point1 = Vector3.Cross(ConeLine1,line);
                point1 = point1 / point1.Z;

                float dist1 = Vector2.Dot(new Vector2(point1.X, point1.Y) - Position, Dir1);
                if (dist1 < radius && dist1 > 0)
                {
                    return true;
                }

                Vector3 point2 = Vector3.Cross(ConeLine2, line);
                point2 = point2 / point2.Z;

                float dist2 = Vector2.Dot(new Vector2(point1.X, point2.Y) - Position, Dir2);
                if (dist2 < radius && dist2 > 0)
                {
                    return true;
                }
            }

            for (int i = 0; i < polygon.Count; ++i)
            {
                int ind2 = i == polygon.Count - 1 ? i + 1 : 0;

                Vector2 p1 = polygon[i] - Position;
                Vector2 p2 = polygon[ind2] - Position;

                float a = (p2 - p1).LengthSquared();
                float b = 2 * p1.X * (p2.X - p1.X) + 2 * p1.Y * (p2.Y - p1.Y);
                float c = p1.LengthSquared() - radius * radius;

                float Disc = b * b - 4 * a * c;

                if(Disc < 0)
                {
                    continue;
                }
                if(Disc == 0)
                {
                    float t = -b / 2 * a;

                    if(t > 0 && t < 1)
                    {
                        Vector2 intersect = p1 + t * (p2-p1);

                        float angle = (float)Math.Atan2(intersect.Y, intersect.X);

                        if ((angle > angle1 && angle < angle2) || (angle2 < angle1 && angle > angle1 || angle < angle2))
                        {
                            return true;
                        }
                    }

                    continue;
                    
                }
                if(Disc > 0)
                {
                    float t1 = -b + (float)Math.Sqrt(Disc) / 2 * a;

                    if (t1 > 0 && t1 < 1)
                    {
                        Vector2 intersect = p1 + t1 * (p2 - p1);

                        float angle = (float)Math.Atan2(intersect.Y, intersect.X);

                        if ((angle > angle1 && angle < angle2) || (angle2 < angle1 && angle > angle1 || angle < angle2))
                        {
                            return true;
                        }
                    }

                    float t2 = -b + (float)Math.Sqrt(Disc) / 2 * a;

                    if (t2 > 0 && t2 < 1)
                    {
                        Vector2 intersect = p1 + t2 * (p2 - p1);

                        float angle = (float)Math.Atan2(intersect.Y, intersect.X);

                        if ((angle > angle1 && angle < angle2) || (angle2 < angle1 && angle > angle1 || angle < angle2))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

        }

    }
}
