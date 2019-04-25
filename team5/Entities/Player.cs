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

        private Vector2 TargetSpot;
        private Entity TargetEntity;

        public enum PlayerState
        {
            Normal,
            QueueHide,
            Hiding,
            Climbing,
            QueueDoor,
            
            OpeningDoor,
            QueueCrash,
            CrashDoor,
            Dying
        }

        private PlayerState State = PlayerState.Normal;

        public bool IsHiding { get { return State == PlayerState.Hiding; } }
        public bool IsCrouched { get; private set; }
        public float DeathTimer = 0;
        public const float DeathDuration = 2;

        public float InteractTimer = 0;
        public const float InteractHold = 0.2F;

        private AnimatedSprite Sprite;

        public Player(Vector2 position, Game1 game):base(game, new Vector2(Chunk.TileSize/2, 0.98F*Chunk.TileSize))
        {
            Sprite = new AnimatedSprite(null, game, new Vector2(32, 40));

            this.Position = position;

            Controller = new Controller();
        }
        
        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = Game.TextureCache["player"];
            Sprite.Add("idle",   0,  6, 1.0);
            Sprite.Add("run",    6, 22, 0.8);
            Sprite.Add("climb", 22, 34, 1.0);
            Sprite.Add("hide",  34, 38, 0.5, 37);
            Sprite.Add("die",   38, 46, 0.8, 45);
            Sprite.Add("jump",  46, 49, 0.5, 48);
            Sprite.Add("fall",  49, 54, 0.3, 51);
            Sprite.Add("crouch",54, 55, 1.0);
            Sprite.Add("crouchwalk", 55, 67, 1.0);
            Sprite.Add("call",  67, 76, 0.8, 75);
            // Door should play opening animation on frame 78.
            Sprite.Add("open",  76, 86, 1.0, -1, 0);
            // Door should play crash animation on frame 88.
            Sprite.Add("crash", 86, 94, 0.6, -1, 1);
            Sprite.Add("slide", 94, 103, 0.9, 102);
            
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

            if(Controller.Crouch && !Controller.Was.Crouch)
                IsCrouched = !IsCrouched;
                
            bool hide = Controller.Hide && !Controller.Was.Hide;
            bool jump = Controller.Jump && !Controller.Was.Jump;
            
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

            chunk.ForEachCollidingTile(this, (tile) => {
                if (tile is TileSpike) Kill();
                else if (tile is TileGoal) Game.ShowScore();
            });

            switch (State)
            {
                case PlayerState.Climbing:
                case PlayerState.Normal:
                    chunk.ForEachCollidingEntity(this, (entity) => {
                        if (entity is Pickup && !chunk.Level.Alarm.IsRaised)
                        {
                            Vector2 buttonPos = Game.TextEngine.TranslateToWindow(entity.Position + new Vector2(0, 36));
                            Game.TextEngine.QueueButton(TextEngine.Button.Y, buttonPos);
                            if (Controller.Interact)
                            {
                                if (chunk.NextItem < chunk.StoryItems.Length)
                                    chunk.Level.OpenDialogBox(chunk.StoryItems[chunk.NextItem++]);
                                chunk.Die(entity);
                            }
                        }
                        else if (entity is HidingSpot)
                        {
                            Vector2 buttonPos = Game.TextEngine.TranslateToWindow(entity.Position + new Vector2(0, 36));
                            Game.TextEngine.QueueButton(TextEngine.Button.Y, buttonPos);
                            if (hide)
                            {
                                TargetSpot = entity.Position;
                                hide = false;
                                State = PlayerState.QueueHide;
                            }
                        }
                        else if (entity is Door && ((Door)entity).State == Door.EState.Closed)
                        {
                            
                            float dir = Math.Sign(Position.X - entity.Position.X);
                            if (-Sprite.Direction == dir)
                            {
                                Vector2 buttonPos = Game.TextEngine.TranslateToWindow(entity.Position + new Vector2(dir * 1.1F * Chunk.TileSize, 36));
                                Game.TextEngine.QueueButton(TextEngine.Button.Y, buttonPos);
                                if (entity == TargetEntity)
                                {
                                    if (Controller.Interact && !IsCrouched)
                                    {
                                        InteractTimer -= Game1.DeltaT;
                                        if (InteractTimer <= 0)
                                        {
                                            State = PlayerState.QueueCrash;
                                            TargetSpot = entity.Position + new Vector2(dir * 1F * Chunk.TileSize, 0);
                                        }
                                    }
                                    else
                                    {
                                        State = PlayerState.QueueDoor;
                                        TargetSpot = entity.Position + new Vector2(dir * 1F * Chunk.TileSize, 0);
                                    }
                                }
                                else
                                {
                                    if (Controller.Interact)
                                    {
                                        TargetEntity = entity;
                                        InteractTimer = InteractHold;
                                    }
                                }
                            }
                        }
                    });

                    if(State == PlayerState.QueueHide || State == PlayerState.QueueDoor || State == PlayerState.QueueCrash)
                    {
                        break;
                    }

                    if (left != null || right != null || (State == PlayerState.Climbing && (leftCorner != null || rightCorner != null)))
                    {
                        HasWallJumped = false;
                        State = PlayerState.Normal;
                        if (Controller.Climb)
                        {
                            State = PlayerState.Climbing;
                            if (Controller.MoveUp && Velocity.Y < ClimbSpeed)
                                Velocity.Y = +ClimbSpeed;
                            else if (Controller.MoveDown)
                                Velocity.Y = -ClimbSpeed;
                            else if (Velocity.Y <= ClimbSpeed)
                                Velocity.Y = 0;

                            if (Velocity.X != 0)
                            {
                                State = PlayerState.Normal;
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
                        State = PlayerState.Normal;
                    }

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

                    if (State == PlayerState.Normal || Grounded)
                    {
                        float max = (IsCrouched) ? CrouchSpeed : MaxVel;
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
                        else if (IsCrouched && max < Math.Abs(Velocity.X))
                            Velocity.X = Math.Sign(Velocity.X) * max;
                    }

                    if (Controller.Jump && 0 < LongJump)
                    {
                        Velocity.Y += AccelRate * dt;
                    }


                    break;
                   
                case PlayerState.QueueHide:
                    if (Position.X < TargetSpot.X && Velocity.X < MaxVel)
                    {
                        // Allow quick turns on the ground
                        if (Velocity.X < 0 && Grounded) Velocity.X = 0;
                        Velocity.X += AccelRate * dt;
                    }
                    else if (Position.X > TargetSpot.X && -MaxVel < Velocity.X)
                    {
                        // Allow quick turns on the ground
                        if (0 < Velocity.X && Grounded) Velocity.X = 0;
                        Velocity.X -= AccelRate * dt;
                    }

                    if (Math.Abs(Position.X - TargetSpot.X) <= MaxVel * dt)
                    {
                        HasWallJumped = false;
                        State = PlayerState.Hiding;

                        Position = TargetSpot + new Vector2(0, Size.Y - Chunk.TileSize / 2);
                        Velocity.X = 0;
                    }
                    break;
                case PlayerState.Hiding:
                    Velocity.X = 0;
                    Velocity.Y = 0;
                    if (hide)
                        State = PlayerState.Normal;
                    break;
                case PlayerState.QueueDoor:
                    if (Position.X < TargetSpot.X && Velocity.X < MaxVel)
                    {
                        // Allow quick turns on the ground
                        if (Velocity.X < 0 && Grounded) Velocity.X = 0;
                        Velocity.X += AccelRate * dt;
                    }
                    else if (Position.X > TargetSpot.X && -MaxVel < Velocity.X)
                    {
                        // Allow quick turns on the ground
                        if (0 < Velocity.X && Grounded) Velocity.X = 0;
                        Velocity.X -= AccelRate * dt;
                    }

                    if (Math.Abs(Position.X - TargetSpot.X) <= MaxVel * dt)
                    {
                        HasWallJumped = false;
                        State = PlayerState.OpeningDoor;

                        Position = TargetSpot;
                        Velocity.X = 0;
                    }
                    break;
                
                case PlayerState.OpeningDoor:
                    Velocity.X = 0;
                    Velocity.Y = 0;
                    if(Sprite.Frame == 78)
                    {
                        ((Door)TargetEntity).Interact(chunk, false);
                    }

                    if(Sprite.Frame == 0)
                    {
                        State = PlayerState.Normal;
                        TargetEntity = null;
                    }
                    break;
                case PlayerState.QueueCrash:
                    if (Position.X < TargetSpot.X && Velocity.X < MaxVel)
                    {
                        // Allow quick turns on the ground
                        if (Velocity.X < 0 && Grounded) Velocity.X = 0;
                        Velocity.X += AccelRate * dt;
                    }
                    else if (Position.X > TargetSpot.X && -MaxVel < Velocity.X)
                    {
                        // Allow quick turns on the ground
                        if (0 < Velocity.X && Grounded) Velocity.X = 0;
                        Velocity.X -= AccelRate * dt;
                    }

                    if (Math.Abs(Position.X - TargetSpot.X) <= MaxVel * (dt + 0.6F/4))
                    {
                        HasWallJumped = false;
                        State = PlayerState.CrashDoor;
                        TargetSpot = TargetSpot + 999 * Vector2.UnitX * Math.Sign(TargetEntity.Position.X - Position.X);
                        Velocity.X = MaxVel * Math.Sign(TargetSpot.X-Position.X);
                    }
                    break;
                case PlayerState.CrashDoor:

                    Velocity.Y = 0;
                    if (Sprite.Frame >= 88 && Sprite.Frame <= 91)
                    {
                        if (Velocity.X < 0 && Grounded) Velocity.X = 0;
                        if (Velocity.X < MaxVel)
                            Velocity.X += Math.Sign(TargetSpot.X - Position.X) * 0.5F * AccelRate * dt;
                    }
                    else
                    {
                        if (Velocity.X < 0 && Grounded) Velocity.X = 0;
                        if(Velocity.X < MaxVel)
                            Velocity.X += Math.Sign(TargetSpot.X - Position.X) * AccelRate * dt;
                    }

                    if (Sprite.Frame == 88)
                    {
                        ((Door)TargetEntity).Interact(chunk, true);
                    }

                    if (Sprite.Frame == 6)
                    {
                        State = PlayerState.Normal;
                        TargetEntity = null;
                    }
                    break;
                case PlayerState.Dying:
                    break;
            }

            if (0 < LongJump)
            {
                LongJump -= dt;
                if (Velocity.Y < 0)
                {
                    LongJump = 0;
                }
            }

            // Now that all movement has been updated, check for collisions
            HandleCollisions(dt, chunk, true);

            // Animations

            switch (State)
            {
                case PlayerState.QueueDoor:
                case PlayerState.QueueHide:
                case PlayerState.Normal:
                    if ((Velocity.Y < 0 && (left != null || right != null)))
                    {
                        Sprite.Play("slide");
                        if (Velocity.Y < 0) Sprite.FrameStep = -1;
                        else Sprite.FrameStep = +1;
                        // Force direction to face wall
                        if (left != null) Sprite.Direction = -1;
                        if (right != null) Sprite.Direction = +1;
                        if (Velocity.Y == 0 || !Controller.Climb) Sprite.Reset();
                    }
                    else
                    {
                        if (0 < Velocity.Y)
                        {
                            if (Sprite.Frame == 46 && SoundFrame != Sprite.Frame)
                            {
                                SoundFrame = Sprite.Frame;
                                var sound = Game.SoundEngine.Play("footstep", Position, 1);
                                chunk.MakeSound(sound, 60, Position);
                            }
                            Sprite.Play("jump");
                        }
                        else if (Velocity.Y < 0)
                            Sprite.Play("fall");
                        else if (Velocity.X != 0)
                        {
                            if (IsCrouched)
                            {
                                Sprite.Play("crouchwalk");
                            }
                            else
                            {
                                Sprite.Play("run");
                                if ((Sprite.Frame == 10 || Sprite.Frame == 18) && SoundFrame != Sprite.Frame)
                                {
                                    SoundFrame = Sprite.Frame;
                                    var sound = Game.SoundEngine.Play("footstep", Position, 0.9F);
                                    chunk.MakeSound(sound, 60, Position);
                                }
                            }
                        }
                        else
                        {
                            SoundFrame = 0;
                            if (IsCrouched)
                                Sprite.Play("crouch");
                            else
                                Sprite.Play("idle");
                        }
                    }
                    break;
                case PlayerState.Climbing:
                    Sprite.Play("climb");
                    if (Velocity.Y < 0) Sprite.FrameStep = -1;
                    else Sprite.FrameStep = +1;
                    // Force direction to face wall
                    if (left != null) Sprite.Direction = -1;
                    if (right != null) Sprite.Direction = +1;
                    if (Velocity.Y == 0 || !Controller.Climb) Sprite.Reset();
                    break;
                case PlayerState.Hiding:
                    Sprite.Play("hide");
                    break;
                case PlayerState.OpeningDoor:
                    Sprite.Play("open");
                    break;
                case PlayerState.CrashDoor:
                    Sprite.Play("crash");
                    break;
                case PlayerState.Dying:
                    Sprite.Play("die");
                    break;
            }

            // Base direction on movement
            if (Velocity.X < 0)
                Sprite.Direction = -1;
            if (0 < Velocity.X)
                Sprite.Direction = +1;

            Game.SoundEngine.Update(Position);
        }
        
        public void Kill()
        {
            if(DeathTimer <= 0)
                DeathTimer = DeathDuration;
            State = PlayerState.Dying;
        }

        public override void Respawn(Chunk chunk)
        {
            State = PlayerState.Normal;
            Velocity = chunk.SpawnVelocity;
            Position = chunk.SpawnPosition;
            TargetEntity = null;

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
