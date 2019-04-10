using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Content;

namespace team5
{
    class Chunk
    {
        public Level Level;

        // IMPORTANT: Identifiers need to be unique in the GGRR range
        //            Or there will be collisions in the debug visualisation.
        // Tile identifiers     AABBGGRR
        public enum Colors: uint{
            Empty          = 0xFFFFFFFF, // Nothing. All tiles with alpha 0 are also nothing
            SolidPlatform  = 0xFF000000, // A solid wall or platform
            HidingSpot     = 0xFF404040, // A hiding spot for the player
            BackgroundWall = 0xFF808080, // A wall for enemies, but not the player
            FallThrough    = 0xFFC0C0C0, // A jump/fall-through platform
            PlayerStart    = 0xFF00FF00, // The start position for the player
            StaticCamera   = 0xFFFF0100, // An enemy spawn position
            PivotCamera    = 0xFFFF0200, // An enemy spawn position
            GroundDrone    = 0xFFFF0300, // An enemy spawn position
            AerialDrone    = 0xFFFF0400, // An enemy spawn position
            Spike          = 0xFF0000FF, // A death spike
            Pickup         = 0xFF00EEFF, // An information pickup item
            Goal           = 0xFFFFEE00, // A goal tile leading to end-of-level
        }

        public const int TileSize = 16;
        public bool DrawSolids = false;

        private Game1 Game;
        private readonly string TileSetName;
        public Texture2D[] Layers;
        public Vector2 SpawnPosition;

        public uint Width;
        public uint Height;
        public readonly Vector2 Position;
        public Vector2 Size;
        public RectangleF BoundingBox;

        private Player Player;

        public uint[] SolidTiles;
        private Texture2D Tileset, Solidset;

        static Dictionary<uint, TileType> tileObjects;
        private String[] StoryItems; 

        //Viewcones, intelligence
        List<Entity> CollidingEntities = new List<Entity>();

        //Enemies, background objects
        List<Entity> NonCollidingEntities = new List<Entity>();

        //things that will stop you like moving platforms (which are not part of the tileset)
        List<Entity> SolidEntities = new List<Entity>();

        //things that will be removed at the end of the update (to ensure that collections are not modified during loops)
        List<Entity> PendingDeletion = new List<Entity>();


        public Chunk(Game1 game, Level level, LevelContent.Chunk chunk)
        {
            tileObjects = new Dictionary<uint, TileType>
            {
                { (uint)Colors.SolidPlatform, new TilePlatform(game) },
                { (uint)Colors.FallThrough, new TilePassThroughPlatform(game) },
                { (uint)Colors.BackgroundWall, new TileBackgroundWall(game) },
                { (uint)Colors.Spike, new TileSpike(game) },
                { (uint)Colors.HidingSpot, new TileHidingSpot(game) }
            };

            Game = game;
            Level = level;
            Position = new Vector2(chunk.position[0], chunk.position[1]);
            Layers = chunk.maps;
            TileSetName = chunk.tileset;
            StoryItems = chunk.storyItems;
        }
        
        public void Activate(Player player)
        {
            Player = player;
        }

        public void Deactivate()
        {
            Player = null;
        }

        public void Die(Entity entity)
        {
            if (entity is Player)
            {
                CallAll((GameObject x) => { if (x is Entity) ((Entity)x).Respawn(this); });
            }

            if(entity is Pickup)
            {
                PendingDeletion.Add(entity);
            }
        }

        private void CallAll(Action<GameObject> func)
        {
            SolidEntities.ForEach(func);
            NonCollidingEntities.ForEach(func);
            CollidingEntities.ForEach(func);
            if (Player != null)
                func.Invoke(Player);
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
            Tileset = Game.TilemapEngine.Tileset(TileSetName);
            Solidset = Game.TilemapEngine.Tileset("solid");

            Width = (uint)Layers[0].Width;
            Height = (uint)Layers[0].Height;
            Size = new Vector2((Width*TileSize)/2, (Height*TileSize)/2);
            BoundingBox = new RectangleF(Position, Size);

            SolidTiles = new uint[Width * Height];
            Layers[0].GetData<uint>(SolidTiles);

            // Scan through and populate
            for (int y=0; y<Height; ++y)
            {
                for(int x=0; x<Width; ++x)
                {
                    uint tile = GetTile(x, y);

                    if (Enum.IsDefined(typeof(Colors), tile))
                    {
                        Vector2 position = new Vector2(x * TileSize + BoundingBox.X + TileSize / 2,
                                                        y * TileSize + BoundingBox.Y + TileSize / 2);

                        switch (tile)
                        {
                            case (uint)Colors.PlayerStart:
                                SpawnPosition = new Vector2(x * TileSize + BoundingBox.X + TileSize / 2,
                                                            y * TileSize + BoundingBox.Y + TileSize);
                                break;
                            // FIXME: this is way too verbose, factor out.
                            case (uint)Colors.StaticCamera:{
                                bool left = (GetTile(x-1, y) == (uint)Colors.SolidPlatform);
                                NonCollidingEntities.Add(new StaticCamera(position, (left)? 315 : 225, Game));
                                break;}
                            case (uint)Colors.PivotCamera:
                                NonCollidingEntities.Add(new PivotCamera(position, Game));
                                break;
                            case (uint)Colors.GroundDrone:
                                NonCollidingEntities.Add(new GroundDrone(position, Game));
                                break;
                            case (uint)Colors.Pickup:
                                CollidingEntities.Add(new Pickup(position, Game));
                                break;
                            case (uint)Colors.AerialDrone:
                                var drone = new AerialDrone(position, Game);
                                NonCollidingEntities.Add(drone);
                                if(Player != null)
                                {
                                    drone.Target(Player.Position, this);
                                }
                                break;
                        }
                    }
                    

                    
                }
            }

            CallAll(obj => obj.LoadContent(content));
        }

        public void Update()
        {
            CallAll(x => x.Update(this));

            PendingDeletion.ForEach(x => 
            {
                if (!CollidingEntities.Remove(x))
                {
                    if (!NonCollidingEntities.Remove(x))
                        SolidEntities.Remove(x);
                }
            });
        }

        public void Draw()
        {
            if(DrawSolids)
            {
                Game.TilemapEngine.Draw(Layers[0], Solidset, new Vector2(BoundingBox.X, BoundingBox.Y));
                CallAll(x => x.Draw());
            }
            else
            {
                Game.TilemapEngine.Draw(Layers[1], Tileset, new Vector2(BoundingBox.X, BoundingBox.Y));
                CallAll(x => x.Draw());
                for(int i=2; i<Layers.Length; ++i)
                    Game.TilemapEngine.Draw(Layers[i], Tileset, new Vector2(BoundingBox.X, BoundingBox.Y));
            }
        }

        public const int Up =        0b00000001;
        public const int Right =     0b00000010;
        public const int Down =      0b00000100;
        public const int Left =      0b00001000;

        public bool AtHidingSpot(Movable source, out Vector2 location)
        {
            var sourceBB = source.GetBoundingBox();

            int minX = (int)Math.Max(Math.Floor((sourceBB.Left - BoundingBox.X) / TileSize), 0);
            int minY = (int)Math.Max(Math.Floor((sourceBB.Bottom - BoundingBox.Y) / TileSize), 0);
            int maxX = (int)Math.Min(Math.Floor((sourceBB.Right - BoundingBox.X) / TileSize) + 1, Width + 1);
            int maxY = (int)Math.Min(Math.Floor((sourceBB.Top - BoundingBox.Y) / TileSize) + 1, Height + 1);

            float closestSqr = float.PositiveInfinity;
            int xpos = -1;
            int ypos = -1;

            for (int x = minX; x < maxX; ++x)
            {
                for (int y = minY; y < maxY; ++y)
                {
                    if(GetTile(x,y) == (uint)Colors.HidingSpot)
                    {
                        Vector2 tilePos = new Vector2(x * TileSize + BoundingBox.X + TileSize / 2, y * TileSize + BoundingBox.Y + TileSize / 2);

                        float sqrdist = (tilePos-source.Position).LengthSquared();
                        if(sqrdist < closestSqr)
                        {
                            closestSqr = sqrdist;
                            xpos = x;
                            ypos = y;
                        }
                    }
                }
            }

            if(closestSqr == float.PositiveInfinity)
            {
                location = new Vector2(-1, -1);
                return false;
            }

            while (GetTile(xpos, --ypos) == (uint)Colors.HidingSpot);

            location = new Vector2(xpos * TileSize + BoundingBox.X + TileSize / 2, (ypos+1) * TileSize + BoundingBox.Y + TileSize / 2);
            return true;
        }

        //Dictionary is point to cw/ccw line
        public SortedDictionary<Vector2,Tuple<Vector2,Vector2>> BuildLOSHelper(RectangleF BoundingBox, Vector2 pos, float radius, Vector2 dir1, Vector2 dir2, out Dictionary<Vector2,float> angles)
        {
            float radiusSqr = radius * radius;
            float Cross2(Vector2 x1, Vector2 x2) => x1.X * x2.Y - x1.Y * x2.X;

            bool acute = Cross2(dir1, dir2) > 0;

            float startingangle = ConeEntity.ConvertAngle((float)Math.Atan2(dir1.Y, dir1.X));
            float endingangle = ConeEntity.ConvertAngle((float)Math.Atan2(dir2.Y, dir2.X));

            var localAngles = new Dictionary<Vector2, float>();

            bool inRange(Vector2 p)
            {
                Vector2 dirp = p - pos;
                return dirp.LengthSquared() <= radiusSqr && localAngles[p] > 0 && localAngles[p] > endingangle-startingangle;
            }

            var points = new SortedDictionary<Vector2, Tuple<Vector2, Vector2>>(Comparer<Vector2>.Create((Vector2 p1, Vector2 p2) => {
                var angle1 = localAngles[p1];
                var angle2 = localAngles[p2];
                if(angle1 == angle2)
                {
                    return 0;
                }
                else
                {
                    return angle1 < angle2 ? -1 : 1;
                }
            }));

            int minX = (int)Math.Max(Math.Floor((BoundingBox.Left - BoundingBox.X) / TileSize), 0);
            int minY = (int)Math.Max(Math.Floor((BoundingBox.Bottom - BoundingBox.Y) / TileSize), 0);
            int maxX = (int)Math.Min(Math.Floor((BoundingBox.Right - BoundingBox.X) / TileSize) + 1, Width);
            int maxY = (int)Math.Min(Math.Floor((BoundingBox.Top - BoundingBox.Y) / TileSize) + 1, Height);

            var offset = new Vector2(BoundingBox.X, BoundingBox.Y);

            for(int x = minX; x < maxX; ++x)
            {
                for(int y = minY; y < maxY; ++y)
                {
                    if(GetTile(x,y) == (uint)Colors.SolidPlatform || GetTile(x,y) == (uint)Colors.BackgroundWall)
                    {
                        Vector2 tilePosition = offset + new Vector2(x + 0.5F, y + 0.5F) * TileSize;
                        var dir = pos - tilePosition;
                        var cornerOffset = new Vector2(dir.X > 0 ? 1 : -1, dir.Y > 0 ? 1 : -1);

                        Vector2 dircorner = dir + new Vector2(TileSize/2,TileSize/2) + cornerOffset * TileSize / 2;

                        if(Math.Sign(dir.X) == Math.Sign(dircorner.X)
                            && !(GetTile((int)cornerOffset.X + x, y) == (uint)Colors.SolidPlatform 
                            || GetTile((int)cornerOffset.X + x, y) == (uint)Colors.BackgroundWall))
                        {
                            Vector2 point1;
                            Vector2 point2;
                            if (cornerOffset.X == cornerOffset.Y)
                            {
                                point1 = tilePosition + cornerOffset * TileSize / 2;
                                point2 = tilePosition + new Vector2(cornerOffset.X, -cornerOffset.Y) * TileSize / 2;
                            }
                            else
                            {
                                point2 = tilePosition + cornerOffset * TileSize / 2;
                                point1 = tilePosition + new Vector2(cornerOffset.X, -cornerOffset.Y) * TileSize / 2;
                            }

                            if (!points.ContainsKey(point1))
                            {
                                localAngles[point1] = ConeEntity.ConvertAngle((float)Math.Atan2(point1.Y - pos.Y, point1.X - pos.X) - startingangle);
                            }

                            if (!points.ContainsKey(point2))
                            {
                                localAngles[point2] = ConeEntity.ConvertAngle((float)Math.Atan2(point2.Y - pos.Y, point2.X - pos.X) - startingangle);
                            }

                            if (inRange(point1) || inRange(point2))
                            {
                                if(!inRange(point1) || !inRange(point2))
                                {
                                    if(!ConeEntity.IntersectCircle(point1,point2, radius, pos, out float t))
                                    { 
                                        
                                    }
                                }
                                var point1Entry = points.ContainsKey(point1) ? points[point1].Item1 : new Vector2(float.NaN);
                                var point2Entry = points.ContainsKey(point2) ? points[point2].Item2 : new Vector2(float.NaN);

                                points[point1] = new Tuple<Vector2, Vector2>(point1Entry, point2);
                                points[point2] = new Tuple<Vector2, Vector2>(point1, point2Entry);
                            }
                        }

                        if (Math.Sign(dir.Y) == Math.Sign(dircorner.Y)
                            && !(GetTile(x, (int)cornerOffset.Y + y) == (uint)Colors.SolidPlatform
                            || GetTile(x, (int)cornerOffset.Y + y) == (uint)Colors.BackgroundWall))
                        {
                            Vector2 point1;
                            Vector2 point2;
                            if (cornerOffset.X == cornerOffset.Y)
                            {
                                point2 = tilePosition + cornerOffset * TileSize / 2;
                                point1 = tilePosition + new Vector2(-cornerOffset.X, cornerOffset.Y) * TileSize / 2;
                            }
                            else
                            {
                                point1 = tilePosition + cornerOffset * TileSize / 2;
                                point2 = tilePosition + new Vector2(-cornerOffset.X, cornerOffset.Y) * TileSize / 2;
                            }

                            if (!points.ContainsKey(point1))
                            {
                                localAngles[point1] = ConeEntity.ConvertAngle((float)Math.Atan2(point1.Y - pos.Y, point1.X - pos.X) - startingangle);
                            }

                            if (!points.ContainsKey(point2))
                            {
                                localAngles[point2] = ConeEntity.ConvertAngle((float)Math.Atan2(point2.Y - pos.Y, point2.X - pos.X) - startingangle);
                            }

                            if (inRange(point1) || inRange(point2))
                            {
                                var point1Entry = points.ContainsKey(point1) ? points[point1].Item1 : new Vector2(float.NaN);
                                var point2Entry = points.ContainsKey(point2) ? points[point2].Item2 : new Vector2(float.NaN);

                                points[point1] = new Tuple<Vector2, Vector2>(point1Entry, point2);
                                points[point2] = new Tuple<Vector2, Vector2>(point1, point2Entry);
                            }
                        }
                    }
                }
            }

            angles = localAngles;

            return points;
        }

        public List<TileType> TouchingNonSolidTile(Movable source)
        {
            var result = new List<TileType>();

            var sourceBB = source.GetBoundingBox();

            int minX = (int)Math.Max(Math.Floor((sourceBB.Left - BoundingBox.X) / TileSize), 0);
            int minY = (int)Math.Max(Math.Floor((sourceBB.Bottom - BoundingBox.Y) / TileSize), 0);
            int maxX = (int)Math.Min(Math.Floor((sourceBB.Right - BoundingBox.X) / TileSize) + 1, Width);
            int maxY = (int)Math.Min(Math.Floor((sourceBB.Top - BoundingBox.Y) / TileSize) + 1, Height);

            for (int x = minX; x < maxX; ++x)
            {
                for (int y = minY; y < maxY; ++y)
                {
                    if (tileObjects.ContainsKey(GetTile(x, y)))
                    {
                        var tile = tileObjects[GetTile(x, y)];
                        if (!(tile is TileSolid))
                        {
                            result.Add(tile);
                        }

                    }
                }
            }

            return result;
        }

        public GameObject CollidePoint(Vector2 point)
        {
            foreach (var entity in SolidEntities)
            {
                if(entity.Contains(point))
                    return entity;
            }

            int x = (int)Math.Floor((point.X - BoundingBox.X) / TileSize);
            int y = (int)Math.Floor((point.Y - BoundingBox.Y) / TileSize);

            if(x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return null;
            }

            if(GetTile(x,y) == (uint)Colors.SolidPlatform)
            {
                return tileObjects[(uint)Colors.SolidPlatform];
            }

            return null;
        }
        
        public bool IntersectLine(Vector2 Source, Vector2 Dir, float length, out float location)
        {
            Vector2 relSource = Source - new Vector2(BoundingBox.X, BoundingBox.Y);

            Vector2 relEnd = Dir * length + relSource;

            int minX = (int)Math.Max(Math.Floor(Math.Min(relSource.X, relEnd.X) / TileSize), 0);
            int minY = (int)Math.Max(Math.Floor(Math.Min(relSource.Y, relEnd.Y) / TileSize), 0);

            int maxX = (int)Math.Min(Math.Floor(Math.Max(relSource.X, relEnd.X) / TileSize) + 1, Width);
            int maxY = (int)Math.Min(Math.Floor(Math.Max(relSource.Y, relEnd.Y) / TileSize) + 1, Height);

            float prevLength = 0;
            float nextLength = 0;

            int xincrement = Dir.X > 0 ? 1 : -1;
            int yincrement = Dir.Y > 0 ? 1 : -1;

            for (int x = (Dir.X > 0 ? minX : (maxX-1)); x < maxX && x >= minX; x += xincrement)
            {
                Vector2 prevPos = relSource + Dir * prevLength;

                int localMinY;
                int localMaxY;

                if(Dir.X == 0)
                {
                    localMinY = (int)Math.Max(Math.Floor(Math.Min(relSource.Y, relEnd.Y) / TileSize), minY);
                    localMaxY = (int)Math.Min(Math.Floor(Math.Max(relSource.Y, relEnd.Y) / TileSize) + 1, maxY);
                    nextLength = float.PositiveInfinity;
                }
                else if (Dir.X > 0)
                {
                    float distToNextX = ((x + 1) * TileSize - prevPos.X) / Dir.X;

                    nextLength = distToNextX + prevLength;

                    Vector2 nextPos = relSource + Dir * nextLength;

                    localMinY = (int)Math.Max(Math.Floor(Math.Min(prevPos.Y, nextPos.Y) / TileSize), minY);
                    localMaxY = (int)Math.Min(Math.Floor(Math.Max(prevPos.Y, nextPos.Y) / TileSize) + 1, maxY);
                }
                else
                {
                    float distToNextX = (x * TileSize - prevPos.X) / Dir.X;

                    nextLength = distToNextX + prevLength;

                    Vector2 nextPos = relSource + Dir * nextLength;

                    localMinY = (int)Math.Max(Math.Floor(Math.Min(prevPos.Y, nextPos.Y) / TileSize), minY);
                    localMaxY = (int)Math.Min(Math.Floor(Math.Max(prevPos.Y, nextPos.Y) / TileSize) + 1, maxY);
                }

                for(int y = (Dir.Y > 0 ? localMinY : (localMaxY - 1)); y < localMaxY && y >= localMinY; y += yincrement)
                {
                    if(GetTile(x,y) == (uint)Colors.SolidPlatform || GetTile(x, y) == (uint)Colors.BackgroundWall)
                    {
                        if(y == (Dir.Y > 0 ? localMinY : (localMaxY - 1)))
                        {
                            location = prevLength;
                            return true;
                        }

                        if(Dir.Y == 0)
                        {
                            location = prevLength;
                            if (location < length)
                            {
                                return true;
                            }
                            else
                            {
                                location = -1;
                                return false;
                            }
                        }

                        float yPos = Dir.Y > 0 ? y * TileSize : (y + 1) * TileSize;

                        location = (yPos - relSource.Y) / Dir.Y;

                        if (location < length)
                        {
                            return true;
                        }
                        else
                        {
                            location = -1;
                            return false;
                        }
                    }
                }

                prevLength = nextLength;
            }
            

            location = -1;
            return false;
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

            motionBB.X = sourceBB.X + (float)Math.Min(0.0F, sourceMotion.X);
            motionBB.Y = sourceBB.Y + (float)Math.Min(0.0F, sourceMotion.Y);
            motionBB.Width = sourceBB.Width + Math.Abs(sourceMotion.X);
            motionBB.Height = sourceBB.Height + Math.Abs(sourceMotion.Y);

            int minX = (int)Math.Max(Math.Floor((motionBB.Left - BoundingBox.X - 1) / TileSize), 0);
            int minY = (int)Math.Max(Math.Floor((motionBB.Bottom - BoundingBox.Y - 1) / TileSize), 0);
            int maxX = (int)Math.Min(Math.Floor((motionBB.Right - BoundingBox.X + 1) / TileSize) + 1, Width);
            int maxY = (int)Math.Min(Math.Floor((motionBB.Top - BoundingBox.Y + 1) / TileSize) + 1, Height);

            if (source is Movable)
            {
                for (int x = minX; x < maxX; ++x)
                {
                    for (int y = minY; y < maxY; ++y)
                    {
                        if (tileObjects.ContainsKey(GetTile(x,y))) {
                            var tile = tileObjects[GetTile(x, y)];

                            if (tile is TileSolid)
                            {

                                int tempDirection;
                                float tempTime;
                                bool tempCorner;

                                var tileBB = new RectangleF(x * TileSize + BoundingBox.X,
                                                            y * TileSize + BoundingBox.Y,
                                                            TileSize, TileSize);

                                if (((TileSolid)tile).Collide((Movable)source, tileBB, timestep, out tempDirection, out tempTime, out tempCorner))
                                {
                                    if (tempTime < time || (tempTime == time && (corner && !tempCorner)))
                                    {
                                        corner = tempCorner;
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
                                    else if (tempTime == time && (corner || !tempCorner))
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
            }

            if (direction != 0)
                return true;

            return false;
        }
    }
}
