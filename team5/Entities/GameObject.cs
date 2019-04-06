using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace team5
{
    abstract class GameObject
    {
        protected Game1 Game;

        public GameObject(Game1 game)
        {
            this.Game = game;
        }

        /// <summary>
        ///   Load any content for this object.
        /// </summary>
        public virtual void LoadContent(ContentManager content)
        {
            
        }

        /// <summary>
        ///   Updates this object's physics/mechanics, with a reference to the chunk it's in.
        /// </summary>
        public virtual void Update(GameTime gameTime, Chunk chunk)
        {

        }

        /// <summary>
        ///   Draws this object.
        /// </summary>
        public virtual void Draw(GameTime gameTime)
        {

        }
    }
}
