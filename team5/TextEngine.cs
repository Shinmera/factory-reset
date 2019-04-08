using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace team5
{
    public class TextEngine
    {
        private SpriteFont Font;
        private SpriteBatch SpriteBatch;
        private Game1 Game;
        private List<Tuple<string, Vector2, Color, string>> QueuedText;
        private readonly RasterizerState NoCull;

        private Dictionary<string, SpriteFont> Fonts;

        public TextEngine(Game1 game)
        {
            Game = game;
            NoCull = new RasterizerState { CullMode = CullMode.None };
            QueuedText = new List<Tuple<string, Vector2, Color, string>>();
        }

        public void LoadContent(ContentManager content)
        {
            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
            Fonts = new Dictionary<string, SpriteFont>
            {
                { "Arial", content.Load<SpriteFont>("Fonts/Arial") },
                { "ArialLarge", content.Load<SpriteFont>("Fonts/ArialBoldLarge") }
            };
        }

        public void QueueText(string str, Vector2 position, Color color, string font)
        {
            QueuedText.Add(new Tuple<string,Vector2,Color, string>(str, position, color, font));
        }

        public void DrawText()
        {
            SpriteBatch.Begin();

            foreach (Tuple<string, Vector2, Color, string> text in QueuedText)
            {
                SpriteBatch.DrawString(Font, text.Item1, text.Item2, text.Item3);
            }

            SpriteBatch.End();
            Game.GraphicsDevice.RasterizerState = NoCull;
        }
    }
}
