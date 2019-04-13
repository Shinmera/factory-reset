﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    class GroundDrone : Movable, IEnemy
    {
        public enum AIState{
            Patrolling,
            Waiting,
        };
        
        private const float EdgeWaitTime = 1;
        private readonly Vector2 PatrolSpeed = new Vector2(50, 0);
        
        private AnimatedSprite Sprite;
        private float EdgeTimer = 0;
        private AIState State = AIState.Patrolling;

        private ConeEntity ViewCone;
        private SoundEngine.Sound Sound;

        public GroundDrone(Vector2 position, Game1 game) : base( game, new Vector2(Chunk.TileSize/3, Chunk.TileSize))
        {
            Position = position + new Vector2(0, Chunk.TileSize/2);
            Sprite = new AnimatedSprite(null, game, new Vector2(Chunk.TileSize * 2, Chunk.TileSize * 2));
            ViewCone = new ConeEntity(game);
            ViewCone.Radius = Chunk.TileSize * 6;
            ViewCone.FromDegrees(0, 30);
            ViewCone.UpdatePosition(Position);
            Velocity = new Vector2(PatrolSpeed.X, PatrolSpeed.Y);
        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = content.Load<Texture2D>("Textures/tempplayer");
            Sprite.Add("idle", 0, 4, 1.0);
            Sprite.Add("run", 4, 10, 0.8);
            Game.SoundEngine.Load("submarine");
            Sound = Game.SoundEngine.Play("submarine", Position);
            Sound.Loop = true;
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
            }
        }

        public override void Update(Chunk chunk)
        {
            float dt = Game1.DeltaT;

            base.Update(chunk);
            Sprite.Update(dt);
            Sound.Position = Position;

            switch(State)
            {
                case AIState.Patrolling:
                    Vector2 edgePoint = Position + new Vector2(Size.X * Sprite.Direction, -Size.Y-0.01F);
                    bool switchDirection = (chunk.CollidePoint(edgePoint) == null);

                    if(!switchDirection && chunk.CollideSolid(this, Game1.DeltaT, out int direction, out float time, out RectangleF[] targetBB, out Vector2[] targetVel))
                    {
                        if ((Sprite.Direction == 1 && (direction & Chunk.Right) != 0)
                            || Sprite.Direction == -1 && (direction & Chunk.Left) != 0)
                        {
                            switchDirection = true;
                        }
                    }

                    if (switchDirection)
                    {
                        EdgeTimer = EdgeWaitTime;
                        SetState(AIState.Waiting);
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
                    
                default:
                    SetState(AIState.Patrolling);
                    break;
            }
            
            if(Velocity.X == 0)
                Sprite.Play("idle");
            else 
                Sprite.Play("run");
                
            if(Velocity.X < 0)
                Sprite.Direction = -1;
            if(0 < Velocity.X)
                Sprite.Direction = +1;
            
            ViewCone.Direction = Sprite.Direction;

            { // Kill if touched.
                if (!chunk.Level.Player.IsHiding
                    && GetBoundingBox().Intersects(chunk.Level.Player.GetBoundingBox()))
                {
                    chunk.Level.Alarm.Detected = false;
                    chunk.Level.Player.Kill();
                }
            }
            
            Position += dt * Velocity;
            ViewCone.UpdatePosition(Position);
            ViewCone.Update(chunk);
        }

        public override void Draw()
        {
            ViewCone.Draw();
            Sprite.Draw(Position);
        }
    }
}
