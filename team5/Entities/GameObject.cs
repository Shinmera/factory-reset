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
        
        public virtual void LoadContent(ContentManager content)
        {
            
        }

        public virtual void Update(GameTime gameTime, Chunk chunk)
        {

        }

        public virtual void Draw(GameTime gameTime)
        {

        }
    }
}
