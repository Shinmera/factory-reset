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

        public new readonly ContentManager Content;
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

        private Window RealActiveWindow;
        public Window ActiveWindow { 
            get{
                return RealActiveWindow;
            }
            private set{
                if(value != null)
                    value.Resize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                RealActiveWindow = value;
            }
        }
        
        private Level Level {
            get{ return (ActiveWindow is Level)? (Level)ActiveWindow : null; }
        }
        
        public object NextLevelId =>
            (ActiveWindow is Level)
            ? Level.Next
            : null;

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
            Content = new ContentManager(Services);
            Content.RootDirectory = "Content";

            IsFixedTimeStep = true;
            RealActiveWindow = new LoadScreen(this);
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
            Game1.Log("Game", "Loading level from {0}...", identifier);
            // FIXME: Handle errors during load.
            
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Level level = new Level(this, identifier);
            level.LoadContent(Content);
            Game1.Log("Game", "Level loaded in {0}s", sw.ElapsedMilliseconds/100.0f);
            // Pad out load time so that the load screen doesn't just "pop in" for a fraction of a second.
            while(sw.ElapsedMilliseconds < 1000){
                RunOneFrame();
                System.Threading.Thread.Sleep(1);
            }
            sw.Stop();

            ActiveWindow = level;
        }

        public void UnloadLevel()
        {
            Game1.Log("Game", "Unloading level...");
            ActiveWindow.UnloadContent();
            SoundEngine.UnloadContent();
            TextureCache.UnloadContent();
            // Reload engine that got purged in texture unload
            ParticleEmitter.LoadContent(Content);
            var Loader = new LoadScreen(this);
            Loader.LoadContent(Content);
            ActiveWindow = Loader;
        }
        
        public void ReloadLevel()
        {
            if(!(ActiveWindow is Level)) return;
            SoundEngine.Clear();
            LoadLevel(Level.Identifier);
        }
        
        public void AdvanceLoad()
        {
            if(ActiveWindow is LoadScreen){
                RunOneFrame();
            }
        }
        
        public static void Log(string source, string format, params object[] args)
        {
            if(!System.Diagnostics.Debugger.IsAttached) return;
            string content = String.Format(format, args);
            System.Diagnostics.Debug.WriteLine(String.Format("[{0,-16}] {1}", source, content));
        }

        public bool Paused {
            get
            {
                return (!(ActiveWindow is Level) || Level.Paused);
            }
            set
            {
                if (value == Paused || Level == null) return;
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

        public void Resize(int width, int height)
        {
            Transforms.ProjectionMatrix = Matrix.CreateOrthographicOffCenter(0, width, 0, height, -10, 10);
            Game1.Log("Game", "Viewport: " + width + "x" + height);
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
            GraphicsDevice.Clear(Color.White);
            ActiveWindow.Draw();
            TextEngine.DrawText();
        }
    }
}
