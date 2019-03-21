using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    class Platform : BoxEntity
    {
        public Platform(Vector2 position, Game1 game, int width, int height) : base(game)
        {
            Position = position;
            Size = new Point(width, height);
            Texture2D dummyTexture;
            dummyTexture = new Texture2D(game.GraphicsDevice, width, height);
            Color[] colors = new Color[width * height];
            for (int i = 0; i < width * height; ++i)
            {
                colors[i] = Color.Black;
            }
            dummyTexture.SetData(colors);
            Drawer = new AnimatedSprite(dummyTexture, 1, 1, game.SpriteBatch);
        }
    }
}
