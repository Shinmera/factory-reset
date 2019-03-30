using System;
using Microsoft.Xna.Framework;

namespace team5
{
    class Camera
    {
        private Game1 Game;
        private Player Player;
        private Vector2 Position = new Vector2(0,0);
        private Vector2 Velocity = new Vector2(0,0);
        private float Zoom = 1.0f;
        private float ViewScale = 1.0f;
        // Target view half-size
        private Vector2 TargetSize = new Vector2(40, 26)*Chunk.TileSize/2.0f;
        
        public Camera(Player player, Game1 game)
        {
            Player = player;
            Game = game;
        }
        
        public void Resize(int width, int height)
        {
            ViewScale = Zoom * width / (TargetSize.X*2);
            TargetSize.Y = (height / ViewScale) / 2;
        }
        
        public void Update(GameTime gameTime)
        {
            Vector2 intendedPosition = Player.Position;
            Vector2 direction = intendedPosition - Position;
            float length = (float)Math.Max(1.0, direction.Length());
            float ease = (float)Math.Max(0.0, Math.Min(20.0, 0.2+(Math.Pow(length, 1.5)/100)));
            Position += direction*ease/length;
            
            Vector2 camera = (TargetSize / Zoom)-Position;
            Game.Transforms.ScaleView(ViewScale);
            Game.Transforms.TranslateView(camera);
        }
    }
}
