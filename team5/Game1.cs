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
        public readonly Transforms Transforms = new Transforms();

        Level level;

        public Game1()
        {
            DeviceManager = new GraphicsDeviceManager(this);
            SpriteEngine = new SpriteEngine(this);
            TilemapEngine = new TilemapEngine(this);
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
        }

        protected override void LoadContent()
        {
            SpriteEngine.LoadContent(Content);
            TilemapEngine.LoadContent(Content);

            level = new Level(this);
            level.LoadContent(Content);
        }
        
        protected override void UnloadContent()
        {
        }

        protected void Resize(int width, int height)
        {
            Transforms.ProjectionMatrix = Matrix.CreateOrthographicOffCenter(0, width, 0, height, -10, 10);
            System.Diagnostics.Debug.WriteLine("Viewport: " + width + "x" + height);
        }

        // KLUDGE: Fix for odd monogame behaviour that causes B to suspend the app.
        // See: http://community.monogame.net/t/xbox-one-back-vs-b-button-possible-bug/8862/10
        // See: https://github.com/MonoGame/MonoGame/issues/6404
        int ButtonB_KeyUpTime = 0;
        GamePadState previousState, currentState;
        public bool BackButtonDownEvent()
        {
            if (ButtonB_KeyUpTime > 0)
                return false;
            return currentState.IsButtonDown(Buttons.Back) && previousState.IsButtonUp(Buttons.Back);
        }
        
        protected override void Update(GameTime gameTime)
        {
            previousState = currentState;
            currentState = GamePad.GetState(0);
            if (currentState.IsButtonUp(Buttons.B) && previousState.IsButtonDown(Buttons.B))
                ButtonB_KeyUpTime = 3;
            else
                ButtonB_KeyUpTime -= 1;
            
            Transforms.Reset();
            Transforms.ResetView();
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
