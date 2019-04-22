using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace team5
{
    class AerialDrone : BoxEntity, IEnemy
    {
        #region Constants and Enums

        public enum AIState
        {
            //Wanders around the spawn
            Patrolling,
            //Waits, while rotating its camera
            Waiting,
            //Moving towards a target location
            Targeting,
            //Returning to spawn
            Returning,
            //Wandering around a target location
            Searching,
            //Checking a heard sound
            Investigating,
            //Checking a heard sound
            CursorySearching,
        };

        /// <summary> The range of the viewcone</summary>
        private const float ViewSize = 60;
        /// <summary> The minimum distance the drone will move in one wander segment</summary>
        private const float MinMovement = 30;
        /// <summary> The speed of the drone while patrolling</summary>
        private const float PatrolSpeed = 50;
        /// <summary> The speed of the drone while actively searching the area</summary>
        private const float SearchSpeed = 80;
        /// <summary> The speed of the drone when moving towards a target</summary>
        private const float TargetSpeed = 120;
        /// <summary> How far the drone will patrol from its spawn location</summary>
        private const float PatrolRange = 160;
        /// <summary> How far away from a target location the drone will search</summary>
        private const float SearchRange = 160;
        /// <summary> How long the drone will search before giving up after an investigation</summary>
        private const float SearchTime = 10;
        /// <summary> How long the drone will wait before wandering to a new spot (in patrol mode)</summary>
        private const float WaitTime = 5;
        /// <summary> How fast the drone turns during idling</summary>
        private const float WaitAngularVelocity = 0.375F * (float)Math.PI;
        /// <summary> How fast the drone turns to reach a new location</summary>
        private const float TurnAngularVelocity = 2F*(float)Math.PI;
        /// <summary> The amount of times the drone will attempt to find a new location to wander to before giving up and returning to spawn.</summary>
        private const int WanderSearchAttempts = 15;

        private const float BaseVolume = 100;
        private const float MinVolume = 7;
        private const float AlertVolume = 4;
        private const float HearingPrecision = Chunk.TileSize * 8;

        private int PositionKnown = 0;
        #endregion

        #region Static Methods

        private static float GetDist(int x1, int y1, int x2, int y2)
        {
            return (float)Math.Sqrt((float)(x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        /// <summary> Finds a path from a source to a target within a chunk. Path is in reverse order.</summary>
        public static List<Vector2> FindPath(Chunk chunk, int startx, int starty, Vector2 target)
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

                if (!fScore.TryGetValue(x, out float scoreX))
                {
                    scoreX = float.PositiveInfinity;
                }

                if (!fScore.TryGetValue(y, out float scoreY))
                {
                    scoreY = float.PositiveInfinity;
                }

                if (scoreX == scoreY)
                {
                    if (x.X == y.X)
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

                if (current.X == targetx && current.Y == targety)
                {
                    return ReconstructPath(cameFrom, current, chunk, target);
                }

                openSet.Remove(current);
                closedSet.Add(current);

                for (int xoffset = -1; xoffset <= 1; ++xoffset)
                {
                    for (int yoffset = -1; yoffset <= 1; ++yoffset)
                    {
                        Point neighbor = current + new Point(xoffset, yoffset);
                        if ((xoffset == 0 && yoffset == 0) || neighbor.X < 0 || neighbor.X >= chunk.Width || neighbor.Y < 0 || neighbor.Y >= chunk.Height
                            || chunk.GetTile(neighbor.X, neighbor.Y) == (uint)Chunk.Colors.SolidPlatform)
                        {
                            continue;
                        }

                        if (Math.Abs(xoffset) + Math.Abs(yoffset) == 2)
                        {
                            if (chunk.GetTile(current.X + xoffset, current.Y) == (uint)Chunk.Colors.SolidPlatform
                                || chunk.GetTile(current.X, current.Y + yoffset) == (uint)Chunk.Colors.SolidPlatform)
                            {
                                continue;
                            }
                        }

                        if (closedSet.Contains(neighbor))
                        {
                            continue;
                        }

                        float tentative_gScore = gScore[current] + (Math.Abs(xoffset) + Math.Abs(yoffset) <= 1 ? 1 : sqrt2);

                        if (!openSet.Contains(neighbor))
                        {
                            cameFrom.Add(neighbor, current);
                            gScore.Add(neighbor, tentative_gScore);
                            fScore.Add(neighbor, gScore[neighbor] + GetDist(neighbor.X, neighbor.Y, targetx, targety));
                            openSet.Add(neighbor);
                        }
                        else
                        {
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

        /// <summary> Reconstructs a path based on a connection graph</summary>
        private static List<Vector2> ReconstructPath(Dictionary<Point, Point> cameFrom, Point current, Chunk chunk, Vector2 FirstPoint)
        {
            var path = new List<Vector2>();

            Vector2 offset = new Vector2(chunk.BoundingBox.X + Chunk.TileSize / 2, chunk.BoundingBox.Y + Chunk.TileSize / 2);

            path.Add(FirstPoint);

            path.Add(current.ToVector2() * Chunk.TileSize + offset);

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current.ToVector2() * Chunk.TileSize + offset);
            }

            return path;
        }

        #endregion

        #region Private Fields

        /// <summary> Spawn and patrol location of this drone</summary>
        private Vector2 Spawn;

        /// <summary> The viewcone of this drone</summary>
        private ConeEntity ViewCone;
        /// <summary> Sprite used to draw this drone</summary>
        private AnimatedSprite Sprite;
        private AnimatedSprite AlertSignal;

        /// <summary> Current AI State</summary>
        private AIState State = AIState.Waiting;
        /// <summary> Current path the drone is taking towards the target location (if the state is targeting)</summary>
        private List<Vector2> Path;
        /// <summary> The node this drone is heading towards next on the path</summary>
        private int NextNode;

        /// <summary> Location the drone is pathfinding towards</summary>
        private Vector2 TargetLocation;
        /// <summary> Next place the drone is heading towards as part of a search or patrol</summary>
        private Vector2 WanderLocation;
        /// <summary> Timer used to control state transitions (like waiting)</summary>
        private float StateTimer = 0;
        /// <summary> The direction this drone is currently facing</summary>
        private float Direction = 0;

        #endregion

        #region Public Fields

        /// <summary> The Velocity of this Entity </summary>
        public Vector2 Velocity = new Vector2();

        #endregion

        #region Constructors

        public AerialDrone(Vector2 position, Game1 game) : base(game, new Vector2(Chunk.TileSize/3, Chunk.TileSize/6))
        {
            Spawn = position;
            Position = position;
            WanderLocation = Position;
            TargetLocation = Position;
            Sprite = new AnimatedSprite(null, game, new Vector2(32, 32));
            AlertSignal = new AnimatedSprite(null, game, new Vector2(16, 16));
            ViewCone = new ConeEntity(game, false);

            Direction = (float)game.RNG.NextDouble() * 2 * (float)Math.PI;

            ViewCone.FromDegrees(0, 50);
            ViewCone.Radius = Chunk.TileSize * 5;
            ViewCone.UpdatePosition(position);
        }

        #endregion

        #region Private Methods

        /// <summary> 
        ///     Rotates towards the target, then moves to it, for one tick.
        /// </summary>
        /// <returns> true if the drone is at the target.</returns>
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
                    if (ConeEntity.ConvertAngle(targetDirection - Direction) < Math.PI)
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
        /// <summary> Sets the wander location randomly. Has a high chance to fail; must be called repeatedly to guarantee success.</summary>
        /// <returns> true if a valid location has been found.</returns>
        private bool FindWander(Vector2 location, float distance, Chunk chunk)
        {
            float angle = (float)(Game.RNG.NextDouble() * 2 * Math.PI);

            var dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

            Vector2 p1 = Position;
            Vector2 p2 = p1 + dir;

            if (!ConeEntity.IntersectCircle(p1, p2, distance, location, float.PositiveInfinity, out float maxDist))
            {
                return false;
            }

            var point1 = new Vector2(dir.Y, -dir.X);
            point1.Normalize();
            point1 *= Size.X;
            var point2 = -point1;

            if (chunk.IntersectLine(Position + point1, dir, maxDist, out float distToIntersect1, false))
            {
                maxDist = distToIntersect1;
            }

            if (chunk.IntersectLine(Position + point2, dir, maxDist, out float distToIntersect2, false))
            {
                maxDist = distToIntersect2;
            }

            maxDist -= Size.X;

            if (maxDist < MinMovement)
            {
                return false;
            }
            else
            {
                Vector2 tentativeWanderLocation = Position + ((float)Game.RNG.NextDouble() * (maxDist - MinMovement) + MinMovement) * dir;
                if (chunk.BoundingBox.Contains(tentativeWanderLocation))
                {
                    WanderLocation = tentativeWanderLocation;
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        #endregion

        #region State Switches

        /// <summary> Sets the state to targeting and pathfinds towards the target location, then searches after it reaches it.</summary>
        public bool Target(Vector2 target, Chunk chunk)
        {
            if(State == AIState.Targeting && (TargetLocation-target).LengthSquared() < Chunk.TileSize*Chunk.TileSize*16 && Path.Count > 2)
            {
                return false;
            }

            Velocity = new Vector2();

            int targetx = (int)Math.Floor((target.X - chunk.BoundingBox.X) / Chunk.TileSize);
            int targety = (int)Math.Floor((target.Y - chunk.BoundingBox.Y) / Chunk.TileSize);

            int startx = (int)Math.Floor((Position.X - chunk.BoundingBox.X) / Chunk.TileSize);
            int starty = (int)Math.Floor((Position.Y - chunk.BoundingBox.Y) / Chunk.TileSize);

            var newPath = FindReducedPath(chunk, FindPath(chunk, startx, starty, target), Size);

            if (newPath.Count <= 1)
            {
                return false;
            }
            else {
                Path = newPath;

                NextNode = 1;

                TargetLocation = target;

                return true;
            }
        }
        /// <summary> Wanders around a target location without waiting between new wanders</summary>
        public void Search(Vector2 target, Chunk chunk, float time, bool cursory = false)
        {
            Path = null;
            StateTimer = time;
            TargetLocation = target;
            WanderLocation = target;
            FindWander(TargetLocation, SearchRange, chunk);
            State = cursory? AIState.CursorySearching : AIState.Searching;
            ViewCone.SetColor(cursory ? ConeEntity.InspectColor : ConeEntity.AlertColor);
        }
        /// <summary> Pathfinds back to the spawn</summary>
        public void Return(Chunk chunk)
        {
            if(Target(Spawn, chunk))
                State = AIState.Returning;
            ViewCone.SetColor(ConeEntity.ClearColor);
        }
        /// <summary> Waits, swiveling the camera back and forth</summary>
        public void Wait()
        {
            Path = null;
            Velocity = new Vector2(0);
            StateTimer = 0;
            State = AIState.Waiting;

            ViewCone.SetColor(ConeEntity.ClearColor);
        }

        #endregion

        #region Overrides

        public override void Respawn(Chunk chunk)
        {
            Direction = (float)Game.RNG.NextDouble() * 2 * (float)Math.PI;
            Position = Spawn;
            Wait();
        }

        public override void Update(Chunk chunk)
        {
            if(PositionKnown > 0)
                --PositionKnown;

            float dt = Game1.DeltaT;
            
            Velocity = new Vector2();
            
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
                        bool foundSearch = false;
                        for (int i = 0; i < WanderSearchAttempts; ++i)
                        {
                            if (FindWander(TargetLocation, PatrolRange, chunk))
                            {
                                foundSearch = true;
                                break;
                            }
                        }
                        if (!foundSearch)
                        {
                            Target(TargetLocation, chunk);
                        }
                    }
                    break;
                case AIState.CursorySearching:
                    if (MoveTo(WanderLocation, PatrolSpeed))
                    {
                        Velocity = new Vector2(0);
                        bool foundSearch = false;
                        for (int i = 0; i < WanderSearchAttempts; ++i)
                        {
                            if (FindWander(TargetLocation, PatrolRange, chunk))
                            {
                                foundSearch = true;
                                break;
                            }
                        }
                        if (!foundSearch)
                        {
                            Target(TargetLocation, chunk);
                        }
                    }

                    StateTimer -= dt;
                    if(StateTimer <= 0)
                    {
                        Return(chunk);
                    }
                    break;
                case AIState.Waiting:
                    StateTimer += dt;
                    if (StateTimer <= 0.05F * WaitTime)
                    {

                    }
                    if (StateTimer <= 0.25F * WaitTime)
                    {
                        Direction += dt * WaitAngularVelocity;
                    }
                    else if (StateTimer <= 0.3F * WaitTime)
                    {

                    }
                    else if (StateTimer <= 0.7F * WaitTime)
                    {
                        Direction -= dt * WaitAngularVelocity;
                    }
                    else if (StateTimer <= 0.75F * WaitTime)
                    {

                    }
                    else if (StateTimer <= 0.95F * WaitTime)
                    {
                        Direction += dt * WaitAngularVelocity;
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
                            Search(Path[NextNode - 1], chunk, float.PositiveInfinity);
                        }
                        else
                        {
                            MoveTo(Path[NextNode], TargetSpeed);
                        }
                    }
                    break;
                case AIState.Investigating:
                    if (MoveTo(Path[NextNode], SearchSpeed))
                    {
                        Velocity = new Vector2();
                        ++NextNode;
                        if (NextNode >= Path.Count)
                        {
                            Search(Path[NextNode - 1], chunk, SearchTime, true);
                        }
                        else
                        {
                            MoveTo(Path[NextNode], SearchSpeed);
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

            { // Kill if touched.
                if (!chunk.Level.Player.IsHiding
                    && GetBoundingBox().Intersects(chunk.Level.Player.GetBoundingBox()))
                {
                    chunk.Level.Player.Kill();
                }
            }
            
            if(State == AIState.Targeting || State == AIState.Searching)
                Sprite.Play("chase");
            else
                Sprite.Play("idle");

            Position += dt * Velocity;
            ViewCone.UpdatePosition(Position);
            ViewCone.Middle = Direction;
            ViewCone.Update(chunk);
            
            AlertSignal.Update(dt);
            Sprite.Update(dt);
            base.Update(chunk);
        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = Game.TextureCache["aerial-drone"];
            Sprite.Add("idle", 0, 4, 0.4);
            Sprite.Add("chase", 4, 12, 0.8, 8);
            AlertSignal.Texture = Game.TextureCache["alerts"];
            AlertSignal.Add("none", 20, 21, 1);
            AlertSignal.Add("noise", 0, 10, 1, -1, 0);
            AlertSignal.Add("alert", 10, 20, 1, -1, 0);
        }

        public override void Draw()
        {
            ViewCone.Draw();
            Sprite.Draw(Position);
            AlertSignal.Draw(Position+new Vector2(0, 16));
        }

        public void HearSound(Vector2 position, float volume, Chunk chunk)
        {
            if (PositionKnown > 0)
            {
                return;
            }

            float sqrDist = (Position - position).LengthSquared();

            if (sqrDist > SoundEngine.AudibleDistance * SoundEngine.AudibleDistance)
            {
                return;
            }

            if (chunk.IntersectLine(position, Position - position, 1, out float temp, false))
            {
                volume /= 2;
            }

            if (((volume > MinVolume) || chunk.ChunkAlarmState && (volume > AlertVolume)))
            {
                float precision = Math.Min(1, volume / BaseVolume);



                for(int i = 0; i < 10; ++i)
                {
                    float angle = (float)Game.RNG.NextDouble() * 2 * (float)Math.PI;

                    float dist = (1 - precision) * HearingPrecision;

                    Vector2 dir = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));

                    if(chunk.IntersectLine(position, dir, dist + Size.X, out float location))
                    {
                        dist = location - Size.X;
                    }
                    
                    if (Target(position + dir * dist, chunk))
                    {
                        if(State == AIState.Searching)
                        {
                            State = AIState.Targeting;
                            AlertSignal.Play("alert");
                        }
                        else if (State != AIState.Targeting && State != AIState.Investigating)
                        {
                            State = AIState.Investigating;
                            AlertSignal.Play("noise");
                            ViewCone.SetColor(ConeEntity.InspectColor);
                        }
                        break;
                    }
                }
                

            }
        }

        public void Alert(Vector2 position, Chunk chunk)
        {
            PositionKnown = 2;
            if (Target(position, chunk))
            {
                State = AIState.Targeting;
                AlertSignal.Play("alert");
                ViewCone.SetColor(ConeEntity.AlertColor);
            }

        }

        public void ClearAlarm(Chunk chunk)
        {
            Return(chunk);

        }

        #endregion

        #region Public Methods

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
                var tentativePoint = path[i];

                var dir = tentativePoint - reducedPath.Last();
                var point1 = new Vector2(dir.Y , -dir.X);
                point1.Normalize();
                point1 *= Size.X;
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
                            for (int p = NextNode - 1; p <= node; ++p)
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
            }

            reducedPath.Add( path[0]);

            return reducedPath;
        }


        #endregion
    }
}
