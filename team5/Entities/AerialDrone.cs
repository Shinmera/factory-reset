using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5.Entities
{
    class AerialDrone : BoxEntity
    {
        public AerialDrone(Game1 game, Vector2 size) : base(game, size)
        {
        }

        static List<Point> FindPath(uint[] tiles, uint width, uint height, int startx, int starty, int targetx, int targety)
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
                        if ((xoffset == 0 && yoffset == 0) || neighbor.X < 0 || neighbor.X >= width || neighbor.Y < 0 || neighbor.Y >= height
                            || tiles[(height - neighbor.Y - 1) * width + neighbor.X] == (uint)Chunk.Colors.SolidPlatform )
                        {
                            continue;
                        }

                        if(Math.Abs(xoffset) + Math.Abs(yoffset) == 2)
                        {
                            if(tiles[(height - current.Y + yoffset - 1) * width + current.X] == (uint)Chunk.Colors.SolidPlatform
                                || tiles[(height - current.Y - 1) * width + current.X + xoffset] == (uint)Chunk.Colors.SolidPlatform)
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
                            openSet.Add(neighbor);
                        } else {
                            bool found = gScore.TryGetValue(neighbor, out float neighborScore);
                            if (found && tentative_gScore >= neighborScore)
                            {
                                continue;
                            }
                        }

                        if (cameFrom.ContainsKey(neighbor)) {
                            cameFrom[neighbor] = current;
                            gScore[neighbor] = tentative_gScore;
                            fScore[neighbor] = gScore[neighbor] + GetDist(neighbor.X, neighbor.Y, targetx, targety);
                        }
                        else
                        {
                            cameFrom.Add(neighbor, current);
                            gScore.Add(neighbor, tentative_gScore);
                            fScore.Add(neighbor, gScore[neighbor] + GetDist(neighbor.X, neighbor.Y, targetx, targety));
                        }
                    }
                }
            }

            return null;
        }

        private static List<Point> ReconstructPath(Dictionary<Point, Point> cameFrom, Point current)
        {
            var path = new List<Point>
            {
                current
            };

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
