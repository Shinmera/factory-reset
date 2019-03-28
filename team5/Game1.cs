using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    public class Game1 : Game
    {
        public const float DeltaT = 1 / 60.0F;

        GraphicsDeviceManager DeviceManager;
        public readonly SpriteEngine SpriteEngine;
        public readonly Transforms Transforms = new Transforms();

        Level level;

        public Game1()
        {
            DeviceManager = new GraphicsDeviceManager(this);
            SpriteEngine = new SpriteEngine(this);
            Content.RootDirectory = "Content";

            IsFixedTimeStep = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteEngine.LoadContent(Content);

            level = new Level(this);
            level.LoadContent(Content);
        }
        
        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            level.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
            level.Draw(gameTime);
        }
    }
}
