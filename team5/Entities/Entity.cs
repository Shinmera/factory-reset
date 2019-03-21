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
        protected Game1 game;
        protected AnimatedSprite drawer;

        public Vector2 velocity;
        public Vector2 position;

        public abstract Rectangle getBoundingBox();

        public Entity(Game1 game)
        {
            this.game = game;
        }

        public virtual void Update(GameTime gameTime, Chunk level)
        {

        }

        public virtual void Draw(GameTime gameTime, Vector2 CameraOffset)
        {
            if(drawer != null)
            drawer.Draw(position + CameraOffset);
        }

        //Assume source is always a box
        public abstract bool collide(Entity source, float timestep, out int direction, out float time);
    }
}
