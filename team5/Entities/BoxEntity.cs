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
        /// <summary>
        ///   This is the /half-size/, meaning the width and height from the center of
        ///   the box to its edges.
        /// </summary>
        protected Vector2 Size;

        public BoxEntity(Game1 game, Vector2 size):base(game)
        {
            this.Size = size;
        }

        /// <summary>
        ///   Returns a Bounding Box over the Entity, which in this case is equal to the collision box of the Entity.
        /// </summary>
        public override RectangleF GetBoundingBox()
        {
            return new RectangleF(Position.X - Size.X, Position.Y - Size.Y, Size.X*2, Size.Y*2);
        }

        public override bool Contains(Vector2 point)
        {
            return Position.X - Size.X <= point.X
                && Position.Y - Size.Y <= point.Y
                && point.X <= Position.X + Size.X
                && point.Y <= Position.Y + Size.Y;
        }

        //Standard swept AABB
        //Source: https://www.gamedev.net/articles/programming/general-and-gameplay-programming/swept-aabb-collision-detection-and-response-r3084
        public override bool Collide(Entity source, float timestep, out int direction, out float time, out bool corner)
        {
            if (source is Movable)
                return CollideMovable((Movable)source, GetBoundingBox(), timestep, out direction, out time, out corner);
            else if (source is BoxEntity)
            {
                // FIXME!!
                corner = false;
                direction = 0;
                time = -1;
                return false;
            }
            else
            {
                corner = false;
                direction = 0;
                time = -1;
                return false;
            }
        }

        /// <summary>
        ///   Tests collision from a given entity to an arbitrary Box. Swept AABB will be performed with a moving source onto the target.
        /// </summary>
        /// <param name="source">The entity with which to test collsion</param>
        /// <param name="target">The box with which to test collision.</param>
        /// <param name="timestep">A copy of the timestep size</param>
        /// <param name="direction">Returns the direction of collisions. Mostly used by Box Entities, will typically be zero otherwise.</param>
        /// <param name="time">For moving Entities, returns at which point in time the collision occured. Specified in multiples of the timestep (will always be in [0,1])</param>
        /// <param name="corner">In the case of Box entities, returns true iff the collision was exactly on a corner. Used to allow smooth movement across multiple boxes.</param>
        public static bool CollideMovable(Movable source, RectangleF target, float timestep, out int direction, out float time, out bool corner)
        {
            corner = false;

            RectangleF motionBB;

            RectangleF sourceBB = source.GetBoundingBox();
            Vector2 sourceMotion = source.Velocity * timestep;

            motionBB.X = sourceBB.X + (float)Math.Min(0.0, sourceMotion.X);
            motionBB.Y = sourceBB.Y + (float)Math.Min(0.0, sourceMotion.Y);
            motionBB.Width = sourceBB.Width + (float)Math.Abs(sourceMotion.X);
            motionBB.Height = sourceBB.Height + (float)Math.Abs(sourceMotion.Y);

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
                InvEntry.X = target.Left - sourceBB.Right;
                InvExit.X = target.Right - sourceBB.Left;
            }
            else
            {
                InvEntry.X = target.Right - sourceBB.Left;
                InvExit.X = target.Left - sourceBB.Right;
            }

            if (sourceMotion.Y > 0.0f)
            {
                InvEntry.Y = target.Bottom - sourceBB.Top;
                InvExit.Y = target.Top - sourceBB.Bottom;
            }
            else
            {
                InvEntry.Y = target.Top - sourceBB.Bottom;
                InvExit.Y = target.Bottom - sourceBB.Top;
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
                else if(Entry.X < Entry.Y)
                {
                    if (sourceMotion.Y < 0.0f)
                    {
                        direction = Chunk.Down;
                    }
                    else
                    {
                        direction = Chunk.Up;
                    }
                }
                else
                {
                    corner = true;
                    if(Math.Abs(sourceMotion.X) <= Math.Abs(sourceMotion.Y))
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
                            direction = Chunk.Down;
                        }
                        else
                        {
                            direction = Chunk.Up;
                        }
                    }
                }


                // return the time of collision
                time = entryTime;
                return true;
            }
        }

    }
}
