using System;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using team5.UI;

namespace team5
{
    public class Game1 : Game
    {
        public const float DeltaT = 1 / 60.0F;
        public const string FirstLevel = "lobby";

        public readonly GraphicsDeviceManager DeviceManager;
        public readonly TextureCache TextureCache;
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
        private readonly ConcurrentQueue<Action<Game1>> ActionQueue = new ConcurrentQueue<Action<Game1>>();

        public Window ActiveWindow { get; private set; }
        Level Level;

        public Game1()
        {
            RNG = new Random();
            DeviceManager = new GraphicsDeviceManager(this);
            TextureCache = new TextureCache(this);
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
            TextureCache.UnloadContent();
            ParticleEmitter.UnloadContent();
            SpriteEngine.UnloadContent();
            TilemapEngine.UnloadContent();
            TriangleEngine.UnloadContent();
            TextEngine.UnloadContent();
            ParallaxEngine.UnloadContent();
            SoundEngine.UnloadContent();
            ActiveWindow.UnloadContent();
        }

        public void LoadLevel(object identifier)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            System.Diagnostics.Debug.WriteLine(String.Format("Loading level from {0}...", identifier));
            Level = new Level(this, identifier);
            Level.LoadContent(Content);
            System.Diagnostics.Debug.WriteLine("Level loaded in "+(sw.ElapsedMilliseconds/100.0f)+"s");
            // Pad out load time so that the load screen doesn't just "pop in" for a fraction of a second.
            while(sw.ElapsedMilliseconds < 1000){
                RunOneFrame();
                System.Threading.Thread.Sleep(1);
            }
            sw.Stop();

            ActiveWindow = Level;
            Resize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        public void UnloadLevel()
        {
            Level.UnloadContent();
            SoundEngine.UnloadContent();
            TextureCache.UnloadContent();
            var Loader = new LoadScreen(this);
            Loader.LoadContent(Content);
            Level = null;
            ActiveWindow = Loader;
        }
        
        public void ReloadLevel()
        {
            SoundEngine.Clear();
            LoadLevel(Level.Identifier);
        }
        
        public object NextLevel =>
            (Level == null || Level.Next == null)
            ? null
            : Level.Next;
        
        public void AdvanceLoad()
        {
            if(ActiveWindow is LoadScreen){
                RunOneFrame();
            }
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
                Root.Current.QueueAction((root) => root.Game.Paused = value);
            }
        }
        
        public void ShowScore()
        {
            Level.Paused = true;
            var score = Level.Score();
            Root.Current.QueueAction((root)=>root.Game.ShowScore(score));
        }

        protected void Resize(int width, int height)
        {
            Transforms.ProjectionMatrix = Matrix.CreateOrthographicOffCenter(0, width, 0, height, -10, 10);
            System.Diagnostics.Debug.WriteLine("Viewport: " + width + "x" + height);
            ActiveWindow.Resize(width, height);
            TextEngine.Resize(width, height);
        }
        
        public void QueueAction(Action<Game1> action)
        {
            ActionQueue.Enqueue(action);
        }
        
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if(ActionQueue.TryDequeue(out var action))
                action(this);
            Controller.Update();
            Transforms.Reset();
            Transforms.ResetView();
            ActiveWindow.Update();
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            GraphicsDevice.Clear(Color.Black);
            ActiveWindow.Draw();
            TextEngine.DrawText();
        }
    }
}
