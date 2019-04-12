using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

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
            Returning,
            Searching,
        };

        private const float DroneSize = 7;
        private const float ViewSize = 60;
        private const float MinMovement = 30;
        private const float PatrolSpeed = 50;
        private const float SearchSpeed = 100;
        private const float TargetSpeed = 150;
        private const float PatrolRange = 200;
        private const float SearchRange = 100;
        private const float SearchTime = 15;
        private const float WaitTime = 5;
        private const float WaitAngularVelocity = 0.375F * (float)Math.PI;
        private const float TurnAngularVelocity = 2F*(float)Math.PI;
        private const int WanderSearchAttempts = 10;

        private Vector2 Spawn;

        private ConeEntity ViewCone;

        private AnimatedSprite Sprite;
        public AIState State = AIState.Waiting;
        private List<Vector2> Path;
        private int NextNode;

        private Vector2 TargetLocation;
        private Vector2 WanderLocation;
        
        private float StateTimer = 0;

        private float Direction = 225;

        public AerialDrone(Vector2 position, Game1 game) : base(game, new Vector2(Chunk.TileSize*0.375F))
        {
            Spawn = position;
            Position = position;
            WanderLocation = Position;
            TargetLocation = Position;
            Sprite = new AnimatedSprite(null, game, new Vector2(Chunk.TileSize, Chunk.TileSize));
            ViewCone = new ConeEntity(game);
            ViewCone.FromDegrees(225, 90);
            ViewCone.Radius = Chunk.TileSize * 3;
            ViewCone.UpdatePosition(position);
            ViewCone.parent = this;
        }

        public void Target(Vector2 target, Chunk chunk)
        {
            if(State == AIState.Targeting && (TargetLocation-target).LengthSquared() < Chunk.TileSize*Chunk.TileSize*16 && Path.Count > 2)
            {
                return;
            }

            Velocity = new Vector2();

            int targetx = (int)Math.Floor((target.X - chunk.BoundingBox.X) / Chunk.TileSize);
            int targety = (int)Math.Floor((target.Y - chunk.BoundingBox.Y) / Chunk.TileSize);

            int startx = (int)Math.Floor((Position.X - chunk.BoundingBox.X) / Chunk.TileSize);
            int starty = (int)Math.Floor((Position.Y - chunk.BoundingBox.Y) / Chunk.TileSize);

            var newPath = FindReducedPath(chunk, FindPath(chunk, startx, starty, target), Size);

            if (newPath.Count <= 1)
            {

            }
            else {
                Path = newPath;

                NextNode = 1;

                State = AIState.Targeting;

                TargetLocation = target;
            }
        }

        public void Search(Vector2 target, Chunk chunk)
        {
            Path = null;
            StateTimer = 0;
            TargetLocation = target;
            WanderLocation = target;
            FindWander(TargetLocation, SearchRange, chunk);
            State = AIState.Searching;
        }

        public void Return(Chunk chunk)
        {
            Target(Spawn, chunk);
            State = AIState.Returning;
        }

        public void Wait()
        {
            Path = null;
            Velocity = new Vector2(0);
            StateTimer = 0;
            State = AIState.Waiting;
        }

        private bool MoveTo(Vector2 target, float speed)
        {
            Vector2 dir = (target - Position);

            if (dir.LengthSquared() <= 4 * Game1.DeltaT * Game1.DeltaT * speed * speed)
            {
                return true;
            }
            else
            {
                float targetDirection = (float)Math.Atan2(dir.Y, dir.X);
                if (ConeEntity.ConvertAngle(targetDirection - Direction) <= 2 * Game1.DeltaT * TurnAngularVelocity || ConeEntity.ConvertAngle(Direction - targetDirection) <= 2 * Game1.DeltaT * TurnAngularVelocity)
                {
                    Direction = targetDirection;
                    Velocity = dir;
                    Velocity.Normalize();
                    if (float.IsNaN(Velocity.X) || float.IsNaN(Velocity.Y))
                    {

                        Velocity = new Vector2(0);
                        return true;
                    }
                    Velocity *= speed;
                }
                else
                {
                    if(ConeEntity.ConvertAngle(targetDirection - Direction) < Math.PI)
                    {
                        Direction += Game1.DeltaT * TurnAngularVelocity;
                    }
                    else
                    {
                        Direction -= Game1.DeltaT * TurnAngularVelocity;
                    }
                }
            }

            return false;
        }

        private bool FindWander(Vector2 location, float distance, Chunk chunk)
        {
            float angle = (float)(Game.RNG.NextDouble() * 2 * Math.PI);

            var dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

            Vector2 p1 = Position;
            Vector2 p2 = p1 + dir;

            if(!ConeEntity.IntersectCircle(p1,p2, distance, location, float.PositiveInfinity, out float maxDist))
            {
                return false;
            }

            var point1 = new Vector2(dir.Y, -dir.X);
            point1.Normalize();
            point1 *= DroneSize;
            var point2 = -point1;

            if (chunk.IntersectLine(Position + point1, dir, maxDist, out float distToIntersect1, false)){
                maxDist = distToIntersect1;
            }

            if (chunk.IntersectLine(Position + point2, dir, maxDist, out float distToIntersect2, false))
            {
                maxDist = distToIntersect2;
            }

            maxDist -= DroneSize;

            if(maxDist < MinMovement)
            {
                return false;
            }
            else
            {
                Vector2 tentativeWanderLocation = Position + ((float)Game.RNG.NextDouble() * (maxDist - MinMovement) + MinMovement) * dir;
                if (chunk.BoundingBox.Contains(tentativeWanderLocation)){
                    WanderLocation = tentativeWanderLocation;
                    return true;
                }
                else
                {
                    return false;
                }
            }
                
        }

        public override void Respawn(Chunk chunk)
        {

            Position = Spawn;
            State = AIState.Waiting;
        }

        public override void Update(Chunk chunk)
        {
            Velocity = new Vector2();
            if (chunk.Level.Alarm.Drones)
            {
                Target(chunk.Level.Alarm.LastKnowPos, chunk);
                chunk.Level.Alarm.Drones = false;
            }
            
            switch (State)
            {
                case AIState.Patrolling:
                    if (MoveTo(WanderLocation, PatrolSpeed))
                    {
                        Wait();
                    }
                    break;
                case AIState.Searching:
                    if (MoveTo(WanderLocation, SearchSpeed))
                    {
                        Velocity = new Vector2(0);
                        FindWander(TargetLocation, PatrolRange, chunk);
                    }

                    StateTimer += Game1.DeltaT;
                    if(StateTimer >= SearchTime)
                    {
                        Return(chunk);
                    }
                    break;
                case AIState.Waiting:
                    StateTimer += Game1.DeltaT;
                    if (StateTimer <= 0.05F * WaitTime)
                    {

                    }
                    if (StateTimer <= 0.25F * WaitTime)
                    {
                        Direction += Game1.DeltaT * WaitAngularVelocity;
                    }
                    else if (StateTimer <= 0.3F * WaitTime)
                    {

                    }
                    else if (StateTimer <= 0.7F * WaitTime)
                    {
                        Direction -= Game1.DeltaT * WaitAngularVelocity;
                    }
                    else if (StateTimer <= 0.75F * WaitTime)
                    {

                    }
                    else if (StateTimer <= 0.95F * WaitTime)
                    {
                        Direction += Game1.DeltaT * WaitAngularVelocity;
                    }
                    else if (StateTimer <= WaitTime)
                    {

                    }
                    else
                    {
                        bool found = false;
                        for (int i = 0; i < WanderSearchAttempts; ++i)
                        {
                            if (FindWander(Spawn, PatrolRange, chunk))
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            Return(chunk);
                        }
                        else
                        {
                            State = AIState.Patrolling;
                        }
                    }
                    break;
                case AIState.Targeting:
                    if (MoveTo(Path[NextNode], TargetSpeed))
                    {
                        Velocity = new Vector2();
                        ++NextNode;
                        if (NextNode >= Path.Count)
                        {
                            Search(Path[NextNode - 1], chunk);
                        }
                        else
                        {
                            MoveTo(Path[NextNode], TargetSpeed);
                        }
                    }
                    break;
                case AIState.Returning:
                    if (MoveTo(Path[NextNode], PatrolSpeed))
                    {
                        ++NextNode;
                        if (NextNode >= Path.Count)
                        {
                            Wait();
                        }
                    }
                    break;
            }

            if (!chunk.Level.Player.IsHiding)
            {
                if (ViewCone.Collide(chunk.Level.Player, Game1.DeltaT, out int direction, out float time, out bool corner))
                {
                    chunk.Level.Alarm.Detected = false;
                    chunk.Level.Player.Kill();
                }
            }

            Position += Game1.DeltaT * Velocity;
            ViewCone.UpdatePosition(Position);
            ViewCone.Middle = Direction;
            ViewCone.Update(chunk);
            base.Update(chunk);
        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = content.Load<Texture2D>("Textures/camera");
            Sprite.Add("idle", 0, 4, 1.0);
        }

        public override void Draw()
        {
            ViewCone.Draw();
            Game.Transforms.Push();
            Game.Transforms.Rotate(ViewCone.Middle);
            Sprite.Draw(Position);
            Game.Transforms.Pop();
        }

        //TODO: Reduce curves as well.
        public List<Vector2> FindReducedPath(Chunk chunk, List<Vector2> path, Vector2 size)
        {
            var reducedPath = new List<Vector2>();

            if(path == null)
            {
                return reducedPath;
            }

            var lastDir = new Vector2(0);

            reducedPath.Add(Position);

            Vector2 lastPoint = reducedPath.Last();

            for (int i = path.Count-1; i > 0; --i)
            {
                /*
                if(lastDir != path[i - 1] - path[i])
                {
                    lastDir = path[i - 1] - path[i];
                    */
                var tentativePoint = path[i];

                var dir = tentativePoint - reducedPath.Last();
                var point1 = new Vector2(dir.Y , -dir.X);
                point1.Normalize();
                point1 *= DroneSize;
                var point2 = -point1;

                point1 += reducedPath.Last();
                point2 += reducedPath.Last();
                    

                if(chunk.IntersectLine(point1, dir, 1, out float location1) || chunk.IntersectLine(point2, dir, 1, out float location2))
                {

                    if (Path != null)
                    {
                        int node = Path.FindIndex(NextNode, x => x == lastPoint);
                        if (node != -1)
                        {
                            reducedPath = new List<Vector2>();
                            for (int p = NextNode - 1; p < node; ++p)
                            {
                                reducedPath.Add(Path[p]);
                            }
                        }
                        else
                        {
                            reducedPath.Add(lastPoint);
                        }
                    }
                    else
                    {
                        reducedPath.Add(lastPoint);
                    }
                }

                lastPoint = tentativePoint;
                //}
            }

            reducedPath.Add( path[0]);

            return reducedPath;
        }

        static List<Vector2> FindPath(Chunk chunk, int startx, int starty, Vector2 target)
        {
            int targetx = (int)Math.Floor((target.X - chunk.BoundingBox.X) / Chunk.TileSize);
            int targety = (int)Math.Floor((target.Y - chunk.BoundingBox.Y) / Chunk.TileSize);

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
                    return ReconstructPath(cameFrom, current, chunk, target);
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

        private static List<Vector2> ReconstructPath(Dictionary<Point, Point> cameFrom, Point current, Chunk chunk, Vector2 FirstPoint)
        {
            var path = new List<Vector2>();

            Vector2 offset = new Vector2(chunk.BoundingBox.X + Chunk.TileSize/2, chunk.BoundingBox.Y + Chunk.TileSize/2);

            path.Add(FirstPoint);

            path.Add(current.ToVector2()*Chunk.TileSize + offset);

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current.ToVector2()*Chunk.TileSize + offset);
            }

            return path;
        }

        private static float GetDist(int x1, int y1, int x2, int y2)
        {
            return (float)Math.Sqrt((float)(x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
    }
}
