
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace team5
{
    class Enemy : Movable
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

        private TempViewCone TempViewCone;

        //Patrolling enemy type
        public Enemy(Vector2 position, Game1 game) : base( game, new Vector2(Chunk.TileSize/2, Chunk.TileSize))
        {
            Position = position+ new Vector2(0,Size.Y);

            Sprite = new AnimatedSprite(null, game, new Vector2(Chunk.TileSize * 2, Chunk.TileSize * 2));

            TempViewCone = new TempViewCone(game);

            Velocity = new Vector2(PatrolSpeed.X, PatrolSpeed.Y);
        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = content.Load<Texture2D>("Textures/tempplayer");
            Sprite.Add("idle", 0, 4, 1.0);
            Sprite.Add("run", 4, 10, 0.8);
            TempViewCone.UpdatePosition(Position);
        }

        public override void Update(GameTime gameTime, Chunk chunk)
        {
            float dt = Game1.DeltaT;

            base.Update(gameTime, chunk);
            Sprite.Update(dt);

            switch(State)
            {
                case AIState.Patrolling:
                    Position += dt * Velocity;
                    
                    if (chunk.CollideSolid(this, Game1.DeltaT, out int direction, out float time, out RectangleF[] targetBB, out Vector2[] targetVel))
                    {
                        if ((Sprite.Direction == 1 && (direction & Chunk.Right) != 0)
                            || Sprite.Direction == -1 && (direction & Chunk.Left) != 0)
                        {
                            State = AIState.Waiting;
                            EdgeTimer = EdgeWaitTime;
                            Velocity.X = 0;
                        }
                    }
                    break;
                    
                case AIState.Waiting:
                    if(EdgeTimer <= 0)
                    {
                        Sprite.Direction *= -1;
                        State = AIState.Patrolling;
                        Velocity.X = PatrolSpeed.X * Sprite.Direction;
                    }
                    else
                    {
                        EdgeTimer -= dt;
                    }
                    break;
                    
                default:
                    State = AIState.Patrolling;
                    break;
            }

            TempViewCone.UpdatePosition(Position);
            
            if(Sprite.Direction == +1)
            {
                TempViewCone.faceRight();
            }
            else
            {
                TempViewCone.faceLeft();
            }

            TempViewCone.Update(gameTime, chunk);
        }

        public override void Draw(GameTime gameTime)
        {
            TempViewCone.Draw(gameTime);

            Sprite.Draw(Position);
            
            if(Velocity.X == 0)
                Sprite.Play("idle");
            else 
                Sprite.Play("run");
                
            if(Velocity.X < 0)
                Sprite.Direction = -1;
            if(0 < Velocity.X)
                Sprite.Direction = +1;
        }
    }
}
