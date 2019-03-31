using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace team5.Entities
{
    class TileHidingSpot : TileType
    {
        public TileHidingSpot(Game1 game) : base(game)
        {
        }

        public override bool Collide(Entity source, RectangleF tileBB, float timestep, out int direction, out float time, out bool corner)
        {
            direction = 0;
            time = -1;
            corner = false;
            return false;
        }
    }
}
