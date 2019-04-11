using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
    class ConeEntity : Entity
    {
        /// <summary>The starting Angle of the cone (CCW)</summary>
        private float LocalAngle1;
        /// <summary>The ending Angle of the cone (CCW)</summary>
        private float LocalAngle2;
        /// <summary>The Radius of the complete unoccluded Cone.</summary>
        private float LocalRadius;
        /// <summary>The position of the starting edge of the cone.</summary>
        private Vector2 ConePoint1;
        /// <summary>The position of the ending edge of the cone.</summary>
        private Vector2 ConePoint2;
        /// <summary>A ray in the direction of the starting edge of the cone.</summary>
        private Vector2 Dir1;
        /// <summary>A ray in the direction of the ending edge of the cone.</summary>
        private Vector2 Dir2;
        /// <summary>The starting edge of the cone as a homogeneous line</summary>
        private Vector3 ConeLine1;
        /// <summary>The ending edge of the cone as a homogeneous line</summary>
        private Vector3 ConeLine2;
        /// <summary>Whether the tight bounding box has been computed</summary>
        private bool ComputedBB = false;
        /// <summary>Whether the occlusion has been computed.</summary>
        private bool ComputedOccludedRadius = false;

        //We need to use two seperate lists for this because apparently binary search is only implemented in lists. WTF.
        private List<float> OcclusionAngles;
        private List<Tuple<Vector2,Vector2,float,float>> OcclusionValues;
        public List<Vector2> Triangles;

        private void AddOcclusionValue(float angle, Tuple<Vector2, Vector2, float, float> occlusionValue)
        {
            OcclusionAngles.Add(angle);
            OcclusionValues.Add(occlusionValue);
        }

        private float GetDistConstraint(float angle)
        {
            angle = ConvertAngle(angle - Angle1);
            int index = OcclusionAngles.BinarySearch(angle);
            if(index > 0)
            {
                float dist = Math.Max(OcclusionValues[index].Item3, OcclusionValues[index].Item4);
                return dist;
            }
            else
            {
                if(index == 0)
                {
                    throw new Exception("OcclusionList has been improperly constructed");
                }

                index = ~index;

                float interp = (angle - OcclusionAngles[index - 1])/(OcclusionAngles[index] - OcclusionAngles[index - 1]);

                return (1 - interp) * OcclusionValues[index - 1].Item4 + interp * OcclusionValues[index].Item3;
            }
        }

        private const float MaxFanAngle = (float)Math.PI/10;

        private RectangleF BoundingBox;

        /// <summary>Clamps any angle into the [0,2pi] domain used by this class.</summary>
        public static float ConvertAngle(float angle)
        {
            angle = angle % (float)(2*Math.PI);
            if(angle < 0)
            {
                angle += (float)(2 * Math.PI);
            }

            return angle;
        }

        public static bool IntersectCircle(Vector2 p1, Vector2 p2, float radius, Vector2 center, float maxT, out float t)
        {
            var relP1 = p1 - center;
            var relP2 = p2 - center;

            float a = (relP2 - relP1).LengthSquared();
            float b = 2 * Vector2.Dot(relP1, (relP2 - relP1));
            float c = relP1.LengthSquared() - radius * radius;

            float Disc = b * b - 4 * a * c;

            t = -1;

            if (Disc <= 0)
            {
                return false;
            }
            else
            {
                float t1 = (-b - (float)Math.Sqrt(Disc)) / (2 * a);
                float t2 = (-b + (float)Math.Sqrt(Disc)) / (2 * a);

                if ((t1 < 0|| t1 > maxT) && (t2 < 0 || t2 > maxT))
                {
                    return false;
                }
                else
                {
                    if(t1 < 0)
                    {
                        t1 = float.PositiveInfinity;
                    }

                    if(t2 < 0)
                    {
                        t2 = float.PositiveInfinity;
                    }
                    t = Math.Min(t1,t2);
                    return true;
                }
            }
        }

        public ConeEntity(Game1 game) : base(game)
        {
        }

        /// <summary>Changes the position of the cone center</summary>
        public void UpdatePosition(Vector2 position)
        {
            Position = position;
            if (Math.Abs(Position.X % 1) < 0.001F || 1 - Math.Abs(Position.X % 1) < 0.001F) {
                position.X += 0.002F;
            }
            if(Math.Abs(Position.Y % 1) < 0.001F || 1 - Math.Abs(Position.Y % 1) < 0.001F)
            {
                position.Y += 0.002F;
            }
            ComputedBB = false;
            ComputedOccludedRadius = false;
        }

        /// <summary>Set to change the full radius, get to get the occluded radius.</summary>
        public float Radius {
            get {
                return LocalRadius;
            }
            set {
                LocalRadius = value;
                ComputedBB = false;
                ComputedOccludedRadius = false;
            }
        }

        /// <summary>The starting Angle of the cone (CCW)</summary>
        public float Angle1
        {
            get
            {
                return LocalAngle1;
            }
            set
            {
                LocalAngle1 = ConvertAngle(value);
                ComputedBB = false;
                ComputedOccludedRadius = false;
            }
        }

        /// <summary>The ending Angle of the cone (CCW)</summary>
        public float Angle2
        {
            get
            {
                return LocalAngle2;
            }
            set
            {
                LocalAngle2 = ConvertAngle(value);
                ComputedBB = false;
                ComputedOccludedRadius = false;
            }
        }

        /// <summary>Sets the starting and ending angles of a cone by a direction and a width, specified in degrees.</summary>
        public void FromDegrees(float direction, float view)
        {
            double deg1 = (direction-view/2) % 360;
            double deg2 = (direction+view/2) % 360;
            Angle1 = (float)(deg1/180*Math.PI);
            Angle2 = (float)(deg2/180*Math.PI);
        }

        /// <summary>Returns the direction of the cone, and the width, in degrees.</summary>
        public void ToDegrees(out float direction, out float view)
        {
            double diff = (LocalAngle2-LocalAngle1)/2;
            if(LocalAngle2 < LocalAngle1) diff += Math.PI;
            direction = (float)((LocalAngle2-diff)*180/Math.PI) % 360;
            view = (float)(diff*2*180/Math.PI) % 360;
        }

        /// <summary>The direction the cone is facing (as an angle)</summary>
        public float Middle
        {
            get
            {
                double diff = (LocalAngle2-LocalAngle1)/2;
                if(LocalAngle2 < LocalAngle1) diff += Math.PI;
                return (float)(LocalAngle2-diff);
            }
            set
            {
                double diff = (LocalAngle2-LocalAngle1)/2;
                if(LocalAngle2 < LocalAngle1) diff += Math.PI;
                Angle1 = (float)((value - diff) % (2*Math.PI));
                Angle2 = (float)((value + diff) % (2*Math.PI));
            }
        }

        /// <summary>The direction the cone is facing (as a binary direction (left/right))</summary>
        public float Direction
        {
            get
            {
                double diff = (LocalAngle2-LocalAngle1)/2;
                if(LocalAngle2 < LocalAngle1) diff += Math.PI;
                double mid = LocalAngle2-diff;
                return ((Math.PI*3)/2 < mid || mid < Math.PI/2) ? +1 : -1;
            }
            set
            {
                if(value == Direction) return;
                double mid = Middle;
                mid = Math.PI - mid;
                if(mid < 0) mid += 2*Math.PI;
                Middle = (float)mid;
            }
        }

        private void RecomputeBB()
        {
            if (ComputedBB)
            {
                return;
            }

            var xvals = new List<float>();
            var yvals = new List<float>();

            xvals.Add(Position.X);
            yvals.Add(Position.Y);

            if(LocalAngle2 < LocalAngle1)
            {
                xvals.Add(Position.X + Radius);
            }

            if((Math.PI > LocalAngle1 && Math.PI < LocalAngle2) || (LocalAngle2 < LocalAngle1 && (Math.PI > LocalAngle1 || Math.PI < LocalAngle2)))
            {
                xvals.Add(Position.X - Radius);
            }

            if ((Math.PI * 0.5F > LocalAngle1 && Math.PI * 0.5F < LocalAngle2) || (LocalAngle2 < LocalAngle1 && (Math.PI * 0.5F > LocalAngle1 || Math.PI * 0.5F < LocalAngle2)))
            {
                yvals.Add(Position.Y - Radius);
            }

            if ((Math.PI * 1.5F > LocalAngle1 && Math.PI * 1.5F < LocalAngle2) || (LocalAngle2 < LocalAngle1 && (Math.PI * 1.5F > LocalAngle1 || Math.PI * 1.5F < LocalAngle2)))
            {
                yvals.Add(Position.Y + Radius);
            }


            xvals.Add(Position.X + Radius * (float)Math.Cos(LocalAngle1));
            xvals.Add(Position.X + Radius * (float)Math.Cos(LocalAngle2));

            yvals.Add(Position.Y + Radius * (float)Math.Sin(LocalAngle1));
            yvals.Add(Position.Y + Radius * (float)Math.Sin(LocalAngle2));

            BoundingBox.X = xvals.Min();
            BoundingBox.Y = yvals.Min();

            BoundingBox.Width = xvals.Max() - BoundingBox.X;
            BoundingBox.Height = yvals.Max() - BoundingBox.Y;

            ConePoint1 = new Vector2(Radius * (float)Math.Cos(LocalAngle1), Radius * (float)Math.Sin(LocalAngle1)) + Position;
            ConePoint2 = new Vector2(Radius * (float)Math.Cos(LocalAngle2), Radius * (float)Math.Sin(LocalAngle2)) + Position;

            ConeLine1 = Vector3.Cross(new Vector3(ConePoint1, 1), new Vector3(Position, 1));
            ConeLine2 = Vector3.Cross(new Vector3(ConePoint2, 1), new Vector3(Position, 1));

            Dir1 = ConePoint1 - Position;
            Dir1.Normalize();
            Dir2 = ConePoint2 - Position;
            Dir2.Normalize();

            ComputedBB = true;
        }

        public override RectangleF GetBoundingBox()
        {
            if (!ComputedBB)
            {
                RecomputeBB();
            }
            return BoundingBox;
        }
        
        public override bool Contains(Vector2 point)
        {
            point = point - Position;
            float r2 = point.LengthSquared();
            float phi = ConvertAngle((float)Math.Atan2(point.Y, point.X));
            return ((phi > LocalAngle1 && phi < LocalAngle2) || (LocalAngle2 < LocalAngle1 && (phi > LocalAngle1 || phi < LocalAngle2)))
                && r2 <= GetDistConstraint(phi);
        }

        public override bool Collide(Entity entity, float timestep, out int direction, out float time, out bool corner)
        {
            corner = false;
            direction = 0;
            time = -1;

            if (!(entity is Movable))
            {
                return false;
            }

            Movable source = (Movable)entity;
            RectangleF sourceBB = source.GetBoundingBox();

            if (!sourceBB.Intersects(GetBoundingBox()))
            {
                return false;
            }

            List<Vector2> points = new List<Vector2>(8)
            {
                new Vector2(sourceBB.Left, sourceBB.Top),
                new Vector2(sourceBB.Left, sourceBB.Bottom),
                new Vector2(sourceBB.Right, sourceBB.Top),
                new Vector2(sourceBB.Right, sourceBB.Bottom),
                new Vector2(sourceBB.Left, source.Position.Y),
                new Vector2(sourceBB.Right, source.Position.Y),
                new Vector2(source.Position.X, sourceBB.Top),
                new Vector2(source.Position.X, sourceBB.Bottom)
            };

            foreach(var point in points)
            {
                if (Contains(point))
                {
                    time = 0;
                    return true;
                }
            }

            return false;
        }

        private static bool IsCloserThanLine(Vector3 line, Vector2 point, Vector2 source, out Vector2 linePoint)
        {
            Vector3 toSource = Vector3.Cross(new Vector3(point, 1), new Vector3(source, 1));
            Vector3 intersect = Vector3.Cross(line, toSource);
            linePoint = new Vector2(intersect.X / intersect.Z, intersect.Y / intersect.Z);

            return (point - source).LengthSquared() < (linePoint - source).LengthSquared();
        }

        public void ComputeOcclusion(Chunk chunk)
        {
            if(!ComputedBB) RecomputeBB();
            var points = chunk.BuildLOSHelper(BoundingBox, Position, Radius, Dir1, Dir2);

            OcclusionAngles = new List<float>();
            OcclusionValues = new List<Tuple<Vector2, Vector2,float,float>>();

            //OcclusionList.Add(0, new Tuple<float,float>(FullRadius,FullRadius));

            bool outofAngle = true;
            bool addedEnd = false;

            Tuple<Vector2, Vector2> closestLine = null;
            Vector3 closestLineHomo = new Vector3(float.NaN);

            Tuple<Vector2, Vector2, float, float> newTuple(Vector2 a, Vector2 b)
            {
                float dista = (a - Position).LengthSquared();
                float distb = (b - Position).LengthSquared();
                return newTuple4(a, b, dista, distb);
            }

            Tuple<Vector2, Vector2, float, float> newTuple4(Vector2 a, Vector2 b, float c, float d)
            {
                return new Tuple<Vector2, Vector2, float, float>(a, b, c, d);
            }

            foreach (var point in points)
            {
                float angle = point.Key;
                if (angle < 0)
                {
                    if(closestLine != null)
                    {
                        if (!IsCloserThanLine(closestLineHomo, point.Value.Item1, Position, out Vector2 closestPoint))
                        {
                            continue;
                        }
                    }

                    if (float.IsNaN(point.Value.Item2.X))
                    {
                        throw new InvalidOperationException("LOSHelper produced invalid structure"); 
                    }

                    closestLine = new Tuple<Vector2, Vector2>(point.Value.Item1, point.Value.Item2);
                    closestLineHomo = Vector3.Cross(new Vector3(closestLine.Item1, 1), new Vector3(closestLine.Item2, 1));
                }
                else
                {
                    if(angle > ConvertAngle(Angle2 - Angle1))
                    {


                        break;
                    }

                    if (outofAngle && angle >= 0)
                    {
                        outofAngle = false;

                        if (angle > 0)
                        {
                            Vector2 closestPoint = ConePoint1;
                            if (closestLine != null)
                            {
                                if(!IsCloserThanLine(closestLineHomo, ConePoint1, Position, out Vector2 linePoint))
                                {
                                    closestPoint = linePoint;
                                }
                            }


                            AddOcclusionValue(0, newTuple(closestPoint, closestPoint));
                        }

                        /**
                        if (angle == 0)
                            
                        {
                            if(point.Value.Item2 == closestLine.Item2)
                            {
                                closestLine = null;
                            }
                            if(IsCloserThanLine(closestLineHomo, point.Key, Position, out Vector2 closestPoint))
                            {
                                if (float.IsNaN(point.Value.Item2.X))
                                {
                                    Vector2 CCWshift = new Vector2(point.Key.Y, -point.Key.X) * 0.0001F;
                                    chunk.IntersectLine(Position, point.Key + CCWshift - Position, float.PositiveInfinity, out float location);
                                    closestPoint = 
                                }
                            }

                            float dist = (closestPoint - Position).LengthSquared();
                            AddOcclusionValue(angle, newTuple(closestPoint, closestPoint, dist, dist));
                        }
                        */
                    }

                    if(closestLine != null && point.Value.Item1 == closestLine.Item2)
                    {
                        if (float.IsNaN(point.Value.Item2.X))
                        {
                            closestLine = null;
                        }
                        else
                        {
                            AddOcclusionValue(angle, newTuple(point.Value.Item1, point.Value.Item1));
                            closestLine = new Tuple<Vector2, Vector2>(point.Value.Item1, point.Value.Item2);
                            closestLineHomo = Vector3.Cross(new Vector3(closestLine.Item1, 1), new Vector3(closestLine.Item2, 1));
                            continue;
                        }
                    }

                    if(closestLine == null)
                    {
                        Vector2 dir = point.Value.Item1 - Position;
                        dir.Normalize();
                        Vector2 CCWoffset = new Vector2(dir.Y, -dir.X) * 0.001F;

                        if (!chunk.IntersectLine(Position, dir - CCWoffset, LocalRadius, out float locationCW))
                        {
                            locationCW = LocalRadius;
                        }
                        if (!chunk.IntersectLine(Position, dir + CCWoffset, LocalRadius, out float locationCCW))
                        {
                            locationCCW = LocalRadius;
                        }
                        Vector2 CW = Position + dir * locationCW;
                        Vector2 CCW = Position + dir * locationCCW;

                        if((CCW - point.Value.Item1).LengthSquared() < 1F && !float.IsNaN(point.Value.Item2.X))
                        {
                            closestLine = new Tuple<Vector2, Vector2>(point.Value.Item1, point.Value.Item2);
                            closestLineHomo = Vector3.Cross(new Vector3(closestLine.Item1, 1), new Vector3(closestLine.Item2, 1));
                        }

                        AddOcclusionValue(angle, newTuple4(CW, CCW, locationCW* locationCW, locationCCW*locationCW));
                    }
                    else
                    {
                        if(IsCloserThanLine(closestLineHomo, point.Value.Item1, Position, out Vector2 linePoint))
                        {
                            closestLine = new Tuple<Vector2, Vector2>(point.Value.Item1, point.Value.Item2);
                            closestLineHomo = Vector3.Cross(new Vector3(closestLine.Item1, 1), new Vector3(closestLine.Item2, 1));

                            AddOcclusionValue(angle, newTuple(linePoint, point.Value.Item1));
                        }
                    }
                }



                /**
                Vector2 dir = point.Value.Item1 - Position;

                if (dir.LengthSquared() < FullRadius * FullRadius)
                {
                    float lastAngle = OcclusionList.Last().Key;
                    float dist = (angles[point.Key] - lastAngle);

                    if (outofRange && dist > MaxFanAngle)
                    {
                        int segments = (int)Math.Ceiling(dist / MaxFanAngle);

                        float portion = dist / segments;

                        for (float f = lastAngle; f < angles[point.Key] - portion / 2; f += portion)
                        {
                            OcclusionList.Add(f, new Tuple<float, float>(FullRadius, FullRadius));
                        }
                    }

                    chunk.IntersectLine(Position, dir, 1, out float location);
                }
                 */
            }

            if (outofAngle)
            {
                AddOcclusionValue(0, newTuple(ConePoint1,ConePoint1));
            }

            Vector2 closestPointEnd = ConePoint2;
            if (closestLine != null)
            {
                if (!IsCloserThanLine(closestLineHomo, ConePoint2, Position, out Vector2 linePoint))
                {
                    closestPointEnd = linePoint;
                }
            }
            else
            {
                if (chunk.IntersectLine(Position, Dir2, LocalRadius, out float distIntersect))
                {
                    closestPointEnd = Position + Dir2 * distIntersect;
                }

            }

            AddOcclusionValue(ConvertAngle(Angle2 - Angle1), newTuple(closestPointEnd, closestPointEnd));

            Triangles = new List<Vector2>
            {
                Capacity = (OcclusionValues.Count - 1) * 3
            };

            for (int i = 0; i < OcclusionValues.Count - 1; ++i)
            {
                Triangles.Add(Position);
                Triangles.Add(OcclusionValues[i].Item2);
                Triangles.Add(OcclusionValues[i - 1].Item1);
            }
        }
        
        public override void Update(Chunk chunk)
        {
            if (!ComputedOccludedRadius)
            {
                ComputedOccludedRadius = true;
                ComputeOcclusion(chunk);
                /*
                OccludedRadius = FullRadius;
                if (chunk.IntersectLine(Position, 
                    new Vector2(OccludedRadius * (float)Math.Cos(LocalAngle1), OccludedRadius * (float)Math.Sin(LocalAngle1)),
                    1, out float location1))
                {
                    OccludedRadius = Math.Min(OccludedRadius, location1 * OccludedRadius);
                    ComputedBB = false;
                }

                if (chunk.IntersectLine(Position,
                    new Vector2(OccludedRadius * (float)Math.Cos(LocalAngle2), OccludedRadius * (float)Math.Sin(LocalAngle2)),
                    1, out float location2))
                {
                    OccludedRadius = Math.Min(OccludedRadius, location2 * OccludedRadius);
                    ComputedBB = false;
                }
                */
            }

            base.Update(chunk);

            if (!chunk.Level.Player.IsHiding)
            {
                if (Collide(chunk.Level.Player, Game1.DeltaT, out int dirction, out float time, out bool corner))
                {
                    chunk.Level.Alarm.Detected = true;
                    chunk.Level.Alarm.SendDrones(chunk.Level.Player.Position);
                }
            }
        }

        public override void Draw()
        {
            RecomputeBB();
            Game.ViewConeEngine.Draw(Position, Radius, Angle1, Angle2);
        }

        /*
        // <Nicolas> This also seems super complicated for what it has to do.
        //           Wouldn't it just be sufficient to test the four corner points of the BoxEntity
        //           against the cone by translating them to polar coordinates and then checking range?
        public bool CollideObsolete(Entity entity, float timestep, out int direction, out float time, out bool corner)
        {
            corner = false;
            direction = 0;
            time = -1;

            if (!(entity is Movable))
            {
                return false;
            }

            Movable source = (Movable)entity;

            RectangleF motionBB;

            RectangleF sourceBB = source.GetBoundingBox();
            Vector2 sourceMotion = source.Velocity * timestep;

            var center = new Vector2((source.GetBoundingBox().Left + source.GetBoundingBox().Right) * 0.5F, (source.GetBoundingBox().Top + source.GetBoundingBox().Bottom) * 0.5F);

            center -= Position;

            motionBB.X = sourceBB.X + (int)Math.Floor(Math.Min(0.0, sourceMotion.X));
            motionBB.Y = sourceBB.Y + (int)Math.Floor(Math.Min(0.0, sourceMotion.Y));
            motionBB.Width = sourceBB.Width + (int)Math.Ceiling(Math.Max(0.0, sourceMotion.X));
            motionBB.Height = sourceBB.Height + (int)Math.Ceiling(Math.Max(0.0, sourceMotion.Y));

            if (!motionBB.Intersects(GetBoundingBox()))
            {
                return false;
            }

            if (!motionBB.Intersects(GetTightBoundingBox()))
            {
                return false;
            }

            float centerRad = center.LengthSquared();

            if (centerRad < OccludedRadius * OccludedRadius)
            {
                float centerAngle = ConvertAngle((float)Math.Atan2(center.Y, center.X));

                if ((centerAngle > LocalAngle1 && centerAngle < LocalAngle2) || (LocalAngle2 < LocalAngle1 && (centerAngle > LocalAngle1 || centerAngle < LocalAngle2)))
                {
                    return true;
                }
            }

            List<Vector2> polygon = source.GetSweptAABBPolygon(timestep);

            for (int i = 0; i < polygon.Count; ++i)
            {
                int ind2 = i != polygon.Count - 1 ? i + 1 : 0;
                Vector3 line = Vector3.Cross(new Vector3(polygon[i], 1), new Vector3(polygon[ind2], 1));

                Vector3 point1 = Vector3.Cross(ConeLine1, line);
                point1 = point1 / point1.Z;

                Vector2 DirPoly = (polygon[ind2] - polygon[i]);
                float lengthPoly = DirPoly.Length();
                DirPoly.Normalize();

                float dist1 = Vector2.Dot(new Vector2(point1.X, point1.Y) - Position, Dir1);
                float distPoly1 = Vector2.Dot(new Vector2(point1.X, point1.Y) - polygon[i], DirPoly);
                if (dist1 < OccludedRadius && dist1 > 0 && distPoly1 < lengthPoly && distPoly1 > 0)
                {
                    return true;
                }

                Vector3 point2 = Vector3.Cross(ConeLine2, line);
                point2 = point2 / point2.Z;

                float dist2 = Vector2.Dot(new Vector2(point2.X, point2.Y) - Position, Dir2);
                float distPoly2 = Vector2.Dot(new Vector2(point2.X, point2.Y) - polygon[i], DirPoly);
                if (dist2 < OccludedRadius && dist2 > 0 && distPoly2 < lengthPoly && distPoly2 > 0)
                {
                    return true;
                }
            }

            for (int i = 0; i < polygon.Count; ++i)
            {
                int ind2 = i != polygon.Count - 1 ? i + 1 : 0;

                Vector2 p1 = polygon[i] - Position;
                Vector2 p2 = polygon[ind2] - Position;

                float a = (p2 - p1).LengthSquared();
                float b = 2 * Vector2.Dot(p1, (p2 - p1));
                float c = p1.LengthSquared() - OccludedRadius * OccludedRadius;

                float Disc = b * b - 4 * a * c;

                if (Disc < 0)
                {
                    continue;
                }
                if (Disc == 0)
                {
                    float t = -b / (2 * a);

                    if (t > 0 && t < 1)
                    {
                        Vector2 intersect = p1 + t * (p2 - p1);

                        float angle = (float)Math.Atan2(intersect.Y, intersect.X);

                        if ((angle > LocalAngle1 && angle < LocalAngle2) || (LocalAngle2 < LocalAngle1 && (angle > LocalAngle1 || angle < LocalAngle2)))
                        {
                            return true;
                        }
                    }

                    continue;

                }
                if (Disc > 0)
                {
                    float t1 = (-b - (float)Math.Sqrt(Disc)) / (2 * a);

                    if (t1 > 0 && t1 < 1)
                    {
                        Vector2 intersect = p1 + t1 * (p2 - p1);

                        float angle = ConvertAngle((float)Math.Atan2(intersect.Y, intersect.X));

                        if ((angle > LocalAngle1 && angle < LocalAngle2) || (LocalAngle2 < LocalAngle1 && (angle > LocalAngle1 || angle < LocalAngle2)))
                        {
                            return true;
                        }
                    }

                    float t2 = (-b + (float)Math.Sqrt(Disc)) / (2 * a);

                    if (t2 > 0 && t2 < 1)
                    {
                        Vector2 intersect = p1 + t2 * (p2 - p1);

                        float angle = ConvertAngle((float)Math.Atan2(intersect.Y, intersect.X));

                        if ((angle > LocalAngle1 && angle < LocalAngle2) || (LocalAngle2 < LocalAngle1 && (angle > LocalAngle1 || angle < LocalAngle2)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

        }
        */
    }
}
