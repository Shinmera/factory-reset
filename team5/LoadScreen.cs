using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace team5
{
    class LoadScreen : Window
    {
        private readonly AnimatedSprite Sprite;
        private Vector2 TargetSize = new Vector2(Camera.TargetWidth, 26*Chunk.TileSize/2.0f);
        private float ViewScale = 1.0f;

        public volatile string LevelName = "";

        public LoadScreen(Game1 game):base(game)
        {
            Sprite = new AnimatedSprite(null, game, new Vector2(32, 40));
        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = Game.TextureCache["player"];
            Sprite.Add("run",    6, 22, 0.8);
        }
        
        public override void Resize(int width, int height)
        {
            ViewScale = width / (TargetSize.X*2);
            TargetSize.Y = (height / ViewScale) / 2;
        }
        
        public override void Update()
        {
            Game.Transforms.ScaleView(ViewScale);
            Sprite.Update(Game1.DeltaT);
        }
        
        public override void Draw()
        {
            Game.GraphicsDevice.Clear(Color.Black);
            Vector2 position = new Vector2(TargetSize.X*2-16-Sprite.FrameSize.X,Sprite.FrameSize.Y);
            Game.TextEngine.QueueText(LevelName, TargetSize,
                                      24, Color.White, TextEngine.Orientation.Center, TextEngine.Orientation.Center);
            

            Game.TextEngine.QueueText("Loading...", position+new Vector2(-Sprite.FrameSize.X-8, 0),
                                      16, Color.White, TextEngine.Orientation.Right, TextEngine.Orientation.Center);
            Sprite.Draw(position);
        }
    }
}
