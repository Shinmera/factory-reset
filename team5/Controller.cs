using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace team5
{
    public class Controller
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
            || pad.Buttons.RightShoulder == ButtonState.Pressed
            || pad.Triggers.Left >= 0.5
            || pad.Triggers.Right >= 0.5;

        public bool Hide => key.IsKeyDown(Keys.F)
            || pad.Buttons.X == ButtonState.Pressed
            || pad.Buttons.Y == ButtonState.Pressed;
        
        public bool Crouch => key.IsKeyDown(Keys.LeftControl)
            || pad.Buttons.LeftStick == ButtonState.Pressed;
        
        public bool Pause => key.IsKeyDown(Keys.Escape)
            || pad.Buttons.Start == ButtonState.Pressed;

        public bool Enter => key.IsKeyDown(Keys.Enter)
            || Jump;

        public bool Back => key.IsKeyDown(Keys.Back)
            || pad.Buttons.B == ButtonState.Pressed
            || pad.Buttons.Back == ButtonState.Pressed;

        public void Update()
        {
            key = Keyboard.GetState();
            pad = GamePad.GetState(gamepadIndex);
        }
    }
}
