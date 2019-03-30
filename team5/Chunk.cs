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

        //Tile refernces;                      AABBGGRR
        public const uint Empty         = 0xFFFFFFFF; // Nothing
        public const uint SolidPlatform = 0xFF000000; // A wall
        public const uint HidingSpot    = 0xFF404040; // A hiding spot for the player
        public const uint BackgroundWall= 0xFF808080; // A wall for enemies, but not the player
        public const uint FallThrough   = 0xFFC0C0C0; // A jump/fall-through platform
        public const uint PlayerStart   = 0xFF00FF00; // The start position for the player
        public const uint EnemyStart    = 0xFFFF0000; // An enemy spawn position
        public const uint Spike         = 0xFF0000FF; // A death spike
        public const uint Pickup        = 0xFF00FFFF; // An information pickup item
        public const uint Goal          = 0xFFFFFF00; // A goal tile leading to end-of-level

        public const int TileSize = 16;

        private readonly string TileSetName;
        public Texture2D TileSetTexture;
        public Vector2 SpawnPosition;

        public uint Width;
        public uint Height;
        public uint[] SolidTiles;
        public Texture2D Tileset;

        Dictionary<uint, TileType> tileObjects;

        //Viewcones, intelligence
        List<Entity> CollidingEntities;

        //Enemies, background objects
        List<Entity> NonCollidingEntities;

        //things that will stop you like moving platforms (which are not part of the tileset)
        List<Entity> SolidEntities;

        Game1 Game;

        //TESTING ONLY
        public Chunk(Game1 game, Player player, string tileSetName)
        {
            TileSetName = tileSetName;

            tileObjects = new Dictionary<uint, TileType>();
            tileObjects.Add(SolidPlatform, new TilePlatform(game));
            // FIXME: FallThrough, BackgroundWall

            SolidEntities = new List<Entity>();
            NonCollidingEntities = new List<Entity>();
            CollidingEntities = new List<Entity>();

            relPosition = new Vector2(0, 0);
            
            NonCollidingEntities.Add(player);

            this.Game = game;
        }

        public Chunk(Game1 game, string tileSetName)
        {
            TileSetName = tileSetName;
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

        public uint GetTile(int x, int y)
        {
            if(x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new IndexOutOfRangeException("No such tile exists");
            }

            return SolidTiles[(Height - y - 1)*Width + x];
        }

        public void LoadContent(ContentManager content)
        {
            TileSetTexture = content.Load<Texture2D>(TileSetName);
            Tileset = Game.TilemapEngine.CreateDebugTileset();

            Width = (uint)TileSetTexture.Width;
            Height = (uint)TileSetTexture.Height;

            SolidTiles = new uint[Width * Height];
            TileSetTexture.GetData<uint>(SolidTiles);
            
            // Scan through and populate
            for(int y=0; y<Height; ++y)
            {
                for(int x=0; x<Width; ++x)
                {
                    uint tile = GetTile(x, y);
                    switch(tile)
                    {
                        case PlayerStart: 
                            SpawnPosition = new Vector2(x * TileSize + relPosition.X - TileSize/2,
                                                        y * TileSize + relPosition.Y + TileSize/2);
                            break;
                        case EnemyStart:
                            NonCollidingEntities.Add(new Enemy(new Vector2(x * TileSize + relPosition.X - TileSize/2,
                                                                           y * TileSize + relPosition.Y - TileSize/2),
                                                               200, Game));
                            break;
                    }
                }
            }

            CallAll(obj => obj.LoadContent(content));
        }

        public void Update(GameTime gameTime)
        {
            CallAll(x => x.Update(gameTime, this));
        }

        public void Draw(GameTime gameTime)
        {
            CallAll(x => x.Draw(gameTime));
            
            Game.TilemapEngine.Draw(TileSetTexture, Tileset, relPosition);
        }

        public const int Up =        0b00000001;
        public const int Right =    0b00000010;
        public const int Down =        0b00000100;
        public const int Left =        0b00001000;

        public GameObject CollidePoint(Vector2 point)
        {
            foreach (var entity in SolidEntities)
            {
                if(entity.Contains(point))
                    return entity;
            }

            int x = (int)((point.X - relPosition.X) / TileSize);
            int y = (int)((point.Y - relPosition.Y) / TileSize);

            if(x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return null;
            }

            if(GetTile(x,y) == SolidPlatform)
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

            int minX = (int)Math.Max(Math.Floor((motionBB.Left - relPosition.X) / TileSize),0);
            int minY = (int)Math.Max(Math.Floor((motionBB.Bottom - relPosition.Y) / TileSize),0);
            int maxX = (int)Math.Min(Math.Floor((motionBB.Right - relPosition.X) / TileSize) + 1, Width + 1);
            int maxY = (int)Math.Min(Math.Floor((motionBB.Top - relPosition.Y) / TileSize) + 1, Height + 1);

            if (source is Movable)
            {
                for (int x = minX; x < maxX; ++x)
                {
                    for (int y = minY; y < maxY; ++y)
                    {
                        if (tileObjects.ContainsKey(GetTile(x,y))) {
                            int tempDirection;
                            float tempTime;
                            bool tempCorner;

                            var tileBB = new RectangleF(x * TileSize + relPosition.X - TileSize/2,
                                                        y * TileSize + relPosition.Y - TileSize/2, 
                                                        TileSize, TileSize);

                            if (tileObjects[GetTile(x,y)].Collide((Movable)source, tileBB, timestep, out tempDirection, out tempTime, out tempCorner))
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
