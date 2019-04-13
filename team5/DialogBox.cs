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
    class DialogBox : Container
    {
        string Text;
        public string Font;
        static Texture2D Background;
        private bool PauseKeyWasUp = false;
        private bool EnterKeyWasUp = false;
        private Level Level { get { return (Level)Parent; } }

        private const int TopPadding = 10;
        private const int SidePadding = 10;

        private AnimatedSprite BackgroundSprite;

        public DialogBox(string text, Game1 game, Level parent) : base(game, parent)
        {
            Text = text;
            BackgroundSprite = new AnimatedSprite(Background, game, new Vector2(Background.Bounds.Width,Background.Bounds.Height));
            BackgroundSprite.Add("idle", 0, 1, 100);
            BackgroundSprite.Play("idle");
        }

        public static void LoadStaticContent(ContentManager content)
        {
            Background = content.Load<Texture2D>("Textures/dialog-box");
        }

        public override void Draw()
        {
            float centerX = Game.GraphicsDevice.Viewport.Width / 2;
            float scale = Game.GraphicsDevice.Viewport.Width / 1280F * Level.Camera.Zoom;
            float centerY = Game.GraphicsDevice.Viewport.Height / 2;

            Game.Transforms.Push();
            Game.Transforms.Reset();
            Game.Transforms.Translate(Level.Camera.Position);
            BackgroundSprite.Draw();
            Game.Transforms.Pop();

            Game.Transforms.PushView();
            Game.Transforms.ResetView();
            float textX = centerX - scale * (Background.Width - SidePadding);
            float textY = centerY - scale * (Background.Height - TopPadding);
            Game.TextEngine.QueueText(Text, new Vector2(textX, textY), Color.Black, "welbut", 20, TextEngine.Orientation.Left, TextEngine.Orientation.Bottom, true, Background.Width*2-SidePadding*2);

            Game.TextEngine.DrawText();

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
