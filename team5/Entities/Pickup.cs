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
            Sprite.Texture = content.Load<Texture2D>("Textures/collectible-book");
            Sprite.Add("idle", 0, 1, 1.0);
        }

        public override void Update(Chunk chunk)
        {
            base.Update(chunk);

            if (Collide(chunk.Level.Player, Game1.DeltaT, out int dirction, out float time, out bool corner))
            {
                if (chunk.NextItem < chunk.StoryItems.Length)
                {
                    chunk.Level.OpenDialogBox(chunk.StoryItems[chunk.NextItem++]);
                    chunk.Die(this);
                }
            }
        }

        public override void Draw()
        {

            Sprite.Draw(Position);

        }
    }
}
