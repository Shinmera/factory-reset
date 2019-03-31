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


        //Patrolling enemy type
        public Enemy(Vector2 position, Game1 game) : base( game, new Vector2(Chunk.TileSize/2, Chunk.TileSize))
        {
            Sprite = new AnimatedSprite(null, game, new Vector2(Chunk.TileSize * 2, Chunk.TileSize * 2));

        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = content.Load<Texture2D>("Textures/tempplayer");
            Sprite.Add("idle", 0, 4, 1.0);
            Sprite.Add("run", 0, 4, 1.0);
        }

        public override void Update(GameTime gameTime, Chunk level)
        {
            float dt = Game1.DeltaT;

            base.Update(gameTime, level);
            Sprite.Update(dt);

            Position += Velocity;
        }

        public override void Draw(GameTime gameTime)
        {
            Sprite.Draw(Position);

            if (Velocity.X > 0)
                Sprite.Direction = +1;
            else
                Sprite.Direction = -1;

        }

 


    }
}
