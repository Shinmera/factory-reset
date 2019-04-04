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
        private Chunk ChunkInFocus;
        private RectangleF ChunkClamps;
        private float Zoom = 1.0f;
        private float ViewScale = 1.0f;
        // Target view half-size
        private Vector2 TargetSize = new Vector2(40, 26)*Chunk.TileSize/2.0f;

        public bool IsInClamp { get; private set; } = true;

        public Camera(Player player, Game1 game)
        {
            Player = player;
            Game = game;
        }

        private void UpdateClampData()
        {
            float lx = ChunkInFocus.BoundingBox.X;
            float ly = ChunkInFocus.BoundingBox.Y;
            float lw = ChunkInFocus.Size.X;
            float lh = ChunkInFocus.Size.Y;
            float cw = TargetSize.X;
            float ch = TargetSize.Y;
            ChunkClamps = new RectangleF(lx + cw, ly + ch, -2 * cw + lw * 2, -2 * ch + lh * 2);
            IsInClamp = false;
        }

        public void Resize(int width, int height)
        {
            ViewScale = Zoom * width / (TargetSize.X*2);
            TargetSize.Y = (height / ViewScale) / 2;
            UpdateClampData();
        }   

        public void UpdateChunk(Chunk chunk)
        {
            ChunkInFocus = chunk;
            UpdateClampData();
        }

        public void Update(GameTime gameTime)
        {
            Vector2 intendedPosition = Player.Position;
            
            // Lock intended position within chunk limits. 
            // KLUDGE: Note that chunks are currently positioned by center of lower leftmost tile, 
            //         which is inconsistent.
            {
                
                Func<float, float, float, float> clamp = (l, x, u) => (x < l)? l : (u < x)? u : x;
                /*
                float lx = chunk.BoundingBox.X;
                float ly = chunk.BoundingBox.Y;
                float lw = chunk.Size.X;
                float lh = chunk.Size.Y;
                float cw = TargetSize.X;
                float ch = TargetSize.Y;
                
                intendedPosition.X = clamp(lx + cw, intendedPosition.X, lx - cw + lw*2);
                intendedPosition.Y = clamp(ly + ch, intendedPosition.Y, ly - ch + lh*2);
                */

                intendedPosition.X = clamp(ChunkClamps.Left, intendedPosition.X, ChunkClamps.Right);
                intendedPosition.Y = clamp(ChunkClamps.Bottom, intendedPosition.Y, ChunkClamps.Top);
            }

            // Ease towards intended position
            Vector2 direction = intendedPosition - Position;
            float length = (float)Math.Max(1.0, direction.Length());
            float ease = (float)Math.Max(0.0, Math.Min(20.0, 0.2+(Math.Pow(length, 1.5)/100)));
            Position += direction*ease/length;

            if (!IsInClamp)
            {
                if(Position.X > ChunkClamps.Left && Position.X < ChunkClamps.Right && Position.Y > ChunkClamps.Bottom && Position.Y < ChunkClamps.Top)
                {
                    IsInClamp = true;
                }
            }

            // Update view transform
            Vector2 camera = (TargetSize / Zoom)-Position;
            Game.Transforms.TranslateView(camera);
            Game.Transforms.ScaleView(ViewScale);
        }
    }
}
