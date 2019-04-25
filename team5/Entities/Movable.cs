using System;
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
        /// <summary> The Velocity of this Entity </summary>
        public Vector2 Velocity = new Vector2();
        /// <summary> Whether this Object is touching the ground </summary>
        protected bool Grounded = false;

        public Movable(Game1 game, Vector2 size):base(game, size)
        {
        }

        /// <summary> Call to perform Swept AABB collision with sliding. Also simultaneously moves the object</summary>
        protected void HandleCollisions(float dt, Chunk chunk, bool clampX){
            int direction;
            float time;
            RectangleF[] targetBB;
            Vector2[] targetVel;
            Grounded = false;
            while (chunk.CollideSolid(this, dt, out direction, out time, out targetBB, out targetVel))
            {   
                if ((direction & Chunk.Down) != 0)
                {
                    Grounded = true;
                    Velocity.Y = targetVel[0].Y;
                    Position.Y = targetBB[0].Top + Size.Y;
                }
                if ((direction & Chunk.Up) != 0)
                {
                    float relVel = Velocity.Y - targetVel[0].Y;
                    Velocity.Y = targetVel[0].Y - (relVel / 4);
                    Position.Y = targetBB[0].Bottom - Size.Y;
                }
                if ((direction & Chunk.Left) != 0)
                {
                    Velocity.X = targetVel[1].X;
                    Position.X = targetBB[1].Right + Size.X;
                }
                if ((direction & Chunk.Right) != 0)
                {
                    Velocity.X = targetVel[1].X;
                    Position.X = targetBB[1].Left - Size.X;
                }

                if (chunk.CollidePoint(new Vector2((Position+ Velocity * time * dt).X - Size.X,
                                             (Position + Velocity * time * dt).Y)) != null)
                {
                    bool wrong = chunk.CollideSolid(this, dt, out direction, out time, out targetBB, out targetVel);
                }

                Position += Velocity * time * dt;

                dt = (1 - time) * dt;
            }
            Position += Velocity * dt;

            if (clampX)
            {
                Position.X = Math.Min(chunk.BoundingBox.Right - Size.X, Position.X);
                Position.X = Math.Max(chunk.BoundingBox.Left + Size.X, Position.X);
            }
        }
    }
}
