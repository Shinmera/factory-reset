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
        private Controller Controller;

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

            public virtual void Draw(SpriteBatch batch, Vector2 Center, bool selected)
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

            public override void Draw(SpriteBatch batch, Vector2 Center, bool selected)
            {
                Game.TextEngine.QueueText(Label, Center+Location, (selected)? 48 : 24, TextEngine.Orientation.Center);
            }
        }

        private List<Button> Buttons;
        private Button ActiveButton;

        public MainMenu(Game1 game)
        {
            Game = game;
            Buttons = new List<Button>();

            Buttons.Add(new TextButton(game, new Vector2(0, -200), Game.StartLevel, "Start"));

            Buttons.Add(new TextButton(game, new Vector2(0, 200), Game.Exit, "Quit"));

            Buttons[1].Up = Buttons[0];
            Buttons[0].Down = Buttons[1];

            ActiveButton = Buttons[0];

            Controller = new Controller();
        }

        public override void Update()
        {
            Controller.Update();

            if (Controller.Enter)
            {
                ActiveButton.Action.Invoke();
            }
            else
            {
                if (Controller.MoveLeft)
                {
                    if(ActiveButton.Left != null)
                    {
                        ActiveButton = ActiveButton.Left;
                    }
                }
                if (Controller.MoveRight)
                {
                    if (ActiveButton.Right != null)
                    {
                        ActiveButton = ActiveButton.Right;
                    }
                }
                if (Controller.MoveDown)
                {
                    if (ActiveButton.Down != null)
                    {
                        ActiveButton = ActiveButton.Down;
                    }
                }
                if (Controller.MoveUp)
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
            Game.GraphicsDevice.Clear(Color.LightGray);

            foreach(Button button in Buttons)
            {
                if (button != ActiveButton)
                {
                    button.Draw(SpriteBatch, Size, false);
                }
                else
                {
                    button.Draw(SpriteBatch, Size, true);
                }
            }
            Game.TextEngine.DrawText();
        }

        public override void Resize(int width, int height)
        {
            Size.X = width / 2F;
            Size.Y = height / 2F;


        }
    }
}
