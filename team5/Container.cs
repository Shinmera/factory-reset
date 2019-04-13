using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace team5
{
    abstract class Container
    {
        protected Game1 Game;
        protected Window Parent;

        public Container(Game1 game, Window parent)
        {
            Game = game;
            Parent = parent;
        }

        public virtual void LoadContent(ContentManager content)
        {
        }
        public abstract void Resize(int width, int height);
        public abstract void LoadContent();
        public abstract void Update();
        public abstract void Draw();

    }
}
