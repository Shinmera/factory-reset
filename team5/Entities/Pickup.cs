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

        private float Phase = 0;
        private const float PhaseRate = (float)(Math.PI);
        private float CurOffset = 0;
        private const float MaxOffset = 2.5F;

        public Pickup(Vector2 position, Game1 game) : base(game, new Vector2(Chunk.TileSize * 0.75F))
        {
            Position = position;
            Sprite = new AnimatedSprite(null, game, new Vector2(Chunk.TileSize));
            Phase = (float)(2*Math.PI*game.RNG.NextDouble());
        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = Game.TextureCache["collectible-book"];
            Sprite.Add("idle", 0, 1, 1.0);
        }

        public override void Update(Chunk chunk)
        {
            Phase += Game1.DeltaT * PhaseRate;
            Phase %= (float)(2 * Math.PI);
            CurOffset = (float) Math.Sin(Phase) * MaxOffset;
        }

        public override void Draw()
        {
            Sprite.Draw(Position + Vector2.UnitY * CurOffset);
        }
    }
}
