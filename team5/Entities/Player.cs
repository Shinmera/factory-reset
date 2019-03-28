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
        private bool HasWallJumped = false;
        private bool HasDoubleJumped = false;
        private float LongJump = 0;

        private float Gravity = 800;
        private float MaxVel = 150;
        private float AccelRate = 800;
        private float DeaccelRate = 100;
        private float ClimbSpeed = 70;
        private float JumpSpeed = 150;
        private float LongJumpSpeed = 250;
        private float LongJumpTime = 15;
        private Vector2 WallJumpVelocity = new Vector2(100, 200);
        private float WallSlideFriction = 0.9F;
        
        private AnimatedSprite Sprite;

        public Player(Vector2 position, Game1 game):base(game, new Vector2(Chunk.TileSize, Chunk.TileSize))
        {
            Sprite = new AnimatedSprite(null, game, Size);

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
            Sprite.Draw(Position - Size/2);
        }

        public override void Update(GameTime gameTime, Chunk chunk)
        {
            float dt = Game1.DeltaT;

            Controller.Update();
            Sprite.Update(dt);

            bool Jump = Controller.Jump && JumpKeyWasUp;
            
            //// Perform movement stepping. 
            //// !! This code should never change Position !!
            // Check for neighbors
            Object down = chunk.CollidePoint(new Vector2(Position.X,
                                                         Position.Y-Size.Y/2+1));
            Object left = chunk.CollidePoint(new Vector2(Position.X-Size.X/2-1,
                                                         Position.Y));
            Object right= chunk.CollidePoint(new Vector2(Position.X+Size.X/2+1,
                                                         Position.Y));
            
            // Apply gravity
            Velocity.Y -= dt * Gravity;
            
            IsClimbing = false;
            if (Grounded)
            {
                HasDoubleJumped = false;
                HasWallJumped = false;
                if (Jump)
                {
                    Jump = false;
                    Velocity.Y = JumpSpeed;
                    LongJump = LongJumpTime*dt;
                }
            }
            if (left != null || right != null)
            {
                HasWallJumped = false;
                if(Controller.Climb)
                {
                    IsClimbing = true;
                    if(Controller.MoveUp && Velocity.Y < ClimbSpeed)
                        Velocity.Y = +ClimbSpeed;
                    else if(Controller.MoveDown)
                        Velocity.Y = -ClimbSpeed;
                    else if(Velocity.Y <= ClimbSpeed)
                        Velocity.Y = 0;
                }
                else if(Velocity.Y < 0)
                    Velocity.Y *= WallSlideFriction;

                if (Jump && (!HasWallJumped || CanRepeatWallJump))
                {
                    if (right != null)
                    {
                        Velocity.X = (Controller.MoveRight)? 0 : -WallJumpVelocity.X;
                        Velocity.Y = WallJumpVelocity.Y;
                        HasWallJumped = true;
                        Jump = false;
                    }
                    else if (left != null)
                    {
                        Velocity.X = (Controller.MoveLeft)? 0 : WallJumpVelocity.X;
                        Velocity.Y = WallJumpVelocity.Y;
                        HasWallJumped = true;
                        Jump = false;
                    }
                }
            }
            
            if(!IsClimbing || Grounded){
                if(Controller.MoveRight && Velocity.X < MaxVel)
                {
                    // Allow quick turns on the ground
                    if(Velocity.X < 0 && Grounded) Velocity.X = 0;
                    Velocity.X += AccelRate * dt;
                }
                else if(Controller.MoveLeft && -MaxVel < Velocity.X)
                {
                    // Allow quick turns on the ground
                    if(0 < Velocity.X && Grounded) Velocity.X = 0;
                    Velocity.X -= AccelRate * dt;
                }
                else if (!Controller.MoveLeft && !Controller.MoveRight)
                {
                    // Deaccelerate in the air to accomodate wall jumps
                    if(Grounded || Math.Abs(Velocity.X) < DeaccelRate*dt)
                        Velocity.X = 0;
                    else
                        Velocity.X -= Math.Sign(Velocity.X)*DeaccelRate*dt;
                }
            }
            
            // // Debug
            // if(Controller.MoveUp) Velocity.Y = +MaxVel;
            // else if(Controller.MoveDown) Velocity.Y = -MaxVel;
            // else Velocity.Y = 0;

            if(Controller.Jump && 0 < LongJump)
            {
                Velocity.Y += AccelRate * dt;
            }

            if(0 < LongJump)
            {
                LongJump -= dt;
                if (Velocity.Y < 0)
                {
                    LongJump = 0;
                }
            }

            JumpKeyWasUp = !Controller.Jump;
            
            // Now that all movement has been updated, check for collisions
            HandleCollisions(dt, chunk);
            
            // Animations
            if(IsClimbing)
            {
                Sprite.Play("climb");
                // Force direction to face wall
                Sprite.Direction = (left != null)? -1 : +1;
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
    }
}
