using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace team5
{
    class TilePlatform : TileSolid
    {
        
        public TilePlatform(Game1 game) : base(game)
        {
        }

        public override bool Collide(Entity source, RectangleF tileBB, float timestep, out int direction, out float time, out bool corner)
        {
            return BoxEntity.CollideMovable((Movable)source, tileBB, timestep, out direction, out time, out corner);
        }
    }
}
