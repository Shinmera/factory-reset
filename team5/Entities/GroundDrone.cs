using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    class GroundDrone : Movable, IEnemy
    {
        public enum AIState{
            Patrolling,
            Waiting,
            HeardSound,
        };
        
        private const float EdgeWaitTime = 1;
        private static readonly Vector2 PatrolSpeed = new Vector2(50, 0);
        private static readonly Vector2 ConeOffset = new Vector2(0, -2.5F);
        private Vector2 Spawn;

        private bool NoDirSwitch = false;

        private const float BaseVolume = 100;
        private const float ClearSensitivity = 2;
        private const float AlertSensitivity = 4;

        private bool PlayedThisCycle = false;
        private SoundEngine.Sound WalkSound;

        private AnimatedSprite Sprite;
        private AnimatedSprite AlertSignal;
        private float EdgeTimer = 0;
        private AIState State = AIState.Patrolling;

        private ConeEntity ViewCone;

        public GroundDrone(Vector2 position, Game1 game) : base( game, new Vector2(Chunk.TileSize/3, Chunk.TileSize/2))
        {
            Position = position;
            Spawn = position;
            Sprite = new AnimatedSprite(null, game, new Vector2(32, 32));
            AlertSignal = new AnimatedSprite(null, game, new Vector2(16, 16));
            ViewCone = new ConeEntity(game)
            {
                Radius = Chunk.TileSize * 6
            };
            ViewCone.FromDegrees(0, 30);
            ViewCone.UpdatePosition(Position);
            Velocity = new Vector2(PatrolSpeed.X, PatrolSpeed.Y);
        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = Game.TextureCache["ground-drone"];
            Sprite.Add("run", 0, 5, 0.5);
            Sprite.Add("idle", 5, 10, 0.5);
            AlertSignal.Texture = Game.TextureCache["alerts"];
            AlertSignal.Add("none", 20, 21, 1);
            AlertSignal.Add("noise", 0, 10, 1, -1, 0);
            AlertSignal.Add("alert", 10, 20, 1, -1, 0);

            Game.SoundEngine.Load("Enemy_DroneWalk");
            Game.SoundEngine.Load("Enemy_CamBase");
            Game.SoundEngine.Load("Enemy_Alarmed");
        }

        /// <summary>
        ///   Switches the AI state.
        /// </summary>
        public void SetState(AIState state)
        {
            State = state;
            switch(state)
            {
                case AIState.Patrolling: 
                    Velocity.X = PatrolSpeed.X * Sprite.Direction;
                    Velocity.Y = PatrolSpeed.Y * Sprite.Direction;
                    break;
                case AIState.Waiting:
                    Velocity.X = 0;
                    Velocity.Y = 0;
                    break;
                case AIState.HeardSound:
                    Velocity.X = 0;
                    Velocity.Y = 0;
                    break;
            }
        }

        public override void Update(Chunk chunk)
        {
            float dt = Game1.DeltaT;

            base.Update(chunk);
            Sprite.Update(dt);
            AlertSignal.Update(dt);

            switch(State)
            {
                case AIState.Patrolling:
                    if((chunk.CollideSolid(this, dt, out int direction, out float time, out RectangleF[] targetBB, out Vector2[] targetVel) 
                        && (Sprite.Direction == 1 && (direction & Chunk.Right) != 0)
                            || (Sprite.Direction == -1 && (direction & Chunk.Left) != 0))
                        || (chunk.CollidePoint(Position + new Vector2(Sprite.Direction*Size.X,-Size.Y - 1)) == null)
                        || !chunk.BoundingBox.Contains(Position + Vector2.UnitX*(Sprite.Direction * (Size.X + 1))))
                    {

                        EdgeTimer = EdgeWaitTime;
                        SetState(AIState.Waiting);
                        if(WalkSound != null)
                        {
                            WalkSound.Stopped = true;
                        }
                    }
                    break;
                    
                case AIState.Waiting:
                    if(EdgeTimer <= 0)
                    {
                        Sprite.Direction *= -1;
                        SetState(AIState.Patrolling);
                    }
                    else
                    {
                        EdgeTimer -= dt;
                    }
                    break;
                case AIState.HeardSound:
                    if (EdgeTimer <= 0)
                    {
                        if (!NoDirSwitch) Sprite.Direction *= -1;
                        NoDirSwitch = false;
                        SetState(AIState.Patrolling);
                        if(!chunk.ChunkAlarmState)
                            ViewCone.SetColor(ConeEntity.ClearColor);
                    }
                    else
                    {
                        EdgeTimer -= dt;
                    }
                    break;

                default:
                    SetState(AIState.Patrolling);
                    break;
            }

            if (Velocity.X == 0)
                Sprite.Play("idle");
            else
            {
                Sprite.Play("run");

                if(Sprite.Frame == 0)
                {
                    PlayedThisCycle = false;
                }

                if(Sprite.Frame == 2 && ! PlayedThisCycle)
                {
                    WalkSound = Game.SoundEngine.Play("Enemy_DroneWalk", Position, 1);
                    PlayedThisCycle = true;
                }
            }
                
            if(Velocity.X < 0)
                Sprite.Direction = -1;
            if(0 < Velocity.X)
                Sprite.Direction = +1;
            
            ViewCone.Direction = Sprite.Direction;

            { // Kill if touched.
                if (!chunk.Level.Player.IsHiding
                    && chunk.Level.Player.DeathTimer <= 0
                    && GetBoundingBox().Intersects(chunk.Level.Player.GetBoundingBox()))
                {
                    chunk.Level.Alarm.Detected = false;
                    chunk.Level.Player.Kill();
                }
            }
            
            Position += dt * Velocity;
            ViewCone.UpdatePosition(Position + ConeOffset);
            ViewCone.Update(chunk);
        }

        public override void Draw()
        {
            ViewCone.Draw();
            Sprite.Draw(Position+new Vector2(0, Size.Y));
            AlertSignal.Draw(Position+new Vector2(0, Size.Y+8));
        }

        public override void Respawn(Chunk chunk)
        {
            Position = Spawn;
        }

        public void HearSound(Vector2 position, float volume, Chunk chunk)
        {
            float sqrDist = (Position - position).LengthSquared();

            if(sqrDist > SoundEngine.AudibleDistance * SoundEngine.AudibleDistance)
            {
                return;
            }

            float dist = (float) Math.Sqrt(sqrDist);

            if (chunk.IntersectLine(position, Position - position, 1, out float temp, false))
            {
                volume /= 2;
            }

            float sensitivity = (chunk.ChunkAlarmState ? AlertSensitivity : ClearSensitivity);

            volume -= dist / sensitivity;

            if (volume > 0 && Math.Abs(position.Y - Position.Y) < Chunk.TileSize * 4 && Math.Sign(position.X - Position.X) != Sprite.Direction)
            {
                Sprite.Direction *= -1;
                EdgeTimer = EdgeWaitTime* 1.5F;
                if(State == AIState.Waiting)
                {
                    NoDirSwitch = true;
                }
                if(!chunk.ChunkAlarmState)
                    ViewCone.SetColor(ConeEntity.InspectColor);
                SetState(AIState.HeardSound);
                AlertSignal.Play("noise");
                Game.SoundEngine.Play("Enemy_Alarmed", Position, 1);
            }
        }

        public void Alert(Vector2 position, Chunk chunk)
        {
            ViewCone.SetColor(ConeEntity.AlertColor);
            AlertSignal.Play("alert");
        }

        public void ClearAlarm(Chunk chunk)
        {
            ViewCone.SetColor(ConeEntity.ClearColor);
        }
    }
}
