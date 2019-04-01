using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace team5
{
    abstract class TileSolid : TileType
    {
        public TileSolid(Game1 game) : base(game)
        {
        }

        public abstract bool Collide(Entity source, RectangleF tileBB, float timestep, out int direction, out float time, out bool corner);
    }
}
