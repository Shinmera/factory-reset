using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace team5
{
    public class Game1 : Game
    {
        public const float DeltaT = 1 / 60.0F;
        public const string FirstLevel = "lobby";

        GraphicsDeviceManager DeviceManager;
        public readonly SpriteEngine SpriteEngine;
        public readonly TilemapEngine TilemapEngine;
        public readonly TriangleEngine TriangleEngine;
        public readonly SoundEngine SoundEngine;
        public readonly TextEngine TextEngine;
        public readonly ParallaxEngine ParallaxEngine;
        public readonly ParticleEmitter ParticleEmitter;
        public readonly Transforms Transforms = new Transforms();
        public readonly Random RNG;
        public readonly Controller Controller;

        public Window ActiveWindow { get; private set; }
        Level Level;

        public Game1()
        {
            RNG = new Random();
            DeviceManager = new GraphicsDeviceManager(this);
            SpriteEngine = new SpriteEngine(this);
            TilemapEngine = new TilemapEngine(this);
            TriangleEngine = new TriangleEngine(this);
            SoundEngine = new SoundEngine(this);
            TextEngine = new TextEngine(this);
            ParallaxEngine = new ParallaxEngine(this);
            ParticleEmitter = new ParticleEmitter(this);
            Controller = new Controller();
            DeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";

            IsFixedTimeStep = true;

            ActiveWindow = new LoadScreen(this);
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
            ParticleEmitter.LoadContent(Content);
            SpriteEngine.LoadContent(Content);
            TilemapEngine.LoadContent(Content);
            TriangleEngine.LoadContent(Content);
            TextEngine.LoadContent(Content);
            ParallaxEngine.LoadContent(Content);
            SoundEngine.LoadContent(Content);
            ActiveWindow.LoadContent(Content);
        }
        
        protected override void UnloadContent()
        {
            // FIXME: Should probably do this
        }

        public void LoadLevel(object identifier)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("Loading level from {0}...", identifier));
            Level = new Level(this, identifier);
            Level.LoadContent(Content);
            SoundEngine.LoadContent(Content);

            ActiveWindow = Level;
            Resize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        public void UnloadLevel()
        {
            //Level.UnloadContent();
            //SoundEngine.UnloadContent(Content);
            ActiveWindow = new LoadScreen(this);
        }

        public bool Paused {
            get
            {
                return (ActiveWindow != Level || Level.Paused);
            }
            set
            {
                if (value == Paused) return;
                Level.Paused = value;
                // Oh jeeze.
                Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    ()=>UI.Root.Current.Game.Paused = value);
            }
        }
        
        public void ShowScore()
        {
            Level.Paused = true;
            var score = Level.Score();
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    ()=>UI.Root.Current.Game.ShowScore(score));
        }

        protected void Resize(int width, int height)
        {
            Transforms.ProjectionMatrix = Matrix.CreateOrthographicOffCenter(0, width, 0, height, -10, 10);
            System.Diagnostics.Debug.WriteLine("Viewport: " + width + "x" + height);
            ActiveWindow.Resize(width, height);
            TextEngine.Resize(width, height);
        }
        
        protected override void Update(GameTime gameTime)
        {
            Controller.Update();
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
            TextEngine.DrawText();
        }
    }
}
