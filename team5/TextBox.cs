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

        protected static Dictionary<string,Texture2D> Backgrounds;
        protected Texture2D Background;

        public int Anchor;
        private bool PauseKeyWasUp = false;
        private bool EnterKeyWasUp = false;
        private Level Level { get { return (Level)Parent; } }

        protected int TopPadding = 10;
        protected int LeftPadding = 10;
        protected int RightPadding = 10;
        protected int BottomPadding = 10;

        private AnimatedSprite BackgroundSprite;

        public TextBox(string text, string font, float sizePx, Game1 game, Level parent, 
            string type = "Debug", Vector2 position = new Vector2(), int anchor = 0, 
            int leftPadding = 0, int rightPadding = 0, int topPadding = 0, int bottomPadding = 0 ) : base(game, parent)
        {
            Background = Backgrounds[type];
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

        public void Initialize()
        {
            Text = Game.TextEngine.TextWrap(Text, SizePx, Font, Background.Width - LeftPadding - RightPadding);
        }

        public static void LoadStaticContent(ContentManager content)
        {
            Backgrounds = new Dictionary<string, Texture2D>
            {
                { "Debug", content.Load<Texture2D>("Textures/dialog-box") },
                { "WalkieTalkie", content.Load<Texture2D>("Textures/walkie_talkie_text__box") },
                { "WalkieTalkieSprite", content.Load<Texture2D>("Textures/walkie_talkie_spriteBox") }
            };
        }

        public override void Draw()
        {
            float centerX;
            float centerY;
            float scale = Game.GraphicsDevice.Viewport.Width / Level.Camera.GetTargetSize().X * 0.5F;
            if ((Anchor & Chunk.Left) != 0)
            {
                centerX = scale * Background.Width/2;
            }else if ((Anchor & Chunk.Right) != 0) { 
                centerX = Game.GraphicsDevice.Viewport.Width - scale * Background.Width/2;
            }else{
                centerX = Game.GraphicsDevice.Viewport.Width / 2;
            }

            if ((Anchor & Chunk.Down) != 0)
            {
                centerY = scale * Background.Height / 2;
            }
            else if ((Anchor & Chunk.Up) != 0)
            {
                centerY = Game.GraphicsDevice.Viewport.Height - scale * Background.Height / 2;
            }
            else
            {
                centerY = Game.GraphicsDevice.Viewport.Height / 2;
            }

            centerX += Position.X;
            centerY += Position.Y;

            Game.Transforms.Push();
            Game.Transforms.Reset();
            
            
            Game.Transforms.Pop();

            Game.Transforms.PushView();
            Game.Transforms.ResetView();
            Game.Transforms.ScaleView(scale);
            Game.Transforms.TranslateView(new Vector2(centerX, centerY));
            BackgroundSprite.Draw();
            Game.Transforms.ResetView();
            float textX = centerX - scale * (Background.Width/2 - LeftPadding);
            float textY = centerY + scale * (Background.Height/2 - TopPadding);
            Game.TextEngine.QueueText(Text, new Vector2(textX, textY), Color.White, Font, SizePx, TextEngine.Orientation.Left, TextEngine.Orientation.Top);

            Game.TextEngine.DrawText(new RectangleF(textX, textY - scale*(Background.Height - TopPadding - BottomPadding), scale*(Background.Width - LeftPadding - RightPadding), scale*(Background.Height - TopPadding - BottomPadding)));

            Game.Transforms.PopView();
        }

        public override void Update()
        {
            if ((PauseKeyWasUp && Game.Controller.Quit) || (EnterKeyWasUp && Game.Controller.Enter))
            {
                Level.ClosePopup();
            }

            PauseKeyWasUp = !Game.Controller.Quit;
            EnterKeyWasUp = !Game.Controller.Enter;
        }
    }
}
