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
    class MainMenu:Window
    {
        private Game1 Game;
        private SpriteBatch SpriteBatch;
        //private Controller Controller;
        private AnimatedSprite Background;

        private Vector2 Size;

        private class Button
        {
            public Button Left;
            public Button Right;
            public Button Up;
            public Button Down;

            protected Vector2 Size;
            public Vector2 Location;

            public readonly Action Action;

            public readonly Game1 Game;

            public Button(Game1 game, Vector2 location, Action action)
            {
                Game = game;
                Location = location;
                Action = action;
            }

            public virtual void LoadContent(ContentManager content)
            {
            }

            public virtual void Draw(SpriteBatch batch, bool selected)
            {
            }
        }

        private class TextButton : Button
        {
            private readonly string Label;

            public TextButton(Game1 game, Vector2 location, Action action, string label) : base(game, location, action)
            {
                Label = label;
            }

            public override void Draw(SpriteBatch batch, bool selected)
            {
                Game.TextEngine.QueueText(Label, Game.Transforms * Location, 20, 
                                          (selected)? Color.White : new Color(0.8f, 0.8f, 0.8f),
                                          TextEngine.Orientation.Right);
            }
        }

        private List<Button> Buttons;
        private Button ActiveButton;

        public MainMenu(Game1 game)
        {
            Game = game;
            
            Background = new AnimatedSprite(null, game, new Vector2(480,270));
            
            Buttons = new List<Button>();

            Buttons.Add(new TextButton(game, new Vector2(360, 80), Game.StartLevel, "Start"));
            Buttons.Add(new TextButton(game, new Vector2(360, 90), Game.Exit, "Quit"));

            Buttons[1].Up = Buttons[0];
            Buttons[0].Down = Buttons[1];

            ActiveButton = Buttons[0];
        }
        
        public override void LoadContent(ContentManager content)
        {
            Background.Texture = content.Load<Texture2D>("Textures/main-menu");
            Background.Add("idle", 0, 4, 0.4);
            Background.Play("idle");
        }

        public override void OnQuitButon()
        {
            Game.Exit();
        }

        public override void Update()
        {
            Background.Update(Game1.DeltaT);

            if (Game.Controller.Enter)
            {
                ActiveButton.Action.Invoke();
            }
            else
            {
                if (Game.Controller.MoveLeft)
                {
                    if(ActiveButton.Left != null)
                    {
                        ActiveButton = ActiveButton.Left;
                    }
                }
                if (Game.Controller.MoveRight)
                {
                    if (ActiveButton.Right != null)
                    {
                        ActiveButton = ActiveButton.Right;
                    }
                }
                if (Game.Controller.MoveDown)
                {
                    if (ActiveButton.Down != null)
                    {
                        ActiveButton = ActiveButton.Down;
                    }
                }
                if (Game.Controller.MoveUp)
                {
                    if (ActiveButton.Up != null)
                    {
                        ActiveButton = ActiveButton.Up;
                    }
                }
            }
        }

        public override void Draw()
        {
            Game.Transforms.ScaleView(Math.Max(Game.GraphicsDevice.Viewport.Width/Background.FrameSize.X,
                                               Game.GraphicsDevice.Viewport.Height/Background.FrameSize.Y));
            Game.Transforms.Push();
            Game.Transforms.Translate(Background.FrameSize/2);
            Background.Draw();
            Game.Transforms.Pop();

            foreach(Button button in Buttons)
                button.Draw(SpriteBatch, (button == ActiveButton));
            
            Game.TextEngine.DrawText();
        }

        public override void Resize(int width, int height)
        {
            Size.X = width / 2F;
            Size.Y = height / 2F;
        }
    }
}
