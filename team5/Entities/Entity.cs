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
        protected AnimatedSprite Drawer;

        public Vector2 Position = new Vector2();

        public abstract RectangleF GetBoundingBox();

        public Entity(Game1 game):base(game)
        {
        }

        public override void Draw(GameTime gameTime, Vector2 CameraOffset)
        {
            if(Drawer != null)
            Drawer.Draw(Position + CameraOffset);
        }

        public abstract bool Collide(Entity source, float timestep, out int direction, out float time);
        public abstract bool Contains(Vector2 point);
    }
}
