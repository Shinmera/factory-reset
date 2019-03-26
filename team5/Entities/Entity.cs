using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
    abstract class Entity
    {
        protected Game1 Game;
        protected AnimatedSprite Drawer;

        public Vector2 Position = new Vector2();

        public abstract RectangleF GetBoundingBox();

        public Entity(Game1 game)
        {
            this.Game = game;
        }

        public virtual void Update(GameTime gameTime, Chunk level)
        {

        }

        public virtual void Draw(GameTime gameTime, Vector2 CameraOffset)
        {
            if(Drawer != null)
            Drawer.Draw(Position + CameraOffset);
        }

        //Assume source is always a box
        public abstract bool Collide(Entity source, float timestep, out int direction, out float time);
    }
}
