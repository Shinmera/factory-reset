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
    class Pickup : BoxEntity
    {
        private AnimatedSprite Sprite;

        public Pickup(Game1 game, Vector2 position) : base(game, new Vector2(Chunk.TileSize * 0.75F))
        {
            Position = position;

            Sprite = new AnimatedSprite(null, game, new Vector2(Chunk.TileSize * 0.75F));
        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = content.Load<Texture2D>("Textures/pickup");
            Sprite.Add("idle", 0, 1, 1.0);
        }

        public override void Update(GameTime gameTime, Chunk chunk)
        {
            base.Update(gameTime, chunk);


            if (Collide(chunk.Level.Player, Game1.DeltaT, out int dirction, out float time, out bool corner))
            {
                ++chunk.Level.collected;
                chunk.Die(this);
            }
        }

        public override void Draw(GameTime gameTime)
        {

            Sprite.Draw(Position);

        }
    }
}
