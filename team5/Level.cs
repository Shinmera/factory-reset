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
    class Level
    {
        public List<Chunk> Chunks;
        public Chunk ActiveChunk;
        public Player Player;
        public Camera Camera;
        public int collected = 0;


        private bool ChunkTrans = false;

        private List<Chunk> TransitionChunks;
        private int TransitionDirection;
        private Chunk LastActiveChunk;
        private Chunk TargetChunk;
        private int TransitionLingerCounter = 0;
        private const int TransitionLingerDuration = 20;

        //TESTING ONLY
        public Level(Game1 game)
        {
            Player = new Player(new Vector2(0, 0), game);
            Camera = new Camera(Player, game);

            ActiveChunk = new Chunk(game, this, "Chunks/TestChunk", new Vector2(64*Chunk.TileSize, 64 * Chunk.TileSize));
            ActiveChunk.Activate(Player);
            LastActiveChunk = ActiveChunk;

            Chunks = new List<Chunk>();
            TransitionChunks = new List<Chunk>();
            TransitionDirection = 0;
            Chunks.Add(ActiveChunk);
            Chunks.Add(new Chunk(game, this, "Chunks/TestChunk", new Vector2(192 * Chunk.TileSize, 64 * Chunk.TileSize)));
            Chunks.Add(new Chunk(game, this, "Chunks/TestChunk", new Vector2(-64 * Chunk.TileSize, 64 * Chunk.TileSize)));
            Chunks.Add(new Chunk(game, this, "Chunks/TestChunk2", new Vector2(10 * Chunk.TileSize, -10 * Chunk.TileSize)));
        }
        
        public void LoadContent(ContentManager content)
        {
            foreach(Chunk chunk in Chunks)
            {
                chunk.LoadContent(content);
            }
            Player.Position = ActiveChunk.SpawnPosition;

            Camera.UpdateChunk(ActiveChunk);
        }
        
        public void Resize(int width, int height)
        {
            Camera.Resize(width, height);
        }

        public void Update(GameTime gameTime)
        {
            if (!ChunkTrans)
            {
                Camera.Update(gameTime);
            }
            else
            {
                Camera.Update(gameTime);
            }

            RectangleF PlayerBB = Player.GetBoundingBox();

            if (!ChunkTrans)
            {
                if (PlayerBB.Right > ActiveChunk.BoundingBox.Right && Player.Velocity.X > 0)
                {
                    TransitionDirection = Chunk.Right;
                    ChunkTrans = true;
                }
                else if (PlayerBB.Left < ActiveChunk.BoundingBox.Left && Player.Velocity.X < 0)
                {
                    TransitionDirection = Chunk.Left;
                    ChunkTrans = true;
                }
                else if (PlayerBB.Top > ActiveChunk.BoundingBox.Top && Player.Velocity.Y > 0)
                {
                    TransitionDirection = Chunk.Up;
                    Player.Velocity.Y = 250;
                    ChunkTrans = true;
                }
                else if (PlayerBB.Bottom < ActiveChunk.BoundingBox.Bottom && Player.Velocity.Y < 0)
                {
                    TransitionDirection = Chunk.Down;
                    ChunkTrans = true;
                }

                if (ChunkTrans)
                {
                    foreach (var chunk in Chunks)
                    {
                        if (PlayerBB.Intersects(chunk.BoundingBox))
                        {
                            if(chunk != ActiveChunk)
                            {
                                TargetChunk = chunk;
                            }
                        }
                    }

                    ActiveChunk.Deactivate();
                    LastActiveChunk = ActiveChunk;
                    ActiveChunk = null;

                    if (TargetChunk == null)
                    {
                        TargetChunk = LastActiveChunk;
                    }
                    Camera.UpdateChunk(TargetChunk);
                }
            }

            if(ChunkTrans){
                TransitionChunks.Clear();

                TargetChunk.Update(gameTime);

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

                Player.Update(gameTime, TransitionDirection);
            }
            else
            {
                if (ActiveChunk != null)
                    ActiveChunk.Update(gameTime);
            }

        }

        public void Draw(GameTime gameTime)
        {
            if (ChunkTrans)
            {
                Player.Draw(gameTime);
            }

            foreach (Chunk chunk in Chunks){
                if (Camera.IsVisible(chunk.BoundingBox))
                {
                    chunk.Draw(gameTime);
                }
            }
        }
    }
}
