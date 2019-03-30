using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace team5
{
    class Controller
    {
        readonly int gamepadIndex;
        KeyboardState key;
        GamePadState pad;
        
        public Controller(int gamepadIndex=0)
        {
            this.gamepadIndex = gamepadIndex;
        }

        public bool MoveLeft => key.IsKeyDown(Keys.A)
            || key.IsKeyDown(Keys.Left)
            || pad.DPad.Left == ButtonState.Pressed
            || pad.ThumbSticks.Left.X < -0.25;

        public bool MoveRight => key.IsKeyDown(Keys.D)
            || key.IsKeyDown(Keys.Right)
            || pad.DPad.Right == ButtonState.Pressed
            || pad.ThumbSticks.Left.X > +0.25;

        public bool MoveUp => key.IsKeyDown(Keys.W)
            || key.IsKeyDown(Keys.Up)
            || pad.DPad.Up == ButtonState.Pressed
            || pad.ThumbSticks.Left.Y > +0.25;

        public bool MoveDown => key.IsKeyDown(Keys.S)
            || key.IsKeyDown(Keys.Down)
            || pad.DPad.Down == ButtonState.Pressed
            || pad.ThumbSticks.Left.Y < -0.25;

        public bool Jump => key.IsKeyDown(Keys.Space)
            || pad.Buttons.A == ButtonState.Pressed
            || pad.Buttons.B == ButtonState.Pressed;
        
        public bool Climb => key.IsKeyDown(Keys.LeftShift)
            || pad.Buttons.LeftShoulder == ButtonState.Pressed
            || pad.Buttons.RightShoulder == ButtonState.Pressed;
        
        public bool Quit => key.IsKeyDown(Keys.Escape)
            || pad.Buttons.Start == ButtonState.Pressed;

        public void Update()
        {
            key = Keyboard.GetState();
            pad = GamePad.GetState(gamepadIndex);
        }
    }
}
