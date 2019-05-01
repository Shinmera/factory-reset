﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
        private float Timer = 0;
        private AnimatedSprite Sprite;

        public Alarm(Game1 game) : base( game)
        {
            Sprite = new AnimatedSprite(null, game, new Vector2(64,48));
        }

        public override void LoadContent(ContentManager content)
        {
            Game.SoundEngine.Load("Alert", "Enemy_Detected");
            Sprite.Texture = Game.TextureCache["alert-backdrop"];
            Sprite.Add("alert", 0, 1, 1.0);
        }

        public void ContinueAlert(Vector2 pos, Chunk chunk)
        {
            chunk.ChunkAlarmState = true;

            chunk.CallAll(x => {
                if (x is IEnemy)
                {
                    ((IEnemy)x).Alert(pos, chunk);
                }
            });
        }

        public void Alert(Vector2 pos, Chunk chunk)
        {
            LastKnowPos = pos;
            Timer = AlarmTime;
            //Drones = true;

            chunk.ChunkAlarmState = true;

            chunk.CallAll(x => {
                if (x is IEnemy) {
                    ((IEnemy)x).Alert(pos, chunk);
                }
            });
        }

        public void ClearAlarm(Chunk chunk)
        {
            chunk.ChunkAlarmState = false;

            chunk.CallAll(x => {
                if (x is IEnemy)
                {
                    ((IEnemy)x).ClearAlarm(chunk);
                }
            });
        }

        public void ResetAlarm(Chunk chunk)
        {
            chunk.ChunkAlarmState = false;

            chunk.CallAll(x => {
                if (x is IEnemy)
                {
                    ((IEnemy)x).ClearAlarm(chunk);
                }
                if (x is AerialDrone)
                {
                    ((AerialDrone)x).Respawn(chunk);
                }
            });
        }

        public void SetState(AlarmState state, Chunk chunk)
        {
            State = state;
            switch (state)
            {
                case AlarmState.Clear:
                    break;
                case AlarmState.Raised:
                    Game.Controller.Vibrate(1f, 1f, 0.2f);
                    chunk.Level.Camera.Shake(10, 0.2F);
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
                        SetState(AlarmState.Raised, chunk);
                    }
                    break;
                case AlarmState.Raised:
                    Timer -= dt;
                    if (Timer <= 0)
                    {
                        Timer = 0;
                        Detected = false;
                        ClearAlarm(chunk);
                        SetState(AlarmState.Clear, chunk);
                    }
                    break;
            }
        }
        
        public bool IsRaised {
            get { return State == AlarmState.Raised; }
        }

        public void Draw(Level level)
        {
            if(!Detected) return;
            Game.Transforms.PushView();
            Game.Transforms.ResetView();
            Game.Transforms.TranslateView(new Vector2(Camera.TargetWidth, 300));
            Game.Transforms.ScaleView(level.Camera.ViewScale);
            Sprite.Draw();
            float full = (float)Math.Truncate(Timer);
            float rest = Timer - full;
            Game.Transforms.PopView();
            Game.TextEngine.QueueText(full.ToString("00"), new Vector2(Camera.TargetWidth+5,300), 26,
                                      Color.White, TextEngine.Orientation.Right, TextEngine.Orientation.Center);
            Game.TextEngine.QueueText(rest.ToString(".00"), new Vector2(Camera.TargetWidth+5,300-3), 16,
                                      Color.LightGray, TextEngine.Orientation.Left, TextEngine.Orientation.Center);
        }

    }
}
