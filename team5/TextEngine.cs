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
        private struct Text
        {
            public string String;
            public SpriteFont Font;
            public Vector2 Position;
            public Color Color;
            public float Scale;

            public Text(string text, Vector2 position, Color color, SpriteFont font, float scale) {
                String = text; Position = position; Color = color; Font = font; Scale = scale;
            }
        };
        
        public enum Orientation{
            Left, Top,
            Right, Bottom,
            Center,
        };
        
        private const string DefaultFont = "welbut";
        private const float DefaultSize = 24;
        private float ViewScale = 1;
        private SpriteBatch SpriteBatch;
        private Game1 Game;
        private List<Text> QueuedText;
        private readonly RasterizerState NoCull;

        private Dictionary<string, SpriteFont> Fonts;

        public TextEngine(Game1 game)
        {
            Game = game;
            NoCull = new RasterizerState { CullMode = CullMode.None };
            QueuedText = new List<Text>();
        }

        public void LoadContent(ContentManager content)
        {
            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
            Fonts = new Dictionary<string, SpriteFont>
            {
                { DefaultFont, content.Load<SpriteFont>("Fonts/"+DefaultFont) },
                { "crashed-scoreboard", content.Load<SpriteFont>("Fonts/crashed-scoreboard") }
            };
        }

        public void QueueText(string text, Vector2 position, Color color, string fontName=DefaultFont, float sizePx=DefaultSize, Orientation horizontal=Orientation.Left, Orientation vertical=Orientation.Bottom)
        {
            Vector2 pos = new Vector2(position.X, position.Y);
            SpriteFont font = Fonts[fontName];
            Vector2 size = font.MeasureString(text);
            float scale = sizePx / size.Y * ViewScale;
            // Update position based on text size.
            switch(horizontal)
            {
                case Orientation.Left: break;
                case Orientation.Right: pos.X -= size.X*scale; break;
                case Orientation.Center: pos.X -= size.X/2*scale; break;
                default: throw new ArgumentException(String.Format("{0} is not a valid horizontal orientation.", horizontal));
            }
            switch(vertical)
            {
                case Orientation.Bottom: break;
                case Orientation.Top: pos.Y -= size.Y*scale; break;
                case Orientation.Center: pos.Y -= size.Y/2*scale; break;
                default: throw new ArgumentException(String.Format("{0} is not a valid vertical orientation.", vertical));
            }
            
            QueuedText.Add(new Text(text, pos, color, font, scale));
        }
        
        public void QueueText(string text, Vector2 position, string fontName=DefaultFont, float sizePx=DefaultSize, Orientation horizontal=Orientation.Left, Orientation vertical=Orientation.Bottom){
            QueueText(text, position, Color.Black, fontName, sizePx, horizontal, vertical);
        }
        
        public void QueueText(string text, Vector2 position, float sizePx, Orientation horizontal=Orientation.Left, Orientation vertical=Orientation.Bottom){
            QueueText(text, position, Color.Black, DefaultFont, sizePx, horizontal, vertical);
        }
        
        public void Resize(int width, int height)
        {
            ViewScale = width / 720;
        }

        public void DrawText()
        {
            SpriteBatch.Begin();

            foreach(Text text in QueuedText)
            {
                SpriteBatch.DrawString(text.Font, text.String, text.Position, text.Color, 0, Vector2.Zero, text.Scale, SpriteEffects.None, 0);
            }

            SpriteBatch.End();
            Game.GraphicsDevice.RasterizerState = NoCull;

            QueuedText.Clear();
        }
    }
}
