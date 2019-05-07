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
        private readonly float HardFallVelocity = -300;
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
            QueueInCall,
            QueueOutCall,
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
        private bool ItemDialog;

        public float InteractTimer = 0;
        public const float InteractHold = 0.2F;

        private float CallTimer = 0;
        private const float CallDuration = 0.5F;

        private AnimatedSprite Sprite;
        private SoundEngine.Sound Sound;
        private bool StopSoundLoop;

        public Player(Vector2 position, Game1 game):base(game, new Vector2(Chunk.TileSize/2, 15.5F))
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
            Sprite.Add("open",  76, 86, 1.0, -1, 0);
            Sprite.Add("crash", 86, 94, 0.4, -1, 1);
            Sprite.Add("slide", 94, 103, 0.5, 102);
            
            Game.SoundEngine.Load("climb", "Player_Climb1", "Player_Climb2", "Player_Climb3");
            Game.SoundEngine.Load("hide", "Player_Hide_NEW");
            Game.SoundEngine.Load("jump", "Player_Jump");
            Game.SoundEngine.Load("walljump", "Player_JumpWall");
            Game.SoundEngine.Load("land", "Player_Landing");
            Game.SoundEngine.Load("run_outside", "Player_OutsideStep1", "Player_OutsideStep2", "Player_OutsideStep3");
            Game.SoundEngine.Load("run_inside", "Player_LoudStep1", "Player_LoudStep2", "Player_LoudStep3");
            Game.SoundEngine.Load("crouch_outside", "Player_QuietStep1", "Player_QuietStep2", "Player_QuietStep3");
            Game.SoundEngine.Load("crouch_inside", "Player_QuietStep1", "Player_QuietStep2", "Player_QuietStep3");
            Game.SoundEngine.Load("slide", "Player_Sliding");
            Game.SoundEngine.Load("call", "Player_WalkieTalkie");
            Game.SoundEngine.Load("win", "UI_Win");
            Game.SoundEngine.Load("die", "Enemy_Kill_NEW");
        }

        public override void Draw()
        {
            Sprite.Draw(Position+new Vector2(0, 4+ 0.5F));
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

            chunk.ForEachCollidingTile(this, (tile, pos) => {
                if (tile is TileSpike && State != PlayerState.Dying) Kill();
                else if (tile is TileGoal)
                {
                    Game.SoundEngine.Play("win");
                    Game.ShowScore();
                }
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
                                {
                                    State = PlayerState.QueueInCall;
                                    ItemDialog = true;
                                }
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
                            
                            if(entity.GetBoundingBox().Bottom <= GetBoundingBox().Bottom)
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
                                                TargetSpot = entity.Position + new Vector2(dir * (0.5F * Chunk.TileSize + 1), 0);
                                            }
                                        }
                                        else
                                        {
                                            State = PlayerState.QueueDoor;
                                            TargetSpot = entity.Position + new Vector2(dir * (0.5F * Chunk.TileSize + 1), 0);
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
                        }
                        else if (entity is ButtonPrompt)
                        {
                            Vector2 buttonPos = Game.TextEngine.TranslateToWindow(entity.Position + new Vector2(0, 36));
                            ((ButtonPrompt)entity).DrawPrompt();
                        }
                        else if (entity is DialogTrigger)
                        {
                            if (chunk.Level.NextTrigger < chunk.Level.TriggeredDialogs.Length)
                            {
                                State = PlayerState.QueueInCall;
                                ItemDialog = false;
                            }

                            chunk.Die(entity);
                        }
                        else if (entity is AlarmTrigger)
                        {
                            ((AlarmTrigger)entity).Triggered = true;
                            chunk.Level.Alarm.Detected = true;
                            chunk.Level.Alarm.Alert(Position, chunk);
                        }
                    });

                    if(State == PlayerState.QueueHide 
                       || State == PlayerState.QueueDoor
                       || State == PlayerState.QueueCrash
                       || State == PlayerState.QueueInCall)
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
                            if (right != null && (!Grounded || Controller.MoveRight))
                            {
                                MakeSound(chunk, "walljump");
                                Velocity.X = -WallJumpVelocity.X;
                                Velocity.Y = WallJumpVelocity.Y;
                                HasWallJumped = true;
                                jump = false;
                            }
                            else if (left != null && (!Grounded || Controller.MoveLeft))
                            {
                                MakeSound(chunk, "walljump");
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
                        SoundFrame = -1;
                        HasWallJumped = false;
                        if (jump)
                        {
                            IsCrouched = false;
                            MakeSound(chunk, "jump");
                            jump = false;
                            Velocity.Y = JumpSpeed;
                            LongJump = LongJumpTime * dt;
                        }
                        else if(!chunk.ChunkAlarmState && Controller.Call && !Controller.Was.Call)
                        {
                            State = PlayerState.QueueOutCall;
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
                    else
                    {
                        LongJump = 0;
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

                        Position.X = TargetSpot.X;
                        Velocity.X = 0;
                    }
                    break;
                
                case PlayerState.OpeningDoor:
                    Velocity.X = 0;
                    Velocity.Y = 0;
                    if(Sprite.Frame == 78)
                    {
                        ((Door)TargetEntity).Interact(chunk, false, TargetEntity.Position.X < Position.X);
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
                        ((Door)TargetEntity).Interact(chunk, true, TargetEntity.Position.X < Position.X);
                    }

                    if (Sprite.Frame == 6)
                    {
                        State = PlayerState.Normal;
                        TargetEntity = null;
                    }
                    break;
                case PlayerState.QueueInCall:
                    Velocity.X = 0;
                    if(Sprite.Frame == 75){
                        CallTimer -= dt;
                        if(CallTimer == float.PositiveInfinity)
                        {
                            State = PlayerState.Normal;
                            CallTimer = 0;
                            break;
                        }
                        if (CallTimer < 0)
                        {
                            if (ItemDialog)
                            {
                                chunk.Level.OpenDialogBox(chunk.StoryItems[chunk.NextItem++]);
                            }
                            else
                            {
                                chunk.Level.OpenDialogBox(chunk.Level.TriggeredDialogs[chunk.Level.NextTrigger++]);
                            }
                            
                            CallTimer = float.PositiveInfinity;
                        }
                    }
                    else
                    {
                        CallTimer = CallDuration;
                    }
                    break;
                case PlayerState.QueueOutCall:
                    Velocity.X = 0;
                    if (Sprite.Frame == 75)
                    {
                        CallTimer -= dt;
                        if (CallTimer == float.PositiveInfinity)
                        {
                            State = PlayerState.Normal;
                            CallTimer = 0;
                            break;
                        }
                        if (CallTimer < 0)
                        {
                            chunk.Level.OpenDialogBox(Level.RandomDialogs[chunk.Level.NextRandomDialog++]);

                            CallTimer = float.PositiveInfinity;
                        }
                    }
                    else
                    {
                        CallTimer = CallDuration;
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

            bool HardFall = (Velocity.Y < HardFallVelocity);
            
            // Now that all movement has been updated, check for collisions
            HandleCollisions(dt, chunk, true);
            
            if(Grounded && HardFall)
                MakeSound(chunk, "land", 80);

            // Animations
            StopSoundLoop = true;
            switch (State)
            {
                case PlayerState.QueueInCall:
                case PlayerState.QueueOutCall:
                    if (CallTimer == CallDuration)
                    {
                        Sprite.Play("call");
                        LoopSound("call");
                    }
                    break;
                case PlayerState.QueueDoor:
                case PlayerState.QueueHide:
                case PlayerState.Normal:
                    if ((Velocity.Y < 0 && (left != null || right != null)))
                    {
                        Sprite.Play("slide");
                        LoopSound("slide");
                        // Force direction to face wall
                        if (left != null) Sprite.Direction = -1;
                        if (right != null) Sprite.Direction = +1;
                    }
                    else
                    {
                        if (0 < Velocity.Y)
                        {
                            Sprite.Play("jump");
                        }
                        else if (Velocity.Y < 0)
                        {
                            Sprite.Play("fall");
                        }
                        else if (Velocity.X != 0)
                        {
                            if (IsCrouched)
                            {
                                Sprite.Play("crouchwalk");
                                if (Sprite.Frame == 58 || Sprite.Frame == 63)
                                    MakeSound(chunk, chunk.IsOutside ? "crouch_outside" : "crouch_inside" , 5);
                            }
                            else
                            {
                                Sprite.Play("run");
                                if (Sprite.Frame == 10 || Sprite.Frame == 18)
                                    MakeSound(chunk, chunk.IsOutside ? "run_outside" : "run_inside", 70);
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
                    if(Sprite.Frame == 26 || Sprite.Frame == 31)
                        MakeSound(chunk, "climb", 20);
                    
                    if (Velocity.Y < 0) Sprite.FrameStep = -1;
                    else Sprite.FrameStep = +1;
                    // Force direction to face wall
                    if (left != null) Sprite.Direction = -1;
                    if (right != null) Sprite.Direction = +1;
                    if (Velocity.Y == 0 || !Controller.Climb) Sprite.Reset();
                    break;
                case PlayerState.Hiding:
                    Sprite.Play("hide");
                    if(Sprite.Frame == 34)
                        MakeSound(chunk, "hide", 5);
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
            
            if(StopSoundLoop)
                StopSound();

            // Base direction on movement
            if (Velocity.X < 0)
                Sprite.Direction = -1;
            if (0 < Velocity.X)
                Sprite.Direction = +1;

            Game.SoundEngine.Update(Position);
        }
        
        private void MakeSound(Chunk chunk, string sound, float aiVolume=70, float volume=0.9f)
        {
            if(SoundFrame == Sprite.Frame) return;
            SoundFrame = Sprite.Frame;
            chunk.MakeSound(Game.SoundEngine.Play(sound, Position, volume), aiVolume, Position);
        }
        
        private void LoopSound(string sound, float volume=0.9f)
        {
            StopSoundLoop = false;
            if(Sound != null) return;
            Sound = Game.SoundEngine.Play(sound, Position, volume, true);
        }
        
        private void StopSound()
        {
            if(Sound == null) return;
            Sound.Stopped = true;
            Sound = null;
        }
        
        public void Kill()
        {
            if (DeathTimer <= 0)
                DeathTimer = DeathDuration;
            if(State != PlayerState.Dying)
            {
                Game.SoundEngine.Play("die", Position, 1);
                State = PlayerState.Dying;
                Controller.Vibrate(1f, 1f, 0.5f);
                if (Game.ActiveWindow is Level)
                    ((Level)Game.ActiveWindow).Camera.Shake(10, 0.5F);
            }
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

        public bool Update(int direction, int lingerCounter, Chunk targetChunk)
        {

            float dt = Game1.DeltaT;
            Controller.Update();
            Sprite.Update(dt);


            if (0 < DeathTimer)
            {
                DeathTimer -= dt;
                if (DeathTimer <= 0)
                {
                    targetChunk.Die(this);
                    Respawn(targetChunk);
                    return true;
                }
                if (Grounded)
                    Velocity.X = 0;
            }

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

            return false;
        }
    }
}
