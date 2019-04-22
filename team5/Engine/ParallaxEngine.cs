using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    public class ParallaxEngine
    {
        private Game1 Game;
        private VertexBuffer VertexBuffer;
        private IndexBuffer IndexBuffer;
        private Effect Effect;
        public float ParallaxStrength = 2.0f/3.0f;
        public float BackgroundScale = 2.0f;

        public ParallaxEngine(Game1 game)
        {
            Game = game;
        }
        
        public void LoadContent(ContentManager content)
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[] 
            { 
                new VertexPositionTexture(new Vector3(+1, -1, 0), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(-1, +1, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(+1, +1, 0), new Vector2(1, 1))
            };
            VertexBuffer = new VertexBuffer(Game.GraphicsDevice, VertexPositionTexture.VertexDeclaration, 
                                            vertices.Length, BufferUsage.None);
            
            VertexBuffer.SetData(vertices);
            short[] indices = new short[] { 0, 1, 2, 2, 3, 0 };
            IndexBuffer = new IndexBuffer(Game.GraphicsDevice, typeof(short), 
                                          indices.Length, BufferUsage.None);
            IndexBuffer.SetData(indices);
            
            // Create shader
            Effect = content.Load<Effect>("Shaders/parallax");
        }
        
        public void UnloadContent()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
            Effect.Dispose();
        }

        public void Draw(Texture2D parallax, Vector2 position, float scale)
        {
            GraphicsDevice device = Game.GraphicsDevice;
            
            Effect.CurrentTechnique = Effect.Techniques["Tile"];
            Effect.Parameters["viewSize"].SetValue(new Vector2(device.Viewport.Width, device.Viewport.Height));
            Effect.Parameters["viewPos"].SetValue(position / ParallaxStrength);
            Effect.Parameters["viewScale"].SetValue(scale * BackgroundScale);
            Effect.Parameters["parallax"].SetValue(parallax);
            
            device.SetVertexBuffer(VertexBuffer);
            device.Indices = IndexBuffer;
            device.BlendState = BlendState.AlphaBlend;
            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }
        }
    }
}
