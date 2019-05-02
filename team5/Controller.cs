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
        public static float VibrationMultiplier = 0.5F;
        public struct State{
            public readonly bool MoveLeft, MoveRight, MoveUp, MoveDown, Jump, Climb, Hide, Crouch, Pause, Interact, Advance, Back;
            public State(KeyboardState key, GamePadState pad){
                MoveLeft = key.IsKeyDown(Keys.A)
                    || key.IsKeyDown(Keys.Left)
                    || pad.DPad.Left == ButtonState.Pressed
                    || pad.ThumbSticks.Left.X < -0.25;

                MoveRight = key.IsKeyDown(Keys.D)
                    || key.IsKeyDown(Keys.Right)
                    || pad.DPad.Right == ButtonState.Pressed
                    || pad.ThumbSticks.Left.X > +0.25;

                MoveUp = key.IsKeyDown(Keys.W)
                    || key.IsKeyDown(Keys.Up)
                    || pad.DPad.Up == ButtonState.Pressed
                    || pad.ThumbSticks.Left.Y > +0.25;

                MoveDown = key.IsKeyDown(Keys.S)
                    || key.IsKeyDown(Keys.Down)
                    || pad.DPad.Down == ButtonState.Pressed
                    || pad.ThumbSticks.Left.Y < -0.25;

                Jump = key.IsKeyDown(Keys.Space)
                    || pad.Buttons.A == ButtonState.Pressed
                    || pad.Buttons.B == ButtonState.Pressed;
        
                Climb = key.IsKeyDown(Keys.LeftShift)
                    || pad.Buttons.LeftShoulder == ButtonState.Pressed
                    || pad.Buttons.RightShoulder == ButtonState.Pressed
                    || pad.Triggers.Left >= 0.5
                    || pad.Triggers.Right >= 0.5;

                Hide = key.IsKeyDown(Keys.F)
                    || pad.Buttons.X == ButtonState.Pressed
                    || pad.Buttons.Y == ButtonState.Pressed;
        
                Crouch = key.IsKeyDown(Keys.LeftControl)
                    || pad.Buttons.LeftStick == ButtonState.Pressed;
        
                Pause = key.IsKeyDown(Keys.Escape)
                    || pad.Buttons.Start == ButtonState.Pressed;

                Interact = key.IsKeyDown(Keys.E)
                    || pad.Buttons.X == ButtonState.Pressed
                    || pad.Buttons.Y == ButtonState.Pressed;
                
                Advance = key.IsKeyDown(Keys.Space)
                    || pad.Buttons.A == ButtonState.Pressed
                    || pad.Buttons.B == ButtonState.Pressed;

                Back = key.IsKeyDown(Keys.Back)
                    || pad.Buttons.B == ButtonState.Pressed
                    || pad.Buttons.Back == ButtonState.Pressed;
            }
        };
        
        readonly int gamepadIndex;
        public State Is, Was;
        private float VibrationTimer = 0;
        
        public Controller(int gamepadIndex=0)
        {
            this.gamepadIndex = gamepadIndex;
        }

        public void Vibrate(float left, float right, float duration)
        {
            GamePad.SetVibration(gamepadIndex, left*VibrationMultiplier, right*VibrationMultiplier);
            
            VibrationTimer = duration;
        }

        public bool MoveLeft => Is.MoveLeft;
        public bool MoveRight => Is.MoveRight;
        public bool MoveUp => Is.MoveUp;
        public bool MoveDown => Is.MoveDown;
        public bool Jump => Is.Jump;
        public bool Climb => Is.Climb;
        public bool Hide => Is.Hide;
        public bool Crouch => Is.Crouch;
        public bool Pause => Is.Pause;
        public bool Interact => Is.Interact;
        public bool Advance => Is.Advance;
        public bool Back => Is.Back;

        public void Update()
        {
            Was = Is;
            Is = new State(Keyboard.GetState(), GamePad.GetState(gamepadIndex));
            if (VibrationTimer > 0)
            {
                VibrationTimer -= Game1.DeltaT;
                if (VibrationTimer <= 0)
                    GamePad.SetVibration(gamepadIndex, 0, 0);
            }
        }
    }
}
