using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace team5
{
    class TilePassThroughPlatform : TileSolid
    {
        const int PassThroughs = 0b00001011;

        public TilePassThroughPlatform(Game1 game) : base(game)
        {
        }

        public override bool Collide(Entity source, RectangleF tileBB, float timestep, out int direction, out float time, out bool corner)
        {
            if (BoxEntity.CollideMovable((Movable)source, tileBB, timestep, out direction, out time, out corner))
            {
                direction = direction & ~PassThroughs;

                if (source is Player)
                {
                    if (((Player)source).FallThrough)
                    {
                        direction = direction & ~Chunk.Down;
                    }
                }

                return direction == 0 ? false : true;
            }

            return false;
            
        }
    }
}
