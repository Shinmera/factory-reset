using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace team5
{
    class LoadScreen : Window
    {
        public LoadScreen(Game1 game):base(game) { }

        public override void LoadContent(ContentManager content) {
            // FIXME
        }
        
        public override void Resize(int width, int height){
        }
        
        public override void Update(){
            // FIXME
        }
        
        public override void Draw(){
            Game.GraphicsDevice.Clear(Color.Black);
        }
    }
}
