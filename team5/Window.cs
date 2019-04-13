using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;


namespace team5
{
    public abstract class Window
    {

        public virtual void LoadContent(ContentManager content) {
        }
        public abstract void Resize(int width, int height);
        public abstract void Update();
        public abstract void Draw();

        public abstract void OnQuitButon();
    }
}
