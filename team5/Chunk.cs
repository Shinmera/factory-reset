using System.Collections;
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
        ArrayList CollidingEntities;

        //Enemies, background objects
        ArrayList NonCollidingEntities;

        //things that will stop you like moving platforms (which are not part of the tileset)
        ArrayList SolidEntities;

        Game1 Game;

        //TESTING ONLY
        public Chunk(Game1 game, Player player)
        {
            SolidEntities = new ArrayList();
            NonCollidingEntities = new ArrayList();
            CollidingEntities = new ArrayList();

            NonCollidingEntities.Add(player);

            SolidEntities.Add(new Platform(new Vector2(100, 700), game, 600, 10));

            SolidEntities.Add(new Platform(new Vector2(600, 400), game, 10, 400));

            SolidEntities.Add(new Platform(new Vector2(500, 670), game, 100, 10));

            this.Game = game;
        }

        public Chunk(Game1 game, int [][] tileset)
        {
            TileSet = tileset;
            SolidEntities = new ArrayList();
            NonCollidingEntities = new ArrayList();
            CollidingEntities = new ArrayList();
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

        //TODO: Tile collisions
        public bool CollideSolid(Entity source, float timestep, out int direction, out float time, out Entity[] target)
        {

            time = float.PositiveInfinity;
            direction = 0;
            target = new Entity[2];

            foreach (var entity in SolidEntities)
            {
                float tempTime;
                int tempDirection;
                if (entity is BoxEntity)
                {
                    if (((BoxEntity)entity).Collide(source, timestep, out tempDirection, out tempTime))
                    {
                        if (tempTime < time)
                        {
                            time = tempTime;
                            direction = tempDirection;
                            if ((tempDirection & (Up | Down)) != 0)
                            {
                                target[0] = (BoxEntity)entity;
                            }
                            else
                            {
                                target[1] = (BoxEntity)entity;
                            }
                        }
                        if(tempTime == time)
                        {
                            //Allows collisions with multiple directions
                            direction = direction | tempDirection;

                            if ((tempDirection & (Up | Down)) != 0)
                            {
                                target[0] = (BoxEntity)entity;
                            }
                            else
                            {
                                target[1] = (BoxEntity)entity;
                            }
                        }
                    }
                }
            }

            if (target[0] != null || target[1] != null)
                return true;

            return false;
        }
    }
}
