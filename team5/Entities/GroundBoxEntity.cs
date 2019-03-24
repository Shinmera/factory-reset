using System;
using Microsoft.Xna.Framework;

namespace team5
{
    abstract class GroundBoxEntity : BoxEntity
    {
        public bool MoveRight = false;
        public bool MoveLeft = false;
        public bool Jump = false;
        public bool JumpKeyDown = false;

        public float LongJump = 0;
        public float LongJumpTime = 15*Game1.DeltaT;

        public float MaxVel = 200;
        public float AccelRate = 600;
        public float JumpSpeed = 200;
        public float LongJumpSpeed = 200;
        public static float AirFriction = 1F;
        public static float StepAirFriction = (float)Math.Pow(AirFriction, Game1.DeltaT);

        public GroundBoxEntity(Vector2 position, Game1 game, Point size) : base(game)
        {
            Position = position;
            Size = size;
        }

        public virtual void WallAction(int direction)
        {

        }

        public virtual void OnTouchGround()
        {

        }

        public override void Update(GameTime gameTime, Chunk chunk)
        {

            int direction;
            float time;

            RectangleF[] targetBB;
            Vector2[] targetVel;

            if (chunk != null)
            {
                if (MoveRight && Velocity.X < MaxVel)
                {
                    Velocity.X = Math.Min(MaxVel, Velocity.X + AccelRate * Game1.DeltaT);
                }
                if (MoveLeft && -Velocity.X < MaxVel)
                {
                    Velocity.X = Math.Max(-MaxVel, Velocity.X - AccelRate * Game1.DeltaT);
                }

                Velocity.Y += Game1.DeltaT * Game1.GRAVITY;

                float deltat = Game1.DeltaT;

                while (chunk.CollideSolid(this, deltat, out direction, out time, out targetBB, out targetVel))
                {
                    if ((direction & Chunk.Down) != 0)
                    {
                        Velocity.Y = targetVel[0].Y;
                        Position.Y = targetBB[0].Top - Size.Y;
                        OnTouchGround();
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

                    Position += Velocity * time * deltat;

                    deltat = (1 - time) * deltat;

                    if ((direction & Chunk.Down) != 0)
                    {
                        if (Jump)
                        {
                            Jump = false;
                            Velocity.Y -= JumpSpeed;
                            LongJump = LongJumpTime;
                        }
                        if (!(MoveLeft || MoveRight || Jump))
                        {
                            Velocity.X = 0;
                        }
                    }
                    if ((direction & (Chunk.Left | Chunk.Right)) != 0)
                    {
                        WallAction(direction);
                    }
                }

                if(JumpKeyDown && LongJump > 0)
                {
                    Velocity.Y -= AccelRate * Game1.DeltaT;
                }

                if(LongJump > 0)
                {
                    
                    LongJump -= Game1.DeltaT;
                    if (Velocity.Y > 0)
                    {
                        LongJump = 0;
                    }
                }

                Position += Velocity * deltat;
                Velocity = Velocity * StepAirFriction;
            }
            else
            {

            }
        }

    }
}
