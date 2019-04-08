using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace team5
{
    public class Game1 : Game
    {
        public const float DeltaT = 1 / 60.0F;

        GraphicsDeviceManager DeviceManager;
        public readonly SpriteEngine SpriteEngine;
        public readonly TilemapEngine TilemapEngine;
        public readonly ViewConeEngine ViewConeEngine;
        public readonly SoundEngine SoundEngine;
        public readonly Transforms Transforms = new Transforms();

        Level Level;

        public Game1()
        {
            DeviceManager = new GraphicsDeviceManager(this);
            SpriteEngine = new SpriteEngine(this);
            TilemapEngine = new TilemapEngine(this);
            ViewConeEngine = new ViewConeEngine(this);
            SoundEngine = new SoundEngine(this);
            DeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";

            IsFixedTimeStep = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Window.ClientSizeChanged += (x, y) => { Resize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height); };
            Resize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            
            RasterizerState rs = new RasterizerState{ CullMode = CullMode.None };
            GraphicsDevice.RasterizerState = rs;
            DepthStencilState ds = new DepthStencilState{ DepthBufferEnable = false };
            GraphicsDevice.DepthStencilState = ds;
        }

        protected override void LoadContent()
        {
            SpriteEngine.LoadContent(Content);
            TilemapEngine.LoadContent(Content);
            ViewConeEngine.LoadContent(Content);

            Level = new Level(this, "test");
            Level.LoadContent(Content);
            // Load later as Entities can register effects to load.
            SoundEngine.LoadContent(Content);
        }
        
        protected override void UnloadContent()
        {
            // FIXME: Should probably do this
        }

        protected void Resize(int width, int height)
        {
            Transforms.ProjectionMatrix = Matrix.CreateOrthographicOffCenter(0, width, 0, height, -10, 10);
            System.Diagnostics.Debug.WriteLine("Viewport: " + width + "x" + height);
            Level.Resize(width, height);
        }
        
        protected override void Update(GameTime gameTime)
        {
            Transforms.Reset();
            Transforms.ResetView();
            base.Update(gameTime);
            Level.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
            Level.Draw(gameTime);
        }
    }
}
