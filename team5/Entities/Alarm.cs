using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System;

namespace team5
{
    class Alarm : GameObject
    {
        public enum AlarmState
        {
            Clear,
            Raised,
            Alert,
        };

        public bool Detected = false;

        private AlarmState State = AlarmState.Clear;

        private float AlarmTime = 20;
        private float AlertTime = 1;
        private float Timer = 0;

        //drawing the hud
        private SpriteFont font;
        private TextEngine TextEngine;

        public Alarm(Game1 game) : base( game)
        {
            TextEngine = game.TextEngine;

        }

        public override void LoadContent(ContentManager content)
        {
            Game.SoundEngine.Load("Alert");
            Game.TextEngine.LoadContent(content);
        }

        public void SetState(AlarmState state )
        {
            State = state;
            switch (state)
            {
                case AlarmState.Clear:

                    break;
                case AlarmState.Raised:
                    Game.SoundEngine.Play("Alert");
                    break;
                case AlarmState.Alert:

                    break;
            }

        }

        public override void Update(Chunk chunk)
        {
            float dt = Game1.DeltaT;
            float textX = Game.GraphicsDevice.Viewport.Width / 2;
            float textY = Game.GraphicsDevice.Viewport.Height / 6;
            Debug.WriteLine(textX);
            
            switch (State)
            {
                case AlarmState.Clear:
                    if (Detected)
                    {
                        Timer = AlarmTime;
                        SetState(AlarmState.Raised);
                    }
                    break;
                case AlarmState.Raised: 
                    Timer -= dt;

                    TextEngine.QueueText(  (Math.Floor( Timer)).ToString(), new Vector2(textX, textY), Color.Red, "Arial");
                    if (Timer <= 0)
                    {
                        Detected = false;
                        chunk.Die(chunk.Level.Player);
                        SetState(AlarmState.Clear);
                    }
                    if (chunk.Level.Player.IsHiding)
                    {
                        Timer = AlertTime;
                        SetState(AlarmState.Alert);
                    }
                    break;
                case AlarmState.Alert:
                    Timer -= dt;
                    if(Timer <= 0)
                    {
                        Detected = false;
                        SetState(AlarmState.Clear);
                    }
                    break;
            }
        }

        public override void Draw()
        {
            Game.Transforms.PushView();
            Game.Transforms.ResetView();
            Game.TextEngine.DrawText();
            Game.Transforms.PopView();
        }

    }
}
