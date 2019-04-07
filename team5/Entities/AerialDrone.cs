using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    class AerialDrone : BoxEntity
    {
        /// <summary> The Velocity of this Entity </summary>
        public Vector2 Velocity = new Vector2();

        public enum AIState
        {
            Patrolling,
            Waiting,
            Targeting,
        };

        private const float ViewSize = 60;
        private float Direction = 225;
        private readonly float PatrolSpeed = 50;

        private AnimatedSprite Sprite;
        private AIState State = AIState.Waiting;
        private List<Vector2> Path;
        private int NextNode;

        private Vector2 Spawn;

        private ConeEntity ViewCone;

        public AerialDrone(Vector2 position, Game1 game) : base(game, new Vector2(Chunk.TileSize*0.75F))
        {
            Spawn = position;
            Position = position;
            Sprite = new AnimatedSprite(null, game, new Vector2(Chunk.TileSize, Chunk.TileSize));
            ViewCone = new ConeEntity(game);
            ViewCone.FromDegrees(225, 90);
            ViewCone.Radius = Chunk.TileSize * 3;
            ViewCone.UpdatePosition(position);
        }

        public void Target(Vector2 target, Chunk chunk)
        {
            int targetx = (int)Math.Floor((target.X - chunk.BoundingBox.X) / Chunk.TileSize);
            int targety = (int)Math.Floor((target.Y - chunk.BoundingBox.Y) / Chunk.TileSize);

            int startx = (int)Math.Floor((Position.X - chunk.BoundingBox.X) / Chunk.TileSize);
            int starty = (int)Math.Floor((Position.Y - chunk.BoundingBox.Y) / Chunk.TileSize);

            Path = FindReducedPath(chunk, FindPath(chunk, startx, starty, targetx, targety));

            NextNode = 1;

            State = AIState.Targeting;

            if(Path.Count <= 1)
            {
                State = AIState.Waiting;
            }
        }

        public override void Respawn(Chunk chunk)
        {
            Position = Spawn;
            State = AIState.Waiting;
        }

        public override void Update(GameTime gameTime, Chunk chunk)
        {

            float dt = Game1.DeltaT;

            switch (State)
            {
                case AIState.Patrolling:
                    break;
                case AIState.Waiting:
                    Vector2 playerPos = chunk.Level.Player.Position;
                    Velocity = new Vector2(0);
                    Target(playerPos, chunk);
                    break;
                case AIState.Targeting:
                    Vector2 dir = Path[NextNode] - Position;
                    if(dir.LengthSquared() < 1F)
                    {
                        ++NextNode;
                        if(NextNode >= Path.Count)
                        {
                            Vector2 playerPosT = chunk.Level.Player.Position;
                            Target(playerPosT, chunk);
                        }
                    }
                    else
                    {
                        Direction = (float)Math.Atan2(dir.Y, dir.X);
                        Velocity = dir;
                        Velocity.Normalize();
                        Velocity *= PatrolSpeed;
                    }
                    break;
            }

            
            Position += dt * Velocity;
            ViewCone.UpdatePosition(Position);
            ViewCone.Middle = Direction;
            ViewCone.Update(gameTime, chunk);
            base.Update(gameTime, chunk);
        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = content.Load<Texture2D>("Textures/camera");
            Sprite.Add("idle", 0, 4, 1.0);
        }

        public override void Draw(GameTime gameTime)
        {
            ViewCone.Draw(gameTime);
            Game.Transforms.Push();
            Game.Transforms.Rotate(ViewCone.Middle);
            Sprite.Draw(Position);
            Game.Transforms.Pop();
        }

        //TODO: Reduce curves as well.
        static List<Vector2> FindReducedPath(Chunk chunk, List<Point> path)
        {
            var ReducedPath = new List<Vector2>();

            Point lastDir = new Point(0);

            if(path == null)
            {
                return ReducedPath;
            }

            for(int i = path.Count-1; i > 0; --i)
            {
                if(lastDir != path[i - 1] - path[i])
                {
                    lastDir = path[i - 1] - path[i];
                    ReducedPath.Add(new Vector2(chunk.BoundingBox.X, chunk.BoundingBox.Y) + new Vector2(Chunk.TileSize/2) + path[i].ToVector2() * Chunk.TileSize);
                }
            }

            ReducedPath.Add(new Vector2(chunk.BoundingBox.X, chunk.BoundingBox.Y) + new Vector2(Chunk.TileSize / 2) + path[0].ToVector2() * Chunk.TileSize);

            return ReducedPath;
        }

        static List<Point> FindPath(Chunk chunk, int startx, int starty, int targetx, int targety)
        {
            var path = new List<Point>();

            float sqrt2 = (float)Math.Sqrt(2);

            var cameFrom = new Dictionary<Point, Point>();

            var gScore = new Dictionary<Point, float>
            {
                [new Point(startx, starty)] = 0
            };

            var fScore = new Dictionary<Point, float>
            {
                { new Point(startx, starty), GetDist(startx, starty, targetx, targety) }
            };

            var closedSet = new HashSet<Point>();

            var openSet = new SortedSet<Point>(Comparer<Point>.Create((Point x, Point y) => {
                if (x == y)
                {
                    return 0;
                }

                if(!fScore.TryGetValue(x,out float scoreX))
                {
                    scoreX = float.PositiveInfinity;
                }

                if (!fScore.TryGetValue(y, out float scoreY))
                {
                    scoreY = float.PositiveInfinity;
                }

                if (scoreX == scoreY)
                {
                    if(x.X == y.X)
                    {
                        return x.Y > y.Y ? 1 : -1;
                    }
                    else
                    {
                        return x.X > y.X ? 1 : -1;
                    }
                }
                return scoreX > scoreY ? 1 : -1;
            }))
            {
                {new Point(startx, starty) }
            };

            while (openSet.Count > 0)
            {

                Point current = openSet.First();

                if(current.X == targetx && current.Y == targety)
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);
                closedSet.Add(current);

                for(int xoffset = -1; xoffset <= 1; ++xoffset)
                {
                    for(int yoffset = -1; yoffset <= 1; ++yoffset)
                    {
                        Point neighbor = current + new Point(xoffset, yoffset);
                        if ((xoffset == 0 && yoffset == 0) || neighbor.X < 0 || neighbor.X >= chunk.Width || neighbor.Y < 0 || neighbor.Y >= chunk.Height
                            || chunk.GetTile(neighbor.X,neighbor.Y) == (uint)Chunk.Colors.SolidPlatform)
                        {
                            continue;
                        }

                        if(Math.Abs(xoffset) + Math.Abs(yoffset) == 2)
                        {
                            if(chunk.GetTile(current.X + xoffset, current.Y) == (uint)Chunk.Colors.SolidPlatform
                                || chunk.GetTile(current.X, current.Y + yoffset) == (uint)Chunk.Colors.SolidPlatform)
                            {
                                continue;
                            }
                        }

                        if(closedSet.Contains(neighbor)){
                            continue;
                        }

                        float tentative_gScore = gScore[current] + (Math.Abs(xoffset) + Math.Abs(yoffset) <= 1 ? 1 : sqrt2);

                        if (!openSet.Contains(neighbor))
                        {
                            cameFrom.Add(neighbor, current);
                            gScore.Add(neighbor, tentative_gScore);
                            fScore.Add(neighbor, gScore[neighbor] + GetDist(neighbor.X, neighbor.Y, targetx, targety));
                            openSet.Add(neighbor);
                        } else {
                            bool found = gScore.TryGetValue(neighbor, out float neighborScore);
                            if (found && tentative_gScore >= neighborScore)
                            {
                                continue;
                            }

                            openSet.Remove(neighbor);

                            cameFrom[neighbor] = current;
                            gScore[neighbor] = tentative_gScore;
                            fScore[neighbor] = gScore[neighbor] + GetDist(neighbor.X, neighbor.Y, targetx, targety);

                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            return null;
        }

        private static List<Point> ReconstructPath(Dictionary<Point, Point> cameFrom, Point current)
        {
            var path = new List<Point>();

            path.Add(current);

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            return path;
        }

        private static float GetDist(int x1, int y1, int x2, int y2)
        {
            return (float)Math.Sqrt((float)(x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
    }
}
