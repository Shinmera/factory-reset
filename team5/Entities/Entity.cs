using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
    abstract class Entity:GameObject
    {
        public Vector2 Position = new Vector2();

        public Entity(Game1 game):base(game)
        {
        }

        public abstract RectangleF GetBoundingBox();

        public abstract bool Collide(Entity source, float timestep, out int direction, out float time, out bool corner);
        public abstract bool Contains(Vector2 point);
    }
}
