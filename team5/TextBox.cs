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
    class TextBox : Container
    {
        public string Text;
        public string Font;
        public float SizePx;

        protected static Dictionary<string, string> Backgrounds = new Dictionary<string, string>
        {
            { "Debug", "dialog-box" },
            { "WalkieTalkie", "walkie_talkie_text__box" },
            { "WalkieTalkieSprite", "walkie_talkie_spriteBox" }
        };
        protected Texture2D Background;

        public int Anchor;
        public float TextOffset;
        protected Level Level { get { return (Level)Parent; } }
        protected int CurLetters;

        protected int TopPadding = 10;
        protected int LeftPadding = 10;
        protected int RightPadding = 10;
        protected int BottomPadding = 10;

        private AnimatedSprite BackgroundSprite;

        public TextBox(string text, string font, float sizePx, Game1 game, Level parent, 
            string type = "Debug", Vector2 position = new Vector2(), int anchor = 0, 
            int leftPadding = 0, int rightPadding = 0, int topPadding = 0, int bottomPadding = 0 ) : base(game, parent)
        {
            Background = game.TextureCache[Backgrounds[type]];
            Text = text;
            Font = font;
            SizePx = sizePx;

            TopPadding = topPadding;

            BackgroundSprite = new AnimatedSprite(Background, game, new Vector2(Background.Bounds.Width,Background.Bounds.Height));
            BackgroundSprite.Add("idle", 0, 1, 100);
            BackgroundSprite.Play("idle");

            Position = position;
            Anchor = anchor;
        }

        public void SetText(string text)
        {
            Text = Game.TextEngine.TextWrap(text.Trim(), SizePx, Font, Background.Width - LeftPadding - RightPadding);
            CurLetters = Text.Length;
        }

        public Vector2 GetPos()
        {
            float centerX;
            float centerY;

            if ((Anchor & Chunk.Left) != 0)
            {
                centerX = Background.Width / 2;
            }
            else if ((Anchor & Chunk.Right) != 0)
            {
                centerX = Level.Camera.GetTargetSize().X * 2 - Background.Width / 2;
            }
            else
            {
                centerX = Level.Camera.GetTargetSize().X;
            }

            if ((Anchor & Chunk.Down) != 0)
            {
                centerY = Background.Height / 2;
            }
            else if ((Anchor & Chunk.Up) != 0)
            {
                centerY = Level.Camera.GetTargetSize().Y * 2 - Background.Height / 2;
            }
            else
            {
                centerY = Level.Camera.GetTargetSize().Y;
            }

            centerX += Position.X;
            centerY += Position.Y;

            return new Vector2(centerX, centerY);
        }

        public override void Draw()
        {
            Vector2 center = GetPos();

            float scale = Game.GraphicsDevice.Viewport.Width / Level.Camera.GetTargetSize().X * 0.5F;

            Game.Transforms.PushView();
            Game.Transforms.ResetView();
            Game.Transforms.ScaleView(scale);
            Game.Transforms.TranslateView(scale * center);
            
            BackgroundSprite.Draw();
            Game.Transforms.ResetView();
            float textX = center.X - (Background.Width/2 - LeftPadding);
            float textY = center.Y + (Background.Height/2 - TopPadding);
            Game.TextEngine.QueueText(Text.Substring(0,CurLetters), new Vector2(textX, textY + TextOffset), Color.White, Font, SizePx, TextEngine.Orientation.Left, TextEngine.Orientation.Top);

            float textHeight = (Background.Height - TopPadding - BottomPadding);
            textHeight = (float)Math.Floor(textHeight / SizePx) * SizePx;

            Game.TextEngine.DrawText(new RectangleF(textX, textY - textHeight, (Background.Width - LeftPadding - RightPadding), textHeight));

            Game.Transforms.PopView();
        }

        public override void Update()
        {
            if ((!Game.Controller.Interact && Game.Controller.Was.Interact))
            {
                Level.ClosePopup();
            }
        }
    }
}
