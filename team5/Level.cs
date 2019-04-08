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
        public int collected = 0;
        
        public readonly string Name;
        private readonly string Description;
        
        private readonly Game1 Game;
        private bool ChunkTrans = false;
        private List<Chunk> TransitionChunks = new List<Chunk>();
        private int TransitionDirection = 0;
        private Chunk LastActiveChunk;
        private Chunk TargetChunk;
        private int TransitionLingerCounter = 0;
        private const int TransitionLingerDuration = 20;

        public Alarm Alarm;

        public Level(Game1 game, string name)
        {
            Player = new Player(new Vector2(0, 0), game);
            Camera = new Camera(Player, game);
            Game = game;
            Name = name;
            Alarm = new Alarm(game);
        }
        
        public override void LoadContent(ContentManager content)
        {
            var data = content.Load<LevelContent>("Levels/"+Name);
            
            foreach(var chunkdata in data.chunks)
            {
                Chunk chunk = new Chunk(Game, this, chunkdata);
                chunk.LoadContent(content);
                Chunks.Add(chunk);
            }
            
            Player.LoadContent(content);
            ActiveChunk = Chunks[data.startChunk];
            ActiveChunk.Activate(Player);
            LastActiveChunk = ActiveChunk;
            Player.Position = ActiveChunk.SpawnPosition;
            
            //  Force camera to be still
            Camera.Position.X = Player.Position.X;
            Camera.Position.Y = Player.Position.Y;
            Camera.UpdateChunk(ActiveChunk);
        }
        
        public override void Resize(int width, int height)
        {
            Camera.Resize(width, height);
        }

        public override void Update()
        {
            if (!ChunkTrans)
            {
                Camera.Update();
            }
            else
            {
                Camera.Update();
            }

            RectangleF PlayerBB = Player.GetBoundingBox();

            if (!ChunkTrans)
            {
                if (PlayerBB.Right + Game1.DeltaT * Player.Velocity.X >= ActiveChunk.BoundingBox.Right && Player.Velocity.X > 0)
                {
                    TransitionDirection = Chunk.Right;
                    ChunkTrans = true;
                }
                else if (PlayerBB.Left + Game1.DeltaT * Player.Velocity.X <= ActiveChunk.BoundingBox.Left && Player.Velocity.X < 0)
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
                    TargetChunk = null;
                    foreach (var chunk in Chunks)
                    {
                        PlayerBB.X += 2 * Game1.DeltaT * Player.Velocity.X;
                        PlayerBB.Width += Math.Abs(2 * Game1.DeltaT * Player.Velocity.X);
                        if (PlayerBB.Intersects(chunk.BoundingBox))
                        {
                            if(chunk != ActiveChunk)
                            {
                                TargetChunk = chunk;
                            }
                        }
                    }

                    if (TargetChunk == null)
                    {
                        TargetChunk = LastActiveChunk;
                        if(TransitionDirection == Chunk.Left || TransitionDirection == Chunk.Right)
                        {
                            ChunkTrans = false;
                        }
                        else
                        {
                            ActiveChunk.Deactivate();
                            LastActiveChunk = ActiveChunk;
                            ActiveChunk = null;
                        }
                    }
                    else
                    {
                        if(TransitionDirection == Chunk.Up)
                        {
                            Player.Velocity.Y = 250;
                        }
                        ActiveChunk.Deactivate();
                        LastActiveChunk = ActiveChunk;
                        ActiveChunk = null;
                    }
                    Camera.UpdateChunk(TargetChunk);
                }
            }

            if(ChunkTrans){
                TransitionChunks.Clear();

                TargetChunk.Update();

                foreach (var chunk in Chunks){
                    if (PlayerBB.Intersects(chunk.BoundingBox)){
                        TransitionChunks.Add(chunk);
                    }
                }

                if (TransitionChunks.Count == 1)
                {
                    if ((TransitionDirection == Chunk.Left || TransitionDirection == Chunk.Right)) { 
                        TransitionLingerCounter++;
                    }
                    else
                    {
                        TransitionLingerCounter = TransitionLingerDuration;
                    }
                }
                
                if(TransitionLingerCounter == TransitionLingerDuration)
                {
                    TransitionLingerCounter = 0;
                    ActiveChunk = TargetChunk;
                    ActiveChunk.Activate(Player);
                    ChunkTrans = false;
                    TransitionChunks.Clear();
                    TransitionDirection = 0;
                }
                else if(TransitionChunks.Count == 0)
                {
                    ActiveChunk = LastActiveChunk;
                    ActiveChunk.Activate(Player);
                    ActiveChunk.Die(Player);
                    ChunkTrans = false;
                    TransitionChunks.Clear();
                    TransitionDirection = 0;
                    Player.Velocity = new Vector2(0);
                    return;
                }

                Player.Update(TransitionDirection);
            }
            else
            {
                if (ActiveChunk != null)
                    ActiveChunk.Update();
            }

            Alarm.Update( ActiveChunk);
        }

        public override void Draw()
        {
            if (ChunkTrans)
            {
                Player.Draw();
            }

            foreach (Chunk chunk in Chunks){
                if (Camera.IsVisible(chunk.BoundingBox))
                {
                    chunk.Draw();
                }
            }
        }
    }
}
