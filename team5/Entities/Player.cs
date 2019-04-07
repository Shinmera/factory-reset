using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    class Player : Movable
    {
        const bool CanRepeatWallJump = false;
        const bool CanDoubleJump = false;

        public bool FallThrough => Controller.MoveDown;

        private Controller Controller;
        private bool IsClimbing = false;
        private bool JumpKeyWasUp = false;
        private bool HideKeyWasUp = false;
        private bool HasWallJumped = false;
        private float LongJump = 0;

        private readonly float Gravity = 800;
        private readonly float MaxVel = 150;
        private readonly float AccelRate = 800;
        private readonly float DeaccelRate = 100;
        private readonly float ClimbSpeed = 70;
        private readonly float JumpSpeed = 150;
        private readonly float LongJumpTime = 15;
        private readonly Vector2 WallJumpVelocity = new Vector2(200, 200);
        private readonly float WallSlideFriction = 0.9F;

        private bool QueueHide = false;
        private Vector2 HidingSpot;
        public bool IsHiding { get; private set; }

        private AnimatedSprite Sprite;

        public Player(Vector2 position, Game1 game):base(game, new Vector2(Chunk.TileSize/2, Chunk.TileSize))
        {
            Sprite = new AnimatedSprite(null, game, new Vector2(Chunk.TileSize*2, Chunk.TileSize*2));

            this.Position = position;

            Controller = new Controller();
        }
        
        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = content.Load<Texture2D>("Textures/tempplayer");
            Sprite.Add("idle",   0,  4, 1.0);
            Sprite.Add("run",    4, 10, 0.8);
            Sprite.Add("jump",  14,  6, 0.5, 5);
            Sprite.Add("fall",  20,  4, 0.5, 3);
            Sprite.Add("climb", 24,  4, 0.5);
            Sprite.Add("die",   28,  5, 0.5, 4);
            Sprite.Add("revive",33,  7, 1.0);
        }

        public override void Draw(GameTime gameTime)
        {
            Sprite.Draw(Position);
        }

        public override void Update(GameTime gameTime, Chunk chunk)
        {
            if(Controller.Quit)
                Game.Exit();
            
            float dt = Game1.DeltaT;

            Controller.Update();
            Sprite.Update(dt);

            bool hide = Controller.Hide && HideKeyWasUp;
            bool jump = Controller.Jump && JumpKeyWasUp;
            
            //// Perform movement stepping. 
            //// !! This code should never change Position !!
            // Check for neighbors
            Object down = chunk.CollidePoint(new Vector2(Position.X,
                                                         Position.Y-Size.Y-1));
            Object left = chunk.CollidePoint(new Vector2(Position.X-Size.X-1,
                                                         Position.Y));
            Object right= chunk.CollidePoint(new Vector2(Position.X+Size.X+1,
                                                         Position.Y));
            Object leftCorner = chunk.CollidePoint(new Vector2(Position.X-Size.X-1,
                                                               Position.Y-Size.Y-1));
            Object rightCorner= chunk.CollidePoint(new Vector2(Position.X+Size.X+1,
                                                               Position.Y-Size.Y-1));
            
            // Apply gravity
            Velocity.Y -= dt * Gravity;

            List<TileType> touchingTiles = chunk.TouchingNonSolidTile(this);

            foreach(var tile in touchingTiles)
            {
                if(tile is TileSpike)
                {
                    chunk.Die(this);
                    return;
                }
            }

            if (IsHiding || QueueHide)
            {
                if (IsHiding)
                {
                    Grounded = true;
                }

                if (jump || hide || (HidingSpot + new Vector2(0, Size.Y - Chunk.TileSize / 2) - Position).Length() > Chunk.TileSize * 1.5)
                {
                    IsHiding = false;
                    QueueHide = false;
                }
            }
            else
            {
                if (hide)
                {
                    if (chunk.AtHidingSpot(this, out HidingSpot))
                    {
                        QueueHide = true;
                        IsClimbing = false;
                    }
                }
            }

            if (!IsHiding && !QueueHide)
            {
                if (Grounded)
                {
                    HasWallJumped = false;
                    if (jump)
                    {
                        jump = false;
                        Velocity.Y = JumpSpeed;
                        LongJump = LongJumpTime * dt;
                    }
                }
                if (left != null || right != null || (IsClimbing && (leftCorner != null || rightCorner != null)))
                {
                    HasWallJumped = false;
                    IsClimbing = false;
                    if (Controller.Climb)
                    {
                        IsClimbing = true;
                        if (Controller.MoveUp && Velocity.Y < ClimbSpeed)
                            Velocity.Y = +ClimbSpeed;
                        else if (Controller.MoveDown)
                            Velocity.Y = -ClimbSpeed;
                        else if (Velocity.Y <= ClimbSpeed)
                            Velocity.Y = 0;

                        // Push over corners
                        if (leftCorner != null && left == null && Sprite.Direction == -1)
                        {
                            Velocity.X = -50;
                            Velocity.Y = ClimbSpeed;
                        }
                        if (rightCorner != null && right == null && Sprite.Direction == +1)
                        {
                            Velocity.X = +50;
                            Velocity.Y = ClimbSpeed;
                        }
                    }
                    else if (Velocity.Y < 0)
                        Velocity.Y *= WallSlideFriction;

                    if (jump && (!HasWallJumped || CanRepeatWallJump))
                    {
                        if (right != null)
                        {
                            Velocity.X = -WallJumpVelocity.X;
                            Velocity.Y = WallJumpVelocity.Y;
                            HasWallJumped = true;
                            jump = false;
                        }
                        else if (left != null)
                        {
                            Velocity.X = WallJumpVelocity.X;
                            Velocity.Y = WallJumpVelocity.Y;
                            HasWallJumped = true;
                            jump = false;
                        }
                    }
                }
                else
                {
                    IsClimbing = false;
                }

                if (!IsClimbing || Grounded)
                {
                    if (Controller.MoveRight && Velocity.X < MaxVel)
                    {
                        // Allow quick turns on the ground
                        if (Velocity.X < 0 && Grounded) Velocity.X = 0;
                        Velocity.X += AccelRate * dt;
                    }
                    else if (Controller.MoveLeft && -MaxVel < Velocity.X)
                    {
                        // Allow quick turns on the ground
                        if (0 < Velocity.X && Grounded) Velocity.X = 0;
                        Velocity.X -= AccelRate * dt;
                    }
                    else if (!Controller.MoveLeft && !Controller.MoveRight)
                    {
                        // Deaccelerate in the air to accomodate wall jumps
                        if (Grounded || Math.Abs(Velocity.X) < DeaccelRate * dt)
                            Velocity.X = 0;
                        else
                            Velocity.X -= Math.Sign(Velocity.X) * DeaccelRate * dt;
                    }
                }

                // // Debug
                // if(Controller.MoveUp) Velocity.Y = +MaxVel;
                // else if(Controller.MoveDown) Velocity.Y = -MaxVel;
                // else Velocity.Y = 0;

                if (Controller.Jump && 0 < LongJump)
                {
                    Velocity.Y += AccelRate * dt;
                }

                if (0 < LongJump)
                {
                    LongJump -= dt;
                    if (Velocity.Y < 0)
                    {
                        LongJump = 0;
                    }
                }
            }

            if (QueueHide)
            {
                if (Position.X < HidingSpot.X && Velocity.X < MaxVel)
                {
                    // Allow quick turns on the ground
                    if (Velocity.X < 0 && Grounded) Velocity.X = 0;
                    Velocity.X += AccelRate * dt;
                }
                else if (Position.X > HidingSpot.X && -MaxVel < Velocity.X)
                {
                    // Allow quick turns on the ground
                    if (0 < Velocity.X && Grounded) Velocity.X = 0;
                    Velocity.X -= AccelRate * dt;
                }

                if (Math.Abs(Position.X - HidingSpot.X) <= MaxVel * dt)
                {
                    HasWallJumped = false;
                    QueueHide = false;

                    IsHiding = true;

                    Position = HidingSpot + new Vector2(0, Size.Y - Chunk.TileSize / 2);
                }
            }

            if (IsHiding)
            {
                Velocity.X = 0;
                Velocity.Y = 0;
            }
            HideKeyWasUp = !Controller.Hide;
            JumpKeyWasUp = !Controller.Jump;
            
            // Now that all movement has been updated, check for collisions
            HandleCollisions(dt, chunk);

            

            // Animations
            if(IsClimbing)
            {
                Sprite.Play("climb");
                // Force direction to face wall
                if(left != null) Sprite.Direction = -1;
                if(right != null) Sprite.Direction = +1;
                if(Velocity.Y == 0) Sprite.Reset();
            }
            else
            {
                if(0 < Velocity.Y)
                    Sprite.Play("jump");
                else if(Velocity.Y < 0)
                    Sprite.Play("fall");
                else if(Velocity.X != 0)
                    Sprite.Play("run");
                else
                    Sprite.Play("idle");
                // Base direction on movement
                if (Velocity.X < 0)
                    Sprite.Direction = -1;
                if (0 < Velocity.X)
                    Sprite.Direction = +1;
            }
        }

        public override void Respawn(Chunk chunk)
        {
            Velocity = new Vector2(0);
            Position = chunk.SpawnPosition;
        }

        public void Update(GameTime gameTime, int direction)
        {
            if (Controller.Quit)
                Game.Exit();

            float dt = Game1.DeltaT;

            Controller.Update();
            Sprite.Update(dt);

            switch (direction)
            {
                case Chunk.Left:
                    Velocity.Y = 0;
                    if (-MaxVel < Velocity.X)
                    {
                        // Allow quick turns on the ground
                        if (0 < Velocity.X && Grounded) Velocity.X = 0;
                        Velocity.X -= AccelRate * dt;
                    }
                    break;
                case Chunk.Right:
                    Velocity.Y = 0;
                    if (Velocity.X < MaxVel)
                    {
                        // Allow quick turns on the ground
                        if (Velocity.X < 0 && Grounded) Velocity.X = 0;
                        Velocity.X += AccelRate * dt;
                    }
                    break;
                case Chunk.Down:
                    Velocity.Y -= dt * Gravity;
                    break;
                case Chunk.Up:
                    break;
            }
            if (0 < Velocity.Y)
                Sprite.Play("jump");
            else if (Velocity.Y < 0)
                Sprite.Play("fall");
            else if (Velocity.X != 0)
                Sprite.Play("run");
            else
                Sprite.Play("idle");
            // Base direction on movement
            if (Velocity.X < 0)
                Sprite.Direction = -1;
            if (0 < Velocity.X)
                Sprite.Direction = +1;

            Position += Velocity * dt;
        }
    }
}
