using System.Collections.Generic;
using Microsoft.Xna.Framework;



namespace team5
{
    class Chunk
    {
        public const int Empty = 0;
        public const int SolidPlatform = 1;

        public const int TileSize = 10;

        public int[][] TileSet;

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
            SolidEntities = new List<Entity>();
            NonCollidingEntities = new List<Entity>();
            CollidingEntities = new List<Entity>();

            NonCollidingEntities.Add(player);

            SolidEntities.Add(new Platform(new Vector2(100, 700), game, 600, 10));

            SolidEntities.Add(new Platform(new Vector2(600, 400), game, 10, 400));

            SolidEntities.Add(new Platform(new Vector2(500, 670), game, 100, 10));

            this.Game = game;
        }

        public Chunk(Game1 game, int [][] tileset)
        {
            TileSet = tileset;
            SolidEntities = new List<Entity>();
            NonCollidingEntities = new List<Entity>();
            CollidingEntities = new List<Entity>();
            Game = game;
        }

        public void Update(GameTime gameTime)
        {
            foreach (var entity in SolidEntities)
            {
                ((Entity)entity).Update(gameTime, this);
            }

            foreach (var entity in NonCollidingEntities)
            {
                ((Entity)entity).Update(gameTime, this);
            }

            foreach (var entity in CollidingEntities)
            {
                ((Entity)entity).Update(gameTime, this);
            }
        }

        public void Draw(GameTime gameTime)
        {
            foreach (var entity in SolidEntities)
            {
                ((Entity)entity).Draw(gameTime, new Vector2());
            }

            foreach (var entity in NonCollidingEntities)
            {
                ((Entity)entity).Draw(gameTime, new Vector2());
            }

            foreach (var entity in CollidingEntities)
            {
                ((Entity)entity).Draw(gameTime, new Vector2());
            }

        }

        public const int Up =        0x00000001;
        public const int Right =    0x00000010;
        public const int Down =        0x00000100;
        public const int Left =        0x00001000;

        // TODO: Tile collisions!!!
        public Entity CollidePoint(Vector2 point)
        {
            foreach (var entity in SolidEntities)
            {
                if(entity.Contains(point))
                    return entity;
            }
            return null;
        }
        
        public bool CollideSolid(Entity source, float timestep, out int direction, out float time, out RectangleF[] targetBB, out Vector2[] targetVel)
        {
            time = float.PositiveInfinity;
            direction = 0;
            targetBB = new RectangleF[2];
            targetVel = new Vector2[2];

            foreach (var entity in SolidEntities)
            {
                float tempTime;
                int tempDirection;
                Vector2 velocity = (entity is Movable)? ((Movable)entity).Velocity : new Vector2();
                if (entity.Collide(source, timestep, out tempDirection, out tempTime))
                {
                    if (tempTime < time)
                    {
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
                            targetVel[0] = velocity;
                        }
                    }
                    if(tempTime == time)
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
                            targetVel[0] = velocity;
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
