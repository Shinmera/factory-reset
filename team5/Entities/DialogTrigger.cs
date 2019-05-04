using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
    class DialogTrigger : BoxEntity
    {
        public DialogTrigger(Vector2 position, Game1 game) : base(game, new Vector2(Chunk.TileSize * 0.5F, 999999))
        {
            Position = position;
        }
    }
}
