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
        public List<Chunk> Chunks = new List<Chunk>();
        public Chunk ActiveChunk = null;
        public Player Player;
        public Camera Camera;
        public Alarm Alarm;
        public float Time = 0;
        public int DeathCounter = 0;
        public int AlertCounter = 0;
        
        private readonly object Identifier;
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

        public bool Paused = false;

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
            var data = LevelContent.Read(Identifier);
            data.Resolve(Game.GraphicsDevice);
            
            foreach(var chunkdata in data.chunks)
            {
                Chunk chunk = new Chunk(Game, this, chunkdata);
                chunk.LoadContent(content);
                Chunks.Add(chunk);
            }
            
            Player.LoadContent(content);
            ActiveChunk = Chunks[data.startChunk];
            Player.Position = ActiveChunk.SpawnPosition;
            ActiveChunk.Activate(Player);
            LastActiveChunk = ActiveChunk;
            
            
            //  Force camera to be still
            Camera.Position.X = Player.Position.X;
            Camera.Position.Y = Player.Position.Y;
            Camera.UpdateChunk(ActiveChunk);
            Camera.SnapToLocation();
            Camera.Update();
            //Alarm sound
            Alarm.LoadContent(content);

            TextBox.LoadStaticContent(content);
            DialogBox.LoadStaticContent(content);
        }
        
        public override void Resize(int width, int height)
        {
            Camera.Resize(width, height);
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

                if (FallFadeTimer > 0)
                {
                    FallFadeTimer -= Game1.DeltaT;

                    TargetChunk.Update();

                    if (FallFadeTimer <= 0)
                    {
                        FallFadeTimer = 0;
                        DeathFadeLingerTimer = DeathFadeLingerDuration;
                        ActiveChunk = LastActiveChunk;
                        ActiveChunk.Activate(Player, false);
                        ActiveChunk.Die(Player);
                        ChunkTrans = false;
                        TransitionChunks.Clear();
                        TransitionDirection = 0;
                        Player.Velocity = new Vector2(0);
                        return;
                    }
                }
                else
                {
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
                                if (TransitionDirection == Chunk.Left || TransitionDirection == Chunk.Right)
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
                            ActiveChunk.Activate(Player);
                            ChunkTrans = false;
                            TransitionChunks.Clear();
                            TransitionDirection = 0;
                        }
                        else if (TransitionChunks.Count == 0)
                        {
                            FallFadeTimer = FallFadeDuration;
                            return;
                        }

                        Player.Update(TransitionDirection, TransitionLingerTimer, TargetChunk);
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
                    chunk.Draw();

            if (ChunkTrans)
                Player.Draw();

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
