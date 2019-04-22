using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace team5
{
    abstract class Container
    {

        protected Game1 Game;
        protected Window Parent;

        public Vector2 Position;

        public Container(Game1 game, Window parent)
        {
            Game = game;
            Parent = parent;
        }

        public virtual void LoadContent(ContentManager content)
        {

        }

        public abstract void Update();
        public abstract void Draw();

    }
}
