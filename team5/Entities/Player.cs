using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
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
        private Vector2 WallJumpVelocity = new Vector2(100, -200);
        private float WallSlideFriction = 0.9F;

        public Player(Vector2 position, Game1 game):base(game, new Vector2(Chunk.TileSize, Chunk.TileSize))
        {
            Texture2D dummyTexture;
            dummyTexture = new Texture2D(game.GraphicsDevice, (int)Size.X, (int)Size.Y);
            Color[] colors = new Color[(int)(Size.X*Size.Y)];
            for(int i = 0; i < colors.Length; ++i)
            {
                colors[i] = Color.Green;
            }
            dummyTexture.SetData(colors);
            Drawer = new AnimatedSprite(dummyTexture, 1, 1, game.SpriteBatch);

            this.Position = position;

            Controller = new Controller();
        }

        public override void Update(GameTime gameTime, Chunk chunk)
        {
            Controller.Update();
            bool Jump = Controller.Jump && JumpKeyWasUp;

            float dt = Game1.DeltaT;
            
            //// Perform movement stepping. 
            //// !! This code should never change Position !!
            // Check for neighbors
            Object down = chunk.CollidePoint(new Vector2(Position.X+Size.X/2,
                                                         Position.Y+Size.Y+1));
            Object left = chunk.CollidePoint(new Vector2(Position.X       -1,
                                                         Position.Y+Size.Y/2));
            Object right= chunk.CollidePoint(new Vector2(Position.X+Size.X+1,
                                                         Position.Y+Size.Y/2));
            
            // Apply gravity
            Velocity.Y += dt * Gravity;
            
            IsClimbing = false;
            if (down != null)
            {
                HasDoubleJumped = false;
                HasWallJumped = false;
                if (Jump)
                {
                    Jump = false;
                    Velocity.Y = -JumpSpeed;
                    LongJump = LongJumpTime*dt;
                }
            }
            if (left != null || right != null)
            {
                HasWallJumped = false;
                if(Controller.Climb)
                {
                    IsClimbing = true;
                    if(Controller.MoveUp && -ClimbSpeed < Velocity.Y)
                        Velocity.Y = -ClimbSpeed;
                    else if(Controller.MoveDown)
                        Velocity.Y = +ClimbSpeed;
                    else if(-ClimbSpeed <= Velocity.Y)
                        Velocity.Y = 0;
                }
                else if(0 < Velocity.Y)
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
            
            if(!IsClimbing || down != null){
                if(Controller.MoveRight && Velocity.X < MaxVel)
                {
                    // Allow quick turns on the ground
                    if(Velocity.X < 0 && down != null) Velocity.X = 0;
                    Velocity.X += AccelRate * dt;
                }
                else if(Controller.MoveLeft && -MaxVel < Velocity.X)
                {
                    // Allow quick turns on the ground
                    if(0 < Velocity.X && down != null) Velocity.X = 0;
                    Velocity.X -= AccelRate * dt;
                }
                else if (!Controller.MoveLeft && !Controller.MoveRight)
                {
                    // Deaccelerate in the air to accomodate wall jumps
                    if(down != null || Math.Abs(Velocity.X) < DeaccelRate*dt)
                        Velocity.X = 0;
                    else
                        Velocity.X -= Math.Sign(Velocity.X)*DeaccelRate*dt;
                }
            }
            
            // // Debug
            // if(Controller.MoveUp) Velocity.Y = -MaxVel;
            // else if(Controller.MoveDown) Velocity.Y = MaxVel;
            // else Velocity.Y = 0;

            if(Controller.Jump && 0 < LongJump)
            {
                Velocity.Y -= AccelRate * dt;
            }

            if(0 < LongJump)
            {
                LongJump -= dt;
                if (0 < Velocity.Y)
                {
                    LongJump = 0;
                }
            }

            JumpKeyWasUp = !Controller.Jump;
            
            // Now that all movement has been updated, check for collisions
            HandleCollisions(dt, chunk);
        }
    }
}
