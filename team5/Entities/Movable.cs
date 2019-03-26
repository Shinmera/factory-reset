﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    class Movable : BoxEntity
    {
        public Vector2 Velocity = new Vector2();
        protected int collided = 0;

        public Movable(Game1 game, Point size):base(game, size)
        {
        }

        protected void HandleCollisions(float dt, Chunk chunk){
            int direction;
            float time;
            RectangleF[] targetBB;
            Vector2[] targetVel;
            collided = 0;
            while (chunk.CollideSolid(this, dt, out direction, out time, out targetBB, out targetVel))
            {
                // FIXME: Change this to explicit point tests after sweep test
                //        as the collisions we get during sweeping might not be
                //        relevant at the end of collision handling.
                collided |= direction;
                
                if ((direction & Chunk.Down) != 0)
                {
                    Velocity.Y = targetVel[0].Y;
                    Position.Y = targetBB[0].Top - Size.Y;
                }
                if ((direction & Chunk.Up) != 0)
                {
                    float relVel = Velocity.Y - targetBB[0].Y;
                    Velocity.Y = targetVel[0].Y - (relVel / 3);
                    Position.Y = targetBB[0].Bottom;
                }
                if ((direction & Chunk.Left) != 0)
                {
                    Velocity.X = targetVel[1].X;
                    Position.X = targetBB[1].Right;
                }
                if ((direction & Chunk.Right) != 0)
                {
                    Velocity.X = targetVel[1].X;
                    Position.X = targetBB[1].Left - Size.X;
                }

                Position += Velocity * time * dt;

                dt = (1 - time) * dt;
            }
            Position += Velocity * dt;
        }
        
        // <Nicolas> Why is this code so complicated?
        public override List<Vector2> GetSweptAABBPolygon(float timestep)
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