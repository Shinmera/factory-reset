using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace team5
{
    class Level:Window
    {
        public string Name;

        public List<Chunk> Chunks = new List<Chunk>();
        public Chunk ActiveChunk = null;
        public Player Player;
        public Camera Camera;
        public Alarm Alarm;
        public float Time = 0;
        public int DeathCounter = 0;
        public int AlertCounter = 0;
        public string Next = null;
        public string[][] TriggeredDialogs;
        public int NextTrigger = 0;

        public static string[][] RandomDialogs;

        static Level()
        {
            RandomDialogs = new string[7][];

            RandomDialogs[0] = new string[]{
                "You: Charlie, are you there?",
                "Charlie: Is there a problem?",
                "You: No, I just wanted to hear your voice.",
                "Charlie: We're doing fine, mom. Stay safe and come back soon."
            };

            RandomDialogs[1] = new string[]
            {
                "You: Hey dear! Still alive?",
                "Charlie: Yes of course, but are you still alive?",
                "You: No, I'm actually a ghost!\n" +
                "BOOooooOoOo!",
                "Charlie: Spooky."
            };

            RandomDialogs[2] = new string[]
            {
                "You: How are things going out there, Charlie?",
                "Charlie: Not so good. I'm sad and I miss the internet. I still can't believe it's gone!",
                "You: I'll call again later when you reached the acceptance stage of grief.",
                "Charlie: That's very sympathetic of you.\nThanks mom."
            };

            RandomDialogs[3] = new string[]
            {
                "You: I really wonder how things could get so far.",
                "Charlie: Humanity just got lazy, I suppose.",
                "You: What? No, I'm talking about architects designing places so unhandy for standard human movement! No wonder our city never attracted a lot of tourism!",
                "Charlie: Huh."
            };

            RandomDialogs[4] = new string[]
            {
                "You: Charlie?",
                "?: Hey, grandma!",
                "You: Oh, it's you! Hey there, little one! Is Charlie there?",
                "?: ...Charlie, stop crying! Grandma is here!",
                "You: I'll call again later."
            };

            RandomDialogs[5] = new string[]
            {
                "You: Charlie.",
                "Charlie: Yes, mom?",
                "You: I... I forgot what I wanted to say.",
                "Charlie: Stay safe, mom."
            };

            RandomDialogs[6] = new string[]
            {
                "You: Charlie?",
                "<No response>",
                "You: Maybe they're busy."
            };
        }

        private int InternalNRD = 0;
        public int NextRandomDialog {
            get {
                return Math.Min(RandomDialogs.Length-1,InternalNRD);
            }
            set {
                ++InternalNRD;
            }
        }

        public readonly object Identifier;
        public bool StartChase = false;
        private bool ChunkTrans = false;
        private List<Chunk> TransitionChunks = new List<Chunk>();
        private int TransitionDirection = 0;
        private Chunk LastActiveChunk;
        private Chunk TargetChunk;

        private int TransitionLingerTimer = 0;
        private const int TransitionLingerDuration = 40;
        private float DeathFadeLingerTimer = 0F;
        private const float DeathFadeLingerDuration = 1F;
        private float FallFadeTimer = 0F;
        private const float FallFadeDuration = 2F;
        private float LoadFadeTimer = 2F;
        private const float LoadFadeDuration = 2F;

        private List<Vector2> OverlayTriangles;

        private bool InternalPaused = false;

        public bool Paused
        {
            get { return InternalPaused; }
            set {
                InternalPaused = value;
                Game.SoundEngine.Paused = value;
                Game.MusicEngine.Paused = value;
            }
        }

        private readonly List<Container> Popups = new List<Container>();
        private readonly List<Container> DeletedPopups = new List<Container>();

        public Level(Game1 game, object identifier):base(game)
        {
            Player = new Player(new Vector2(0, 0), game);
            Camera = new Camera(Player, game);
            Game = game;
            Identifier = identifier;
            Alarm = new Alarm(game);

            OverlayTriangles = new List<Vector2>(6);
            for(int i = 0; i < 6; ++i)
            {
                OverlayTriangles.Add(Vector2.Zero);
            }
        }

        public void OpenDialogBox(string[] text)
        {
            Popups.Add(new DialogBox(text, "wellbutrin", 14, Game, this));
            Paused = true;
        }

        public void ClosePopup()
        {
            DeletedPopups.Add(Popups.Last());
            if(Popups.Count == DeletedPopups.Count)
            {
                Paused = false;
            }
        }

        public void ClosePopup(Container popup)
        {
            DeletedPopups.Add(popup);
            if (Popups.Count == DeletedPopups.Count)
            {
                Paused = false;
            }
        }

        public override void LoadContent(ContentManager content)
        {
            using(var data = LevelContent.Read(Identifier))
            {
                data.Resolve(Game.GraphicsDevice);
            
                foreach(var chunkdata in data.chunks)
                {
                    Chunk chunk = new Chunk(Game, this, chunkdata);
                    chunk.LoadContent(content);
                    Chunks.Add(chunk);
                }

                StartChase = data.startChase;
                ActiveChunk = Chunks[data.startChunk];
                Next = data.next;

                Name = data.name;

                TriggeredDialogs = data.storyItems;
            }

            for(int i = 0; i < RandomDialogs.Length - 2; ++i)
            {
                int newIndex = i + (int)Math.Floor(Game.RNG.NextDouble() * (RandomDialogs.Length - i - 1));

                string[] temp = RandomDialogs[i];
                RandomDialogs[i] = RandomDialogs[newIndex];
                RandomDialogs[newIndex] = temp;
            }
            
            Player.LoadContent(content);
            Player.Position = ActiveChunk.SpawnPosition;
            ActiveChunk.Activate(Player);
            LastActiveChunk = ActiveChunk;
            
            //  Force camera to be still
            Camera.Position.X = Player.Position.X;
            Camera.Position.Y = Player.Position.Y;
            Camera.UpdateChunk(ActiveChunk);
            Camera.SnapToLocation();
            Camera.Update();
            Alarm.LoadContent(content);

            Game.MusicEngine.Load("Ambient", "future ambient", 12);
            Game.MusicEngine.Play("Ambient");
            Game.SoundEngine.Load("UI_Button");
            Game.SoundEngine.Load("Player_WalkieEnd");
            Game.SoundEngine.Load("Player_WalkieTalk1");
        }
        
        public override void UnloadContent()
        {
            foreach(var chunk in Chunks)
                chunk.UnloadContent();
            Player.UnloadContent();
            Alarm.UnloadContent();
        }
        
        public override void Resize(int width, int height)
        {
            Camera.Resize(width, height);
        }

        public void RespawnAll()
        {
            foreach (var chunk in Chunks)
                chunk.RespawnAll();
        }

        public override void Update()
        {
            if (Paused)
            {
                Popups.ForEach((x)=>x.Update());
                DeletedPopups.ForEach((x) => Popups.Remove(x));
                DeletedPopups.Clear();
                Camera.Update();
            }
            else
            {
                Camera.Update();
                Time += Game1.DeltaT;

                RectangleF PlayerBB = Player.GetBoundingBox();

                if (!ChunkTrans)
                {
                    if (Player.DeathTimer > 0)
                    {
                        DeathFadeLingerTimer = DeathFadeLingerDuration;
                    }

                    if (PlayerBB.Right + Game1.DeltaT * Player.Velocity.X > ActiveChunk.BoundingBox.Right && Player.Velocity.X > 0)
                    {
                        TransitionDirection = Chunk.Right;
                        ChunkTrans = true;
                    }
                    else if (PlayerBB.Left + Game1.DeltaT * Player.Velocity.X < ActiveChunk.BoundingBox.Left && Player.Velocity.X < 0)
                    {
                        TransitionDirection = Chunk.Left;
                        ChunkTrans = true;
                    }
                    else if (PlayerBB.Top >= ActiveChunk.BoundingBox.Top && Player.Velocity.Y > 0)
                    {
                        TransitionDirection = Chunk.Up;
                        ChunkTrans = true;
                    }
                    else if (PlayerBB.Bottom <= ActiveChunk.BoundingBox.Bottom && Player.Velocity.Y < 0)
                    {
                        TransitionDirection = Chunk.Down;
                        ChunkTrans = true;
                    }

                    if (ChunkTrans)
                    {
                        TransitionLingerTimer = 0;
                        TargetChunk = null;
                        foreach (var chunk in Chunks)
                        {
                            PlayerBB.X += Math.Min(0, Game1.DeltaT * Player.Velocity.X);
                            PlayerBB.Width += Math.Abs(Game1.DeltaT * Player.Velocity.X);
                            if (PlayerBB.Intersects(chunk.BoundingBox))
                            {
                                if (chunk != ActiveChunk)
                                {
                                    TargetChunk = chunk;
                                }
                            }
                        }

                        if (TargetChunk == null)
                        {
                            TargetChunk = LastActiveChunk;
                            if (TransitionDirection == Chunk.Left || TransitionDirection == Chunk.Right || TransitionDirection == Chunk.Up)
                            {
                                ChunkTrans = false;
                            }
                        }
                        if (ChunkTrans)
                        {
                            if ((TransitionDirection == Chunk.Left || TransitionDirection == Chunk.Right) &&
                                TargetChunk.CollideSolid(Player, Game1.DeltaT, out int direction, out float time, out RectangleF[] targetBB, out Vector2[] targetvel))
                            {
                                ChunkTrans = false;
                            }
                            else
                            {
                                if (TransitionDirection == Chunk.Up)
                                {
                                    Player.Velocity.Y = 350;
                                }
                                Player.Position.X += Player.Velocity.X * Game1.DeltaT;
                                ActiveChunk.Deactivate();
                                LastActiveChunk = ActiveChunk;
                                ActiveChunk = null;

                                if (TargetChunk.ChunkAlarmState != Alarm.Detected)
                                {
                                    if (Alarm.Detected)
                                    {
                                        Vector2 posOffset = Vector2.Zero;
                                        switch (TransitionDirection)
                                        {
                                            case Chunk.Up:
                                                posOffset = Vector2.UnitY;
                                                break;
                                            case Chunk.Down:
                                                posOffset = -Vector2.UnitY;
                                                break;
                                            case Chunk.Right:
                                                posOffset = Vector2.UnitX;
                                                break;
                                            case Chunk.Left:
                                                posOffset = -Vector2.UnitX;
                                                break;
                                        }

                                        Alarm.ContinueAlert(Player.Position + Chunk.TileSize * 2F * posOffset, TargetChunk);
                                    }
                                    else
                                    {
                                        Alarm.ResetAlarm(TargetChunk);
                                    }
                                }

                                Camera.UpdateChunk(TargetChunk);
                            }
                        }
                    }
                }

                if (ChunkTrans)
                {
                    TransitionChunks.Clear();

                    TargetChunk.Update();
                    Alarm.Update(TargetChunk);


                    foreach (var chunk in Chunks)
                    {
                        if (PlayerBB.Intersects(chunk.BoundingBox))
                        {
                            TransitionChunks.Add(chunk);
                        }
                    }

                    if (TransitionChunks.Count == 1 && TargetChunk != LastActiveChunk)
                    {
                        if ((TransitionDirection == Chunk.Left || TransitionDirection == Chunk.Right))
                        {
                            TransitionLingerTimer++;
                        }
                        else
                        {
                            TransitionLingerTimer = TransitionLingerDuration;
                        }
                    }

                    if (TransitionLingerTimer == TransitionLingerDuration)
                    {
                        TransitionLingerTimer = 0;
                        ActiveChunk = TargetChunk;
                        LastActiveChunk = ActiveChunk;
                        ActiveChunk.Activate(Player);
                        ChunkTrans = false;
                        TransitionChunks.Clear();
                        TransitionDirection = 0;
                    }
                    else if (TransitionChunks.Count == 0)
                    {
                        Player.Kill();
                    }

                    if(Player.Update(TransitionDirection, TransitionLingerTimer, TargetChunk))
                    {
                        DeathFadeLingerTimer = DeathFadeLingerDuration;
                        ActiveChunk = LastActiveChunk;
                        LastActiveChunk = ActiveChunk;
                        ActiveChunk.Activate(Player, false);
                        ChunkTrans = false;
                        TransitionChunks.Clear();
                        TransitionDirection = 0;
                        Player.Velocity = new Vector2(0);
                    }
                }
                else
                {
                    if (ActiveChunk != null)
                    {
                        ActiveChunk.Update();
                        Alarm.Update(ActiveChunk);
                    }
                }
            }

            if(DeathFadeLingerTimer > 0)
            DeathFadeLingerTimer -= Game1.DeltaT;

            if(LoadFadeTimer > 0)
                LoadFadeTimer -= Game1.DeltaT;
        }

        public override void Draw()
        {
            foreach (Chunk chunk in Chunks)
                if (Camera.IsVisible(chunk.BoundingBox))
                    chunk.DrawBackground();

            foreach (Chunk chunk in Chunks)
                if (Camera.IsVisible(chunk.BoundingBox))
                    chunk.Draw();

            if (ChunkTrans)
                Player.Draw();

            foreach (Chunk chunk in Chunks)
                if (Camera.IsVisible(chunk.BoundingBox))
                    chunk.DrawForeground();

            Alarm.Draw(this);

            foreach (Container container in Popups)
                container.Draw();

            if(Player.DeathTimer > 0 || DeathFadeLingerTimer > 0 || FallFadeTimer > 0 || LoadFadeTimer > 0)
            {
                float alpha = Player.DeathTimer > 0 ? 1 - Player.DeathTimer / Player.DeathDuration : 
                    (FallFadeTimer > 0 ? 1 - FallFadeTimer / FallFadeDuration : 
                    Math.Max(LoadFadeTimer/LoadFadeDuration,DeathFadeLingerTimer / DeathFadeLingerDuration));
                Vector2 LL = -1.2F*Camera.GetTargetSize();
                Vector2 UL = 1.2F*Camera.GetTargetSize() * new Vector2(-1,1);
                Vector2 LR = 1.2F*Camera.GetTargetSize() * new Vector2(1, -1);
                Vector2 UR = 1.2F*Camera.GetTargetSize();

                OverlayTriangles[0] = UR;
                OverlayTriangles[1] = UL;
                OverlayTriangles[2] = LL;
                OverlayTriangles[3] = LL;
                OverlayTriangles[4] = LR;
                OverlayTriangles[5] = UR;

                Game.TriangleEngine.DrawTriangles(Camera.Position, OverlayTriangles, new Color(0,0,0,alpha));

                if(Player.DeathTimer > 0 || FallFadeTimer > 0)
                {
                    //Game.TextEngine.QueueText("Try not dying", Camera.GetTargetSize() + Vector2.UnitY * 100, 40, Color.DarkRed, TextEngine.Orientation.Center, TextEngine.Orientation.Center);
                    //Game.TextEngine.DrawText();
                }
                else if(DeathFadeLingerTimer > 0)
                {
                    //Game.TextEngine.QueueText("YOU DIED", Camera.GetTargetSize() + Vector2.UnitY * 100, 40, new Color(alpha * 139 / 255F, 0, 0, alpha), TextEngine.Orientation.Center, TextEngine.Orientation.Center);
                    //Game.TextEngine.DrawText();
                }
            }
        }
        
        public Dictionary<string, string> Score()
        {
            var result = new Dictionary<string, string>();
            int minutes = (int)(Time / 60);
            int seconds = (int)Time % 60;
            int total = 0;
            int collected = 0;
            
            foreach(var chunk in Chunks)
            {
                total += chunk.TotalPickups;
                collected += chunk.NextItem;
            }
            
            result.Add("Time", minutes+":"+seconds.ToString("00"));
            result.Add("Pickups", collected+"/"+total);
            result.Add("Alerts", AlertCounter+"");
            result.Add("Deaths", DeathCounter+"");
            return result;
        }
    }
}
