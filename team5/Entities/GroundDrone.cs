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
        };
        
        private const float EdgeWaitTime = 1;
        private readonly Vector2 PatrolSpeed = new Vector2(50, 0);
        
        private AnimatedSprite Sprite;
        private float EdgeTimer = 0;
        private AIState State = AIState.Patrolling;

        private ConeEntity ViewCone;

        public GroundDrone(Vector2 position, Game1 game) : base( game, new Vector2(Chunk.TileSize/2, Chunk.TileSize))
        {
            Position = position;
            Sprite = new AnimatedSprite(null, game, new Vector2(Chunk.TileSize * 2, Chunk.TileSize * 2));
            ViewCone = new ConeEntity(game);
            ViewCone.Radius = Chunk.TileSize * 6;
            ViewCone.FromDegrees(0, 30);
            Velocity = new Vector2(PatrolSpeed.X, PatrolSpeed.Y);
        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = content.Load<Texture2D>("Textures/tempplayer");
            Sprite.Add("idle", 0, 4, 1.0);
            Sprite.Add("run", 4, 10, 0.8);
        }
        
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

        public override void Update(GameTime gameTime, Chunk chunk)
        {
            float dt = Game1.DeltaT;

            base.Update(gameTime, chunk);
            Sprite.Update(dt);

            switch(State)
            {
                case AIState.Patrolling:
                    if (chunk.CollideSolid(this, Game1.DeltaT, out int direction, out float time, out RectangleF[] targetBB, out Vector2[] targetVel))
                    {
                        if ((Sprite.Direction == 1 && (direction & Chunk.Right) != 0)
                            || Sprite.Direction == -1 && (direction & Chunk.Left) != 0)
                        {
                            EdgeTimer = EdgeWaitTime;
                            SetState(AIState.Waiting);
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
            
            Position += dt * Velocity;
            ViewCone.UpdatePosition(Position);
            ViewCone.Update(gameTime, chunk);
        }

        public override void Draw(GameTime gameTime)
        {
            ViewCone.Draw(gameTime);
            Sprite.Draw(Position);
        }
    }
}
