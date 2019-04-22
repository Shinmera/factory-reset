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

        public Pickup(Vector2 position, Game1 game) : base(game, new Vector2(Chunk.TileSize * 0.75F))
        {
            Position = position;
            Sprite = new AnimatedSprite(null, game, new Vector2(Chunk.TileSize * 0.75F));
        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = Game.TextureCache["collectible-book"];
            Sprite.Add("idle", 0, 1, 1.0);
        }

        public override void Draw()
        {
            Sprite.Draw(Position);
        }
    }
}
