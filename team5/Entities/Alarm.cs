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
        };

        public bool Detected = false;
        //For Drone behavior
        public Vector2 LastKnowPos;
        public bool Drones = false;

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


        public void SendDrones(Vector2 pos)
        {
            LastKnowPos = pos;
            Timer = AlarmTime;
            Drones = true;
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
            }

        }

        public override void Update(Chunk chunk)
        {
            float dt = Game1.DeltaT;

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
                    if (Timer <= 0)
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
            if (Detected)
            {
                float textX = Game.GraphicsDevice.Viewport.Width / 2;
                float textY = Game.GraphicsDevice.Viewport.Height / 6;
                TextEngine.QueueText((Math.Floor(Timer)).ToString(), new Vector2(textX, textY), 
                                     "crashed-scoreboard", 48, TextEngine.Orientation.Center);
                Game.TextEngine.DrawText();
            }
            Game.Transforms.PopView();
        }

    }
}
