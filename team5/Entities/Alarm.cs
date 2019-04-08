using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

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

        private float AlarmTime = 5;
        private float AlertTime = 1;
        private float Timer = 0;

        public Alarm(Game1 game) : base( game)
        {

        }

        public override void LoadContent(ContentManager content)
        {
            Game.SoundEngine.Load("Alert");
        }

        public void SetState(AlarmState state )
        {
            State = state;
            switch (state)
            {
                case AlarmState.Clear:

                    break;
                case AlarmState.Raised:

                    break;
                case AlarmState.Alert:

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
                    Debug.WriteLine("Alarm Raised"); 
                    Timer -= dt;
                    if(Timer <= 0)
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
                    Debug.WriteLine("Alert mode");
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
            base.Draw();
        }

    }
}
