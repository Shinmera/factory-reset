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
    class Enemy : BoxEntity
    {
        private AnimatedSprite Sprite;
        private Vector2 Velocity;
        private float Distance;
        private float OldDistance;
        private bool  Right;

        //Patrolling enemy type
        public Enemy(Vector2 position, float distance, Game1 game) : base( game, new Vector2(Chunk.TileSize, Chunk.TileSize))
        {
            Sprite = new AnimatedSprite(null, game);

            this.Position = position;
            this.Distance = distance;

            OldDistance = Distance;

        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = content.Load<Texture2D>("Textures/tempplayer");
            Sprite.Add("idle", 0, 4, 1.0);
        }

        public override void Update(GameTime gameTime, Chunk level)
        {
            float dt = Game1.DeltaT;

            base.Update(gameTime, level);
            Sprite.Update(dt);

            Position += Velocity;

            if (Distance <= 0)
            {
                Right = true;
                Velocity.X = 1f;
            }
            else if (Distance >= OldDistance)
            {
                Right = false;
                Velocity.X = -1f;
            }
            if (Right) Distance += 1; else Distance -= 1;

        }

        public override void Draw(GameTime gameTime)
        {
            Sprite.Draw(Position - Size / 2);
        }

 


    }
}
