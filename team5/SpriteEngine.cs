using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    public class SpriteEngine
    {
        private Game1 Game;
        private SpriteBatch SpriteBatch;
        private Texture2D SolidTexture;
        
        public SpriteEngine(Game1 game)
        {
            Game = game;
        }
        
        public void LoadContent(ContentManager content)
        {
            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
            SolidTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
            SolidTexture.SetData<Color>(new Color[]{Color.White});
        }
        
        public void Draw(Texture2D texture, Rectangle source, Vector2 position)
        {
            // FIXME: We cannot use spriteBatch as it does not handle rotations or scaling
            //        as encoded in the Transforms.
            Vector2 realPos = Game.Transforms*position;
            Rectangle dest = new Rectangle((int)realPos.X, (int)realPos.Y, source.Width, source.Height);
            SpriteBatch.Begin();
            SpriteBatch.Draw(texture, dest, source, Color.White);
            SpriteBatch.End();
        }
        
        public void Draw(Rectangle rect)
        {
            Vector2 realPos = Game.Transforms*new Vector2(rect.X, rect.Y);
            Rectangle dest = new Rectangle((int)realPos.X, (int)realPos.Y, rect.Width, rect.Height);
            SpriteBatch.Begin();
            SpriteBatch.Draw(SolidTexture, dest, new Rectangle(0, 0, 1, 1), Color.White);
            SpriteBatch.End();
        }
    }
}
