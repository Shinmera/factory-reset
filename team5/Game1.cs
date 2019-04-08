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
        public readonly TextEngine TextEngine;
        public readonly Transforms Transforms = new Transforms();

        Window ActiveWindow;
        MainMenu MainMenu;
        Level Level;

        public Game1()
        {
            DeviceManager = new GraphicsDeviceManager(this);
            SpriteEngine = new SpriteEngine(this);
            TilemapEngine = new TilemapEngine(this);
            ViewConeEngine = new ViewConeEngine(this);
            SoundEngine = new SoundEngine(this);
            TextEngine = new TextEngine(this);
            DeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";

            IsFixedTimeStep = true;

            MainMenu = new MainMenu(this);
            ActiveWindow = MainMenu;
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
            TextEngine.LoadContent(Content);
            SoundEngine.LoadContent(Content);
            
            /*
            Level = new Level(this, "test");
            Level.LoadContent(Content);
            */
        }
        
        protected override void UnloadContent()
        {
            // FIXME: Should probably do this
        }

        public void StartLevel()
        {
            Level = new Level(this, "test");
            Level.LoadContent(Content);
            SoundEngine.LoadContent(Content);

            ActiveWindow = Level;
            Resize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        protected void Resize(int width, int height)
        {
            Transforms.ProjectionMatrix = Matrix.CreateOrthographicOffCenter(0, width, 0, height, -10, 10);
            System.Diagnostics.Debug.WriteLine("Viewport: " + width + "x" + height);
            ActiveWindow.Resize(width, height);
        }
        
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Transforms.Reset();
            Transforms.ResetView();
            ActiveWindow.Update();
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            ActiveWindow.Draw();
        }
    }
}
