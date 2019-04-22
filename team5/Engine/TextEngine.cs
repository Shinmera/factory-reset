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
            public float LineWrapSize;

            public Text(string text, Vector2 position, Color color, SpriteFont font, float scale, float lineWrapSize) {
                String = text; Position = position; Color = color; Font = font; Scale = scale; LineWrapSize = lineWrapSize;
            }
        };
        
        public enum Button : int{
            DpadL   = 0x00AF,
            DpadU   = 0x00B0,
            DpadR   = 0x00B1,
            DpadD   = 0x00B2,
            X       = 0x00B5,
            Y       = 0x00B6,
            B       = 0x00B7,
            A       = 0x00B8,
            L1      = 0x00C1,
            R1      = 0x00C2,
            L2      = 0x00C3,
            R2      = 0x00C4,
            L       = 0x00CB,
            R       = 0x00CC,
            LL      = 0x00CD,
            RL      = 0x00CE,
            LU      = 0x00CF,
            RU      = 0x00D0,
            LR      = 0x00D1,
            RR      = 0x00D2,
            LD      = 0x00D3,
            RD      = 0x00D4,
            AL      = 0x00D8,
            AU      = 0x00D9,
            AR      = 0x00DA,
            AD      = 0x00DB,
            AUL     = 0x00E7,
            AUR     = 0x00E8,
            ADR     = 0x00E9,
            ADL     = 0x00EA,
            Select  = 0x0108,
            Start   = 0x0109,
            Home    = 0x010A
        };
        
        public enum Orientation{
            Left, Top,
            Right, Bottom,
            Center,
        };
        
        private const string DefaultFont = "wellbutrin";
        private const float DefaultSize = 24;
        private float ViewScale = 1;
        private SpriteBatch SpriteBatch;
        private Game1 Game;
        private List<Text> QueuedText;
        private readonly RasterizerState NoCull;

        private static readonly char[] Whitespace = { ' ' };

        private Dictionary<string, SpriteFont> Fonts;

        public string TextWrap(string text, float sizePx, string fontName, float textwidth)
        {
            SpriteFont font = Fonts[fontName];

            string[] words = text.Split(Whitespace);

            float scaledWidth = textwidth * font.LineSpacing / sizePx;

            float spaceWidth = font.MeasureString(" ").X;
            float lineWidth = -spaceWidth;

            var lineWrappedString = new System.Text.StringBuilder();

            for (int i = 0; i < words.Length; ++i)
            {
                float wordWidth = font.MeasureString(" " + words[i]).X;
                lineWidth += wordWidth;
                if (lineWidth > scaledWidth)
                {
                    lineWidth = -spaceWidth + wordWidth;
                    lineWrappedString.Append("\n");
                }
                else
                {
                    lineWrappedString.Append(" ");
                }

                lineWrappedString.Append(words[i]);
            }

            return lineWrappedString.ToString();
        }

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
                { "promptfont", content.Load<SpriteFont>("Fonts/promptfont") }
            };
        }
        
        public void UnloadContent()
        {
            QueuedText.Clear();
            SpriteBatch.Dispose();
            foreach (var font in Fonts)
                font.Value.Texture.Dispose();
            Fonts.Clear();
        }

        public void QueueText(string text, Vector2 position, Color color, 
            string fontName=DefaultFont, float sizePx=DefaultSize, 
            Orientation horizontal=Orientation.Left, Orientation vertical=Orientation.Bottom, 
            bool lineWrapping = false, float textwidth = 0)
        {
            
            
            SpriteFont font = Fonts[fontName];

            float scale = sizePx / font.LineSpacing * ViewScale;

            if (lineWrapping)
            {
                text = TextWrap(text, sizePx, fontName, textwidth);
            }
            
            Vector2 size = font.MeasureString(text);

            Vector2 pos = position * ViewScale;

            
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
                case Orientation.Top: break;
                case Orientation.Bottom: pos.Y += size.Y*scale; break;
                case Orientation.Center: pos.Y += size.Y/2*scale; break;
                default: throw new ArgumentException(String.Format("{0} is not a valid vertical orientation.", vertical));
            }
            
            QueuedText.Add(new Text(text, pos, color, font, scale, -1));
        }
        
        public void QueueText(string text, Vector2 position, string fontName=DefaultFont, float sizePx=DefaultSize, Orientation horizontal=Orientation.Left, Orientation vertical=Orientation.Bottom){
            QueueText(text, position, Color.Black, fontName, sizePx, horizontal, vertical);
        }
        
        public void QueueText(string text, Vector2 position, float sizePx, Orientation horizontal=Orientation.Left, Orientation vertical=Orientation.Bottom){
            QueueText(text, position, Color.Black, DefaultFont, sizePx, horizontal, vertical);
        }
        
        public void QueueText(string text, Vector2 position, float sizePx, Color color, Orientation horizontal=Orientation.Left, Orientation vertical=Orientation.Bottom){
            QueueText(text, position, color, DefaultFont, sizePx, horizontal, vertical);
        }
        
        public void QueueButton(Button button, Vector2 position, Color color)
        {
            string text = Char.ConvertFromUtf32((int)button);
            QueueText(text, position, color, "promptfont", DefaultSize, Orientation.Center, Orientation.Center);
        }
        
        public void QueueButton(Button button, Vector2 position)
        {
            QueueButton(button, position, Color.Black);
        }
        
        public Vector2 TranslateToWindow(Vector2 worldPosition)
        {
            var w = Camera.TargetWidth*2;
            var h = Camera.TargetWidth*2 * 1080 / 1920;
            Vector2 window = Game.Transforms * worldPosition;
            window = (window + new Vector2(1, 1))/2;
            window.X *= w; window.Y *= h;
            return window;
        }

        public void Resize(int width, int height)
        {
            ViewScale = width / (Camera.TargetWidth*2);
        }

        public void DrawText(RectangleF Mask = default(RectangleF))
        {
            if (Mask.Width != 0)
            {
                RasterizerState _rasterizerState = new RasterizerState() { ScissorTestEnable = true };

                SpriteBatch.Begin(SpriteSortMode.Deferred, null,
                      null, null, _rasterizerState);
            }
            else
            {
                SpriteBatch.Begin();
            }

            if (Mask.Width != 0)
            {
                
                Mask.X *= ViewScale;
                Mask.Y *= ViewScale;
                Mask.Width *= ViewScale;
                Mask.Height *= ViewScale;
                
                Mask.Y = Game.GraphicsDevice.Viewport.Height - Mask.Y - Mask.Height;
                SpriteBatch.GraphicsDevice.ScissorRectangle = Mask.GetRectangle();
            }

            foreach (Text text in QueuedText)
            {
                SpriteBatch.DrawString(text.Font, text.String, new Vector2(text.Position.X, Game.GraphicsDevice.Viewport.Height - text.Position.Y), text.Color, 0, Vector2.Zero, text.Scale, SpriteEffects.None, 0);
            }

            SpriteBatch.End();
            Game.GraphicsDevice.RasterizerState = NoCull;

            QueuedText.Clear();
        }
    }
}
