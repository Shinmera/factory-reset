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
        protected Vector2 Size;

        public BoxEntity(Game1 game, Vector2 size):base(game)
        {
            this.Size = size;
        }

        // <Nicolas> Does this mean Position is in the top left corner and Size is the full 
        //           width and height? Ime it's generally better to work with centered 
        //           positions and center->bound extends (or half-widths).
        public override RectangleF GetBoundingBox()
        {
            return new RectangleF(Position.X, Position.Y, Size.X+1, Size.Y+1);
        }

        public override bool Contains(Vector2 point)
        {
            return Position.X <= point.X
                && Position.Y <= point.Y
                && point.X <= Position.X + Size.X
                && point.Y <= Position.Y + Size.Y;
        }

        //Standard swept AABB
        //Source: https://www.gamedev.net/articles/programming/general-and-gameplay-programming/swept-aabb-collision-detection-and-response-r3084
        public override bool Collide(Entity source, float timestep, out int direction, out float time)
        {
            if (source is Movable)
                return CollideMovable((Movable)source, timestep, out direction, out time, GetBoundingBox());
            else if (source is BoxEntity)
            {
                // FIXME!!
                direction = 0;
                time = -1;
                return false;
            }
            else
            {
                direction = 0;
                time = -1;
                return false;
            }
        }

        public static bool CollideMovable(Movable source, float timestep, out int direction, out float time, RectangleF target)
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

        public virtual List<Vector2> GetSweptAABBPolygon(float timestep)
        {
            return GetBoundingBox().ToPolygon();
        }
    }
}
