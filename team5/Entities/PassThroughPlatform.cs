using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
    class PassThroughPlatform : Platform
    {
        private int PassThroughs;

        public PassThroughPlatform(int passThroughs, Vector2 position, Game1 game, int width, int height) : base(position, game, width, height)
        {
            PassThroughs = passThroughs;
        }

        public override bool Collide(Entity source, float timestep, out int direction, out float time)
        {
            if(base.Collide(source, timestep, out direction, out time))
            {
                direction = direction & ~PassThroughs;

                if(source is Player)
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
