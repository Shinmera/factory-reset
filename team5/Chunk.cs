using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Content;

namespace team5
{
    class Chunk
    {
        Vector2 relPosition;

        public const int Empty = 0;
        public const int SolidPlatform = 1;

        public const int TileSize = 16;

        public int[,] TileSet;

        Dictionary<int, Vector2> tileDrawers;
        Dictionary<int, GameObject> tileObjects;

        //Viewcones, intelligence
        List<Entity> CollidingEntities;

        //Enemies, background objects
        List<Entity> NonCollidingEntities;

        //things that will stop you like moving platforms (which are not part of the tileset)
        List<Entity> SolidEntities;

        Game1 Game;

        //TESTING ONLY
        public Chunk(Game1 game, Player player)
        {
            Texture2D dummyTexture;
            dummyTexture = new Texture2D(game.GraphicsDevice, TileSize, TileSize);
            Color[] colors = new Color[TileSize * TileSize];
            for (int i = 0; i < TileSize * TileSize; ++i)
            {
                colors[i] = Color.Black;
            }
            dummyTexture.SetData(colors);

            tileDrawers = new Dictionary<int, Vector2>
            {
                { SolidPlatform, new Vector2(0,0) }
            };

            tileObjects = new Dictionary<int, GameObject>();
            tileObjects.Add(SolidPlatform, new TilePlatform(game));

            SolidEntities = new List<Entity>();
            NonCollidingEntities = new List<Entity>();
            CollidingEntities = new List<Entity>();

            TileSet = new int[100,100];

            for(int i = 20; i < 40; ++i)
            {
                TileSet[i, 65] = 1;
            }

            for (int i = 50; i < 67; ++i)
            {
                TileSet[30, i] = 1;
            }

            NonCollidingEntities.Add(player);

            SolidEntities.Add(new Platform(new Vector2(100, 700), game, 600, 10));

            SolidEntities.Add(new Platform(new Vector2(600, 400), game, 10, 400));

            SolidEntities.Add(new PassThroughPlatform(Chunk.Up,new Vector2(500, 670), game, 100, 10));

            this.Game = game;
        }

        public Chunk(Game1 game, int [,] tileset)
        {
            TileSet = tileset;
            SolidEntities = new List<Entity>();
            NonCollidingEntities = new List<Entity>();
            CollidingEntities = new List<Entity>();
            Game = game;
        }
        
        private void CallAll(Action<GameObject> func)
        {
            SolidEntities.ForEach(func);
            NonCollidingEntities.ForEach(func);
            CollidingEntities.ForEach(func);
        }
        
        public void LoadContent(ContentManager content)
        {
            CallAll(x => x.LoadContent(content));
        }

        public void Update(GameTime gameTime)
        {
            CallAll(x => x.Update(gameTime, this));
        }

        public void Draw(GameTime gameTime)
        {
            CallAll(x => x.Draw(gameTime));

            // FIXME: This will all be removed later and replaced by a
            //        full tilemap render method.
            for(int x = 0; x < TileSet.GetUpperBound(0); ++x)
            {
                for(int y = 0; y < TileSet.GetUpperBound(1); ++y)
                {
                    int type = TileSet[x, y];
                    if (tileDrawers.ContainsKey(type))
                    {
                        Vector2 tile = tileDrawers[type];
                        Vector2 pos = new Vector2(x, y) * TileSize + relPosition;
                        Game.SpriteEngine.Draw(new Rectangle((int)pos.X, (int)pos.Y, TileSize, TileSize));
                    }
                }
            }

        }

        public const int Up =        0b00000001;
        public const int Right =    0b00000010;
        public const int Down =        0b00000100;
        public const int Left =        0b00001000;

        // TODO: Tile collisions!!!
        public GameObject CollidePoint(Vector2 point)
        {
            foreach (var entity in SolidEntities)
            {
                if(entity.Contains(point))
                    return entity;
            }

            int x = (int)((point.X - relPosition.X) / TileSize);
            int y = (int)((point.Y - relPosition.Y) / TileSize);

            if(x < 0 || x > TileSet.GetUpperBound(0) || y < 0 || y > TileSet.GetUpperBound(1))
            {
                return null;
            }

            if(TileSet[x,y] == SolidPlatform)
            {
                return tileObjects[SolidPlatform];
            }

            return null;
        }
        
        public bool CollideSolid(Entity source, float timestep, out int direction, out float time, out RectangleF[] targetBB, out Vector2[] targetVel)
        {
            time = float.PositiveInfinity;
            direction = 0;
            bool corner = true;
            targetBB = new RectangleF[2];
            targetVel = new Vector2[2];

            foreach (var entity in SolidEntities)
            {
                bool tempCorner;
                float tempTime;
                int tempDirection;
                Vector2 velocity = (entity is Movable)? ((Movable)entity).Velocity : new Vector2();
                if (entity.Collide(source, timestep, out tempDirection, out tempTime, out tempCorner))
                {
                    if (tempTime < time || (tempTime == time && (corner && !tempCorner)))
                    {
                        corner = tempCorner;
                        time = tempTime;
                        direction = tempDirection;
                        if ((tempDirection & (Up | Down)) != 0)
                        {
                            targetBB[0] = entity.GetBoundingBox();
                            targetVel[0] = velocity;
                        }
                        else
                        {
                            targetBB[1] = entity.GetBoundingBox();
                            targetVel[1] = velocity;
                        }
                    }
                    if(tempTime == time && (corner || !tempCorner))
                    {
                        //Allows collisions with multiple directions
                        direction = direction | tempDirection;

                        if ((tempDirection & (Up | Down)) != 0)
                        {
                            targetBB[0] = entity.GetBoundingBox();
                            targetVel[0] = velocity;
                        }
                        else
                        {
                            targetBB[1] = entity.GetBoundingBox();
                            targetVel[1] = velocity;
                        }
                    }
                }
            }

            RectangleF motionBB;
            RectangleF sourceBB = source.GetBoundingBox();
            Vector2 sourceMotion = ((source is Movable) ? ((Movable)source).Velocity : new Vector2()) * timestep;

            motionBB.X = sourceBB.X + (float)Math.Min(0.0, sourceMotion.X);
            motionBB.Y = sourceBB.Y + (float)Math.Min(0.0, sourceMotion.Y);
            motionBB.Width = sourceBB.Width + (float)Math.Max(0.0, sourceMotion.X);
            motionBB.Height = sourceBB.Height + (float)Math.Max(0.0, sourceMotion.Y);

            int minX = (int)Math.Max(Math.Floor((motionBB.X - relPosition.X) / TileSize),0);
            int minY = (int)Math.Max(Math.Floor((motionBB.Y - relPosition.X) / TileSize),0);
            int maxX = (int)Math.Min(Math.Floor((motionBB.Right - relPosition.X) / TileSize) + 1,TileSet.GetUpperBound(0)+1);
            int maxY = (int)Math.Min(Math.Floor((motionBB.Bottom - relPosition.X) / TileSize) + 1, TileSet.GetUpperBound(0) + 1);

            if (source is Movable)
            {
                for (int x = minX; x < maxX; ++x)
                {
                    for (int y = minY; y < maxY; ++y)
                    {
                        if (TileSet[x,y] == Chunk.SolidPlatform) {
                            int tempDirection;
                            float tempTime;
                            bool tempCorner;

                            var tileBB = new RectangleF(x * TileSize + relPosition.X, y * TileSize + relPosition.Y, TileSize, TileSize);

                            if (BoxEntity.CollideMovable((Movable)source, tileBB, timestep, out tempDirection, out tempTime, out tempCorner))
                            {
                                if (tempTime < time || (tempTime == time && (corner && !tempCorner)))
                                {
                                    time = tempTime;
                                    direction = tempDirection;
                                    if ((tempDirection & (Up | Down)) != 0)
                                    {
                                        targetBB[0] = tileBB;
                                        targetVel[0] = new Vector2();
                                    }
                                    else
                                    {
                                        targetBB[1] = tileBB;
                                        targetVel[1] = new Vector2();
                                    }
                                }
                                if (tempTime == time && (corner || !tempCorner))
                                {
                                    
                                    //Allows collisions with multiple directions
                                    direction = direction | tempDirection;

                                    if ((tempDirection & (Up | Down)) != 0)
                                    {
                                        targetBB[0] = tileBB;
                                        targetVel[0] = new Vector2();
                                    }
                                    else
                                    {
                                        targetBB[1] = tileBB;
                                        targetVel[1] = new Vector2();
                                    }
                                }
                            };
                        }
                    }
                }
            }

            if (direction != 0)
                return true;

            return false;
        }
    }
}
