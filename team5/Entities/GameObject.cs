using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
    abstract class GameObject
    {
        protected Game1 Game;

        public GameObject(Game1 game)
        {
            this.Game = game;
        }

        public virtual void Update(GameTime gameTime, Chunk level)
        {

        }

        public virtual void Draw(GameTime gameTime, Vector2 CameraOffset)
        {

        }
    }
}
