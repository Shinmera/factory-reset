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

        private RectangleF BoundingBox;

        //We need to use two seperate lists for this because apparently binary search is only implemented in lists. WTF.
        private List<float> OcclusionAngles;
        private List<Tuple<Vector2,Vector2,float,float>> OcclusionValues;
        public List<Vector2> Triangles;

        private const float MaxFanAngle = (float)Math.PI / 20;

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
            OcclusionAngles = new List<float>();
            OcclusionValues = new List<Tuple<Vector2, Vector2, float, float>>();
            Triangles = new List<Vector2>();
        }

        /// <summary>Changes the position of the cone center</summary>
        public void UpdatePosition(Vector2 position)
        {
            Position = position;
            if (Math.Abs(Position.X % 1) < 0.01F || 1 - Math.Abs(Position.X % 1) < 0.01F) {
                Position.X += 0.02F;
            }
            if(Math.Abs(Position.Y % 1) < 0.01F || 1 - Math.Abs(Position.Y % 1) < 0.01F)
            {
                Position.Y += 0.02F;
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
                yvals.Add(Position.Y + Radius);
            }

            if ((Math.PI * 1.5F > LocalAngle1 && Math.PI * 1.5F < LocalAngle2) || (LocalAngle2 < LocalAngle1 && (Math.PI * 1.5F > LocalAngle1 || Math.PI * 1.5F < LocalAngle2)))
            {
                yvals.Add(Position.Y - Radius);
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

            Tuple<Vector2, Vector2, float, float> newTuple(Vector2 a, Vector2 b)
            {
                a = a - Position;
                b = b - Position;
                float dista = a.LengthSquared();
                float distb = b.LengthSquared();
                return newTuple4(a, b, dista, distb);
            }

            Tuple<Vector2, Vector2, float, float> newTuple4(Vector2 a, Vector2 b, float c, float d)
            {
                return new Tuple<Vector2, Vector2, float, float>(a, b, c, d);
            }

            OcclusionAngles.Clear();
            OcclusionValues.Clear();

            int x = (int)Math.Floor((Position.X - chunk.BoundingBox.X) / Chunk.TileSize);
            int y = (int)Math.Floor((Position.Y - chunk.BoundingBox.Y) / Chunk.TileSize);

            if (chunk.GetTile(x, y) == (uint)Chunk.Colors.BackgroundWall)
            {
                AddOcclusionValue(0, newTuple(Position, Position));
                AddOcclusionValue(ConvertAngle(Angle2 - Angle1), newTuple(Position, Position));
            }
            else
            {
                if (!ComputedBB) RecomputeBB();
                var points = chunk.BuildLOSHelper(BoundingBox, Position, Radius, Dir1, Dir2);

                bool atBeginning = true;

                Tuple<Vector2, Vector2> closestLine = null;
                Vector3 closestLineHomo = new Vector3(float.NaN);

                foreach (var point in points)
                {
                    float angle = point.Key;

                    if(angle >= 0)
                    {
                        if (angle > ConvertAngle(Angle2 - Angle1))
                        {
                            break;
                        }

                        if (atBeginning && angle >= 0)
                        {
                            atBeginning = false;

                            if (angle > 0)
                            {
                                Vector2 closestPoint = ConePoint1;
                                if (!chunk.IntersectLine(Position, Dir1, LocalRadius, out float locationDir1))
                                {
                                    locationDir1 = LocalRadius;
                                }
                                else
                                {
                                    closestPoint = Position + Dir1 * locationDir1;
                                }
                                AddOcclusionValue(0, newTuple4(closestPoint - Position, closestPoint - Position, locationDir1 * locationDir1, locationDir1 * locationDir1));
                            }
                        }

                        if (closestLine != null && point.Value.Item1 == closestLine.Item2)
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

                        if (closestLine == null)
                        {
                            Vector2 dir = point.Value.Item1 - Position;
                            dir.Normalize();
                            Vector2 CCWoffset = new Vector2(-dir.Y, dir.X) * 0.001F;

                            if (!chunk.IntersectLine(Position, dir - CCWoffset, LocalRadius, out float locationCW) || Math.Abs(locationCW - LocalRadius) < 1F)
                            {
                                locationCW = LocalRadius;
                            }
                            if (!chunk.IntersectLine(Position, dir + CCWoffset, LocalRadius, out float locationCCW) || Math.Abs(locationCCW - LocalRadius) < 1F)
                            {
                                locationCCW = LocalRadius;
                            }
                            Vector2 CW = dir * locationCW;
                            Vector2 CCW = dir * locationCCW;

                            if ((Position + CW - point.Value.Item1).LengthSquared() < 1F)
                            {
                                CW = point.Value.Item1 - Position;
                            }
                            if ((Position + CCW - point.Value.Item1).LengthSquared() < 1F) {
                                if (!float.IsNaN(point.Value.Item2.X)){
                                    CCW = point.Value.Item1 - Position;
                                    closestLine = new Tuple<Vector2, Vector2>(point.Value.Item1, point.Value.Item2);
                                    closestLineHomo = Vector3.Cross(new Vector3(closestLine.Item1, 1), new Vector3(closestLine.Item2, 1));
                                }
                            }

                            AddOcclusionValue(angle, newTuple4(CW, CCW, locationCW * locationCW, locationCCW * locationCCW));
                        }
                        else
                        {
                            if (IsCloserThanLine(closestLineHomo, point.Value.Item1, Position, out Vector2 linePoint))
                            {
                                closestLine = new Tuple<Vector2, Vector2>(point.Value.Item1, point.Value.Item2);
                                closestLineHomo = Vector3.Cross(new Vector3(closestLine.Item1, 1), new Vector3(closestLine.Item2, 1));

                                AddOcclusionValue(angle, newTuple(linePoint, point.Value.Item1));
                            }
                        }
                    }
                }

                if (atBeginning)
                {
                    Vector2 closestPoint = ConePoint1;
                    if (!chunk.IntersectLine(Position, Dir1, LocalRadius, out float locationDir1))
                    {
                        locationDir1 = LocalRadius;
                    }
                    else
                    {
                        closestPoint = Position + Dir1 * locationDir1;
                    }
                    AddOcclusionValue(0, newTuple4(closestPoint - Position, closestPoint - Position, locationDir1 * locationDir1, locationDir1 * locationDir1));
                }

                bool maxRangeEnd = true;

                Vector2 closestPointEnd = ConePoint2;

                if (chunk.IntersectLine(Position, Dir2, LocalRadius, out float distIntersect))
                {
                    closestPointEnd = Position + Dir2 * distIntersect;
                    maxRangeEnd = false;
                }

                if (maxRangeEnd)
                {
                    AddOcclusionValue(ConvertAngle(Angle2 - Angle1), newTuple4(closestPointEnd - Position, closestPointEnd - Position, LocalRadius * LocalRadius, LocalRadius * LocalRadius));

                }
                else
                {
                    AddOcclusionValue(ConvertAngle(Angle2 - Angle1), newTuple(closestPointEnd, closestPointEnd));
                }
            }

            ComputeTriangles();

            ComputedOccludedRadius = true;
        }
        
        public void ComputeTriangles()
        {
            Triangles.Clear();

            Triangles.Capacity = Math.Max((OcclusionValues.Count - 1) * 3,Triangles.Capacity);

            bool maxRange = OcclusionValues[0].Item4 == (LocalRadius * LocalRadius);
            float lastmaxRangeAngle = 0;
            Triangles.Add(new Vector2());
            Triangles.Add(OcclusionValues[0].Item2);

            for (int i = 1; i < OcclusionValues.Count - 1; ++i)
            {
                if (Math.Abs(OcclusionValues[i].Item3 - (LocalRadius * LocalRadius)) < 1)
                {
                    if (maxRange)
                    {
                        if (OcclusionAngles[i] - lastmaxRangeAngle > MaxFanAngle)
                        {
                            int newFans = (int)Math.Ceiling((OcclusionAngles[i] - lastmaxRangeAngle) / MaxFanAngle);
                            float length = (OcclusionAngles[i] - lastmaxRangeAngle) / newFans;
                            for (int fan = 1; fan < newFans; ++fan)
                            {
                                float fanAngle = fan * length + lastmaxRangeAngle + Angle1;
                                Vector2 fanVertex = new Vector2(Radius * (float)Math.Cos(fanAngle), Radius * (float)Math.Sin(fanAngle));
                                Triangles.Add(fanVertex);
                                Triangles.Add(new Vector2());
                                Triangles.Add(fanVertex);
                            }
                        }
                    }
                }
                if (OcclusionValues[i].Item4 == (LocalRadius * LocalRadius))
                {
                    maxRange = true;
                    lastmaxRangeAngle = OcclusionAngles[i];
                }
                else
                {
                    maxRange = false;
                }


                Triangles.Add(OcclusionValues[i].Item1);
                Triangles.Add(new Vector2());
                Triangles.Add(OcclusionValues[i].Item2);
            }

            if (maxRange)
            {
                if (OcclusionAngles.Last() - lastmaxRangeAngle > MaxFanAngle)
                {
                    int newFans = (int)Math.Ceiling((OcclusionAngles.Last() - lastmaxRangeAngle) / MaxFanAngle);
                    float length = (OcclusionAngles.Last() - lastmaxRangeAngle) / newFans;
                    for (int fan = 1; fan < newFans; ++fan)
                    {
                        float fanAngle = fan * length + lastmaxRangeAngle + Angle1;
                        Vector2 fanVertex = new Vector2(Radius * (float)Math.Cos(fanAngle), Radius * (float)Math.Sin(fanAngle));
                        Triangles.Add(fanVertex);
                        Triangles.Add(new Vector2());
                        Triangles.Add(fanVertex);
                    }
                }
            }

            Triangles.Add(OcclusionValues.Last().Item1);


        }

        public override void Update(Chunk chunk)
        {
            if (!ComputedOccludedRadius)
                ComputeOcclusion(chunk);

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
            if (ComputedOccludedRadius)
                Game.ViewConeEngine.DrawTriangles(Position, Triangles);
        }
    }
}
