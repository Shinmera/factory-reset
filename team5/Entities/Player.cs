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
        private bool CrouchWasUp = false;
        private bool HasWallJumped = false;
        private float LongJump = 0;
        private int SoundFrame = 0;

        private readonly float Gravity = 800;
        private readonly float MaxVel = 170;
        private readonly float AccelRate = 800;
        private readonly float DeaccelRate = 100;
        private readonly float ClimbSpeed = 70;
        private readonly float CrouchSpeed = 50;
        private readonly float JumpSpeed = 150;
        private readonly float LongJumpTime = 15;
        private readonly Vector2 WallJumpVelocity = new Vector2(200, 250);
        private readonly float WallSlideFriction = 0.9F;

        private bool QueueHide = false;
        private Vector2 HidingSpot;
        public bool IsHiding { get; private set; }
        public bool IsCrouched { get; private set; }
        public float DeathTimer = 0;
        public const float DeathDuration = 2;

        private AnimatedSprite Sprite;

        public Player(Vector2 position, Game1 game):base(game, new Vector2(Chunk.TileSize/2, 0.98F*Chunk.TileSize))
        {
            Sprite = new AnimatedSprite(null, game, new Vector2(32, 40));

            this.Position = position;

            Controller = new Controller();
        }
        
        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = content.Load<Texture2D>("Textures/player");
            Sprite.Add("idle",   0,  6, 1.0);
            Sprite.Add("run",    6, 22, 0.8);
            Sprite.Add("climb", 22, 34, 1.0);
            Sprite.Add("hide",  34, 38, 0.5, 37);
            Sprite.Add("die",   38, 46, 0.8, 45);
            Sprite.Add("jump",  46, 49, 0.5, 48);
            Sprite.Add("fall",  49, 54, 0.3, 51);
            Sprite.Add("crouch", 54, 55, 1.0);
            Sprite.Add("crouchwalk", 55, 67, 1.0);
            
            Game.SoundEngine.Load("footstep");
        }

        public override void Draw()
        {
            Sprite.Draw(Position+new Vector2(0, 4+ 0.02F * Chunk.TileSize));
        }

        public override void Update(Chunk chunk)
        {
            float dt = Game1.DeltaT;
            Controller.Update();
            Sprite.Update(dt);
            
            if(Controller.Pause)
                Game.Paused = true;

            if (0 < DeathTimer)
            {
                DeathTimer -= dt;
                if(DeathTimer <= 0)
                    chunk.Die(this);
                if(Grounded)
                    Velocity.X = 0;
            }

            if(Controller.Crouch && CrouchWasUp)
                IsCrouched = !IsCrouched;
                
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

            chunk.ForEachCollidingTile(this, (tile)=>{
                    if(tile is TileSpike) Kill();
                });

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
            else if (hide && chunk.AtHidingSpot(this, out HidingSpot))
            {
                QueueHide = true;
                IsClimbing = false;
            }
            
            if (!IsHiding && !QueueHide && DeathTimer <= 0)
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

                        if(Velocity.X != 0)
                        {
                            IsClimbing = false;
                        }

                        // Push over corners
                        if (leftCorner != null && left == null && Sprite.Direction == -1)
                        {
                            Velocity.X = -150;
                            Velocity.Y = ClimbSpeed;
                        }
                        if (rightCorner != null && right == null && Sprite.Direction == +1)
                        {
                            Velocity.X = +150;
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
                    float max = (IsCrouched)? CrouchSpeed : MaxVel;
                    if (Controller.MoveRight && Velocity.X < max)
                    {
                        // Allow quick turns on the ground
                        if (Velocity.X < 0 && Grounded) Velocity.X = 0;
                        Velocity.X += AccelRate * dt;
                    }
                    else if (Controller.MoveLeft && -max < Velocity.X)
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
                    else if(IsCrouched && max < Math.Abs(Velocity.X))
                        Velocity.X = Math.Sign(Velocity.X)*max;
                }

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
            CrouchWasUp = !Controller.Crouch;
            
            // Now that all movement has been updated, check for collisions
            HandleCollisions(dt, chunk, true);

            // Animations
            if(0 < DeathTimer)
                Sprite.Play("die");
            else
            {
                if (IsClimbing || (Velocity.Y < 0 && (left != null || right != null)))
                {
                    Sprite.Play("climb");
                    if(Velocity.Y < 0) Sprite.FrameStep = -1;
                    else               Sprite.FrameStep = +1;
                    // Force direction to face wall
                    if(left != null) Sprite.Direction = -1;
                    if(right != null) Sprite.Direction = +1;
                    if(Velocity.Y == 0 || !Controller.Climb) Sprite.Reset();
                }
                else if(IsHiding || QueueHide)
                {
                    Sprite.Play("hide");
                }
                else
                {
                    if(0 < Velocity.Y){
                        if(Sprite.Frame == 46 && SoundFrame != Sprite.Frame){
                            SoundFrame = Sprite.Frame;
                            var sound = Game.SoundEngine.Play("footstep", Position, 1);
                            chunk.MakeSound(sound, 60, Position);
                        }
                        Sprite.Play("jump");
                    }else if(Velocity.Y < 0)
                        Sprite.Play("fall");
                    else if(Velocity.X != 0){
                        if(IsCrouched)
                        {
                            Sprite.Play("crouchwalk");
                        }
                        else
                        {
                            Sprite.Play("run");
                            if((Sprite.Frame == 10 || Sprite.Frame == 18) && SoundFrame != Sprite.Frame){
                                SoundFrame = Sprite.Frame;
                                var sound = Game.SoundEngine.Play("footstep", Position, 0.9F);
                                chunk.MakeSound(sound, 60, Position);
                            }
                        }
                    }else{
                        SoundFrame = 0;
                        if(IsCrouched)
                            Sprite.Play("crouch");
                        else
                            Sprite.Play("idle");
                    }
                    // Base direction on movement
                    if (Velocity.X < 0)
                        Sprite.Direction = -1;
                    if (0 < Velocity.X)
                        Sprite.Direction = +1;
                }
            }
            
            Game.SoundEngine.Update(Position);
        }
        
        public void Kill()
        {
            if(DeathTimer <= 0)
                DeathTimer = DeathDuration;
        }

        public override void Respawn(Chunk chunk)
        {
            Velocity = chunk.SpawnVelocity;
            Position = chunk.SpawnPosition;

            if (0 < Velocity.Y)
            {
                SoundFrame = 46;
            }
        }

        public void Update(int direction, int lingerCounter, Chunk targetChunk)
        {

            float dt = Game1.DeltaT;
            Controller.Update();
            Sprite.Update(dt);

            switch (direction)
            {
                case Chunk.Left:
                    if (lingerCounter > 0)
                    {
                        if (Grounded)
                        {
                            Velocity.X = 0;
                        }
                        else
                        {
                            if(-AccelRate * 0.5F * dt > Velocity.X)
                            {
                                Velocity.X += AccelRate*0.5F * dt;
                            }
                            else
                            {
                                Velocity.X = 0;
                            }
                        }
                    }
                    else if (-MaxVel < Velocity.X)
                    {
                        // Allow quick turns on the ground
                        if (0 < Velocity.X && Grounded) Velocity.X = 0;
                        Velocity.X -= AccelRate * dt;
                    }

                    break;
                case Chunk.Right:
                    if (lingerCounter > 0)
                    {
                        if (Grounded)
                        {
                            Velocity.X = 0;
                        }
                        else
                        {
                            if (AccelRate * 0.5F * dt < Velocity.X)
                            {
                                Velocity.X -= AccelRate*0.5F * dt;
                            }
                            else
                            {
                                Velocity.X = 0;
                            }
                        }
                    }
                    else if (Velocity.X < MaxVel)
                    {
                        // Allow quick turns on the ground
                        if (Velocity.X < 0 && Grounded) Velocity.X = 0;
                        Velocity.X += AccelRate * dt;
                    }
                    break;
                case Chunk.Down:
                    break;
                case Chunk.Up:
                    break;
            }

            Velocity.Y -= dt * Gravity;

            HandleCollisions(dt, targetChunk, false);

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
        }
    }
}
