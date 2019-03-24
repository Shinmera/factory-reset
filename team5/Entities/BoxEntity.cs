using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
    abstract class BoxEntity : Entity
    {
        protected Point Size;

        public BoxEntity(Game1 game):base(game)
        {
        }

        public override RectangleF GetBoundingBox()
        {
            return new RectangleF(Position.X, Position.Y, Size.X+1, Size.Y+1);
        }


        //Standard swept AABB
        //Source: https://www.gamedev.net/articles/programming/general-and-gameplay-programming/swept-aabb-collision-detection-and-response-r3084
        public override bool Collide(Entity source, float timestep, out int direction, out float time)
        {
            if (source is BoxEntity)
                return collideBox((BoxEntity)source, timestep, out direction, out time, GetBoundingBox());
            else
            {
                direction = -1;
                time = -1;
                return false;
            }
        }

        public static bool collideBox(BoxEntity source, float timestep, out int direction, out float time, RectangleF target)
        {
            RectangleF motionBB;

            RectangleF sourceBB = source.GetBoundingBox();
            Vector2 sourceMotion = source.Velocity * timestep;

            motionBB.X = sourceBB.X + (int)Math.Floor(Math.Min(0.0, sourceMotion.X));
            motionBB.Y = sourceBB.Y + (int)Math.Floor(Math.Min(0.0, sourceMotion.Y));
            motionBB.Width = sourceBB.Width + (int)Math.Ceiling(Math.Max(0.0, sourceMotion.X));
            motionBB.Height = sourceBB.Height + (int)Math.Ceiling(Math.Max(0.0, sourceMotion.Y));

            if (!motionBB.Intersects(target))
            {
                direction = 0;
                time = -1;
                return false;
            }

            Vector2 InvEntry;
            Vector2 InvExit;

            if (sourceMotion.X > 0.0f)
            {
                InvEntry.X = target.X - (source.Position.X + source.Size.X);
                InvExit.X = (target.X + target.Width) - source.Position.X;
            }
            else
            {
                InvEntry.X = (target.X + target.Width) - source.Position.X;
                InvExit.X = target.X - (source.Position.X + source.Size.X);
            }

            if (sourceMotion.Y > 0.0f)
            {
                InvEntry.Y = target.Y - (source.Position.Y + source.Size.Y);
                InvExit.Y = (target.Y + target.Height) - source.Position.Y;
            }
            else
            {
                InvEntry.Y = (target.Y + target.Height) - source.Position.Y;
                InvExit.Y = target.Y - (source.Position.Y + source.Size.Y);
            }

            Vector2 Entry;
            Vector2 Exit;

            if (sourceMotion.X == 0.0f)
            {
                Entry.X = float.NegativeInfinity;
                Exit.X = float.PositiveInfinity;
            }
            else
            {
                Entry.X = InvEntry.X / sourceMotion.X;
                Exit.X = InvExit.X / sourceMotion.X;
            }

            if (sourceMotion.Y == 0.0f)
            {
                Entry.Y = float.NegativeInfinity;
                Exit.Y = float.PositiveInfinity;
            }
            else
            {
                Entry.Y = InvEntry.Y / sourceMotion.Y;
                Exit.Y = InvExit.Y / sourceMotion.Y;
            }

            float entryTime = Math.Max(Entry.X, Entry.Y);
            float exitTime = Math.Min(Exit.X, Exit.Y);

            if (entryTime > exitTime || Entry.X < 0.0f && Entry.Y < 0.0f || Entry.X > 1 || Entry.Y > 1)
            {
                direction = 0;
                time = -1;
                return false;
            }
            else
            {
                // calculate normal of collided surface
                if (Entry.X > Entry.Y)
                {
                    if (sourceMotion.X < 0.0f)
                    {
                        direction = Chunk.Left;
                    }
                    else
                    {
                        direction = Chunk.Right;
                    }
                }
                else
                {
                    if (sourceMotion.Y < 0.0f)
                    {
                        direction = Chunk.Up;
                    }
                    else
                    {
                        direction = Chunk.Down;
                    }
                }

                // return the time of collision
                time = entryTime;
                return true;
            }

        }

        public List<Vector2> GetSweptAABBPolygon(float timestep)
        {
            if(Velocity.X == 0 && Velocity.Y == 0)
            {
                return GetBoundingBox().ToPolygon();
            }

            RectangleF motionBB;

            RectangleF sourceBB = GetBoundingBox();

            Vector2 Motion = Velocity * timestep;

            if (Velocity.X == 0)
            {
                var res = new List<Vector2>(4);
                if(Velocity.Y < 0)
                {
                    res.Add(new Vector2(Position.X, Position.Y + Size.Y));
                    res.Add(new Vector2(Position.X + Size.X, Position.Y + Size.Y));
                    res.Add(new Vector2(Position.X + Size.X, Position.Y + Motion.Y));
                    res.Add(new Vector2(Position.X, Position.Y + Motion.Y));
                    return res;
                }

                if(Velocity.Y > 0)
                {
                    res.Add(new Vector2(Position.X + Size.X, Position.Y));
                    res.Add(new Vector2(Position.X, Position.Y));
                    res.Add(new Vector2(Position.X, Position.Y + Motion.Y + Size.Y));
                    res.Add(new Vector2(Position.X + Size.X, Position.Y + Motion.Y + Size.Y));
                    return res;
                }

                
            }

            if (Velocity.Y == 0)
            {
                var res = new List<Vector2>(4);
                if (Velocity.X < 0)
                {
                    res.Add(new Vector2(Position.X + Size.X, Position.Y + Size.Y));
                    res.Add(new Vector2(Position.X + Size.X, Position.Y));
                    res.Add(new Vector2(Position.X + Motion.X, Position.Y));
                    res.Add(new Vector2(Position.X + Motion.X, Position.Y + Size.Y));
                }
                else
                {
                    res.Add(new Vector2(Position.X, Position.Y + Size.Y));
                    res.Add(new Vector2(Position.X, Position.Y));
                    res.Add(new Vector2(Position.X + Size.X + Motion.X, Position.Y));
                    res.Add(new Vector2(Position.X + Size.X + Motion.X, Position.Y + Size.Y));
                }

                return res;
            }

            if (Velocity.X < 0)
            {
                var res = new List<Vector2>(6);

                if (Velocity.Y < 0)
                {
                    res.Add(new Vector2(Position.X, Position.Y + Size.Y));
                    res.Add(new Vector2(Position.X + Size.X, Position.Y + Size.Y));
                    res.Add(new Vector2(Position.X + Size.X, Position.Y));
                    res.Add(new Vector2(Position.X + Size.X + Motion.X, Position.Y + Motion.Y));
                    res.Add(new Vector2(Position.X + Motion.X, Position.Y + Motion.Y));
                    res.Add(new Vector2(Position.X + Motion.X, Position.Y + Size.Y + Motion.Y));
                }
                else
                {
                    res.Add(new Vector2(Position.X + Size.X, Position.Y + Size.Y));
                    res.Add(new Vector2(Position.X + Size.X, Position.Y));
                    res.Add(new Vector2(Position.X, Position.Y));
                    res.Add(new Vector2(Position.X + Motion.X, Position.Y + Motion.Y));
                    res.Add(new Vector2(Position.X + Motion.X, Position.Y + Size.Y + Motion.Y));
                    res.Add(new Vector2(Position.X + Size.X + Motion.X, Position.Y + Size.Y + Motion.Y));
                }

                return res;
            }

            if (Velocity.X > 0)
            {
                var res = new List<Vector2>(6);

                if (Velocity.Y < 0)
                {
                    res.Add(new Vector2(Position.X, Position.Y));
                    res.Add(new Vector2(Position.X, Position.Y + Size.Y));
                    res.Add(new Vector2(Position.X + Size.X, Position.Y + Size.Y));
                    res.Add(new Vector2(Position.X + Size.X + Motion.X, Position.Y + Size.Y + Motion.Y));
                    res.Add(new Vector2(Position.X + Size.X + Motion.X, Position.Y + Motion.Y));
                    res.Add(new Vector2(Position.X + Motion.X, Position.Y + Motion.Y));
                }
                else
                {
                    res.Add(new Vector2(Position.X + Size.X, Position.Y));
                    res.Add(new Vector2(Position.X, Position.Y));
                    res.Add(new Vector2(Position.X, Position.Y + Size.Y));
                    res.Add(new Vector2(Position.X + Motion.X, Position.Y + Size.Y + Motion.Y));
                    res.Add(new Vector2(Position.X + Size.X + Motion.X, Position.Y + Size.Y + Motion.Y));
                    res.Add(new Vector2(Position.X + Size.X + Motion.X, Position.Y + Motion.Y));
                }

                return res;
            }

            throw new ArithmeticException("Velocity of Swept AABB invalid");
        }

    }
}
