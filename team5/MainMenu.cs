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
    class MainMenu
    {
        private Game1 Game;
        private SpriteBatch SpriteBatch;
        private Controller Controller;

        private class Button
        {
            public Button Left;
            public Button Right;
            public Button Up;
            public Button Down;

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
                string font = selected ? "ArialBoldLarge" : "Arial";
                Game.TextEngine.QueueText(Label, Location, Color.Black, font);
            }
        }

        private List<Button> Buttons;
        private Button ActiveButton;

        public void Start() {

        }

        public MainMenu(Game1 game)
        {
            Game = game;
            Buttons = new List<Button>();

            Buttons.Add(new TextButton(game, new Vector2(), Start, "Start"));

            Buttons.Add(new TextButton(game, new Vector2(), Game.Exit, "Start"));

            Controller = new Controller();
        }

        public void Update()
        {
            Controller.Update();

            if (Controller.Enter)
            {
                ActiveButton.Action.Invoke();
            }
        }

        public void Draw()
        {
            Game.GraphicsDevice.Clear(Color.LightGray);
            foreach(Button button in Buttons)
            {
                if (button != ActiveButton)
                {
                    button.Draw(SpriteBatch, false);
                }
                else
                {
                    button.Draw(SpriteBatch, true);
                }
            }
        }
    }
}
