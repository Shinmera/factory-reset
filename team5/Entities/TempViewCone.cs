using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    class TempViewCone : ConeEntity
    {
        private int Direction;

        public TempViewCone(Game1 game) : base(game)
        {
            Radius = 69;
        }

        // FIXME: Bad naming, also should use existing angles to do a conversion
        //        so that the constants need only be set once.
        public void faceLeft()
        {
            if (Direction != -1)
            {
                Angle1 = 5.0F / 6 * (float)Math.PI;
                Angle2 = 7.0F / 6 * (float)Math.PI;
            }
        }

        public void faceRight()
        {
            if (Direction != 1)
            {
                Angle1 = 11.0F / 6 * (float)Math.PI;
                Angle2 = 1.0F / 6 * (float)Math.PI;
            }
        }

        public override void Update(GameTime gameTime, Chunk chunk)
        {
            base.Update(gameTime, chunk);

            if (!chunk.Level.Player.IsHiding)
            {
                if (Collide(chunk.Level.Player, Game1.DeltaT, out int dirction, out float time, out bool corner))
                {
                    chunk.Die(chunk.Level.Player);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Game.ViewConeEngine.Draw(Position, Radius, Angle1, Angle2);
        }
    }
}
