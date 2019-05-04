using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
    class AlarmTrigger : BoxEntity
    {
        public bool Triggered = false;

        public AlarmTrigger(Vector2 position, Game1 game) : base(game, new Vector2(Chunk.TileSize * 0.5F, 999999))
        {
            Position = position;
        }

        public override void Respawn(Chunk chunk)
        {
            base.Respawn(chunk);
            Triggered = false;
        }
    }
}
