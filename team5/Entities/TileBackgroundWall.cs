using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace team5
{
    class TileBackgroundWall : TileType
    {
        public TileBackgroundWall(Game1 game) : base(game)
        {
        }

        public override bool Collide(Entity source, RectangleF tileBB, float timestep, out int direction, out float time, out bool corner)
        {
            if (!(source is Enemy))
            {
                direction = 0;
                time = -1;
                corner = false;
                return false;
            }

            return BoxEntity.CollideMovable((Movable)source, tileBB, timestep, out direction, out time, out corner);
        }
    }
}
