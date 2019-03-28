using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    public class SpriteEngine
    {
        private Game1 Game;
        private Texture2D SolidTexture;
        
        VertexBuffer VertexBuffer;
        IndexBuffer IndexBuffer;
        Effect TileEffect;
        
        public SpriteEngine(Game1 game)
        {
            Game = game;
        }
        
        public void LoadContent(ContentManager content)
        {
            SolidTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
            SolidTexture.SetData<Color>(new Color[]{Color.White});
            
            VertexPositionTexture[] vertices = new VertexPositionTexture[] 
            { 
                new VertexPositionTexture(new Vector3(+0.5f, -0.5f, 0), Vector2.One),
                new VertexPositionTexture(new Vector3(-0.5f, -0.5f, 0), Vector2.UnitY),
                new VertexPositionTexture(new Vector3(-0.5f, +0.5f, 0), Vector2.Zero),
                new VertexPositionTexture(new Vector3(+0.5f, +0.5f, 0), Vector2.UnitX)
            };
            VertexBuffer = new VertexBuffer(Game.GraphicsDevice, VertexPositionTexture.VertexDeclaration, 
                                            vertices.Length, BufferUsage.None);
            
            VertexBuffer.SetData(vertices);
            short[] indices = new short[] { 0, 1, 2, 2, 3, 0 };
            IndexBuffer = new IndexBuffer(Game.GraphicsDevice, typeof(short), 
                                          indices.Length, BufferUsage.None);
            IndexBuffer.SetData(indices);
            
            // Create shader
            TileEffect = content.Load<Effect>("Shaders/tile");
        }
        
        public void Draw(Texture2D texture, Vector4 source)
        {
            GraphicsDevice device = Game.GraphicsDevice;
            // KLUDGE: Scale to UVs here as we can't use texel fetch (hlsl Load) for some reason.
            TileEffect.CurrentTechnique = TileEffect.Techniques["Tile"];
            TileEffect.Parameters["projectionMatrix"].SetValue(Game.Transforms.ProjectionMatrix);
            TileEffect.Parameters["viewMatrix"].SetValue(Game.Transforms.ViewMatrix);
            TileEffect.Parameters["modelMatrix"].SetValue(Game.Transforms.ModelMatrix);
            TileEffect.Parameters["offset"].SetValue(source);
            TileEffect.Parameters["tileset"].SetValue(texture);
            
            device.SetVertexBuffer(VertexBuffer);
            device.Indices = IndexBuffer;
            device.BlendState = BlendState.AlphaBlend;
            foreach (EffectPass pass in TileEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }
        }
        
        public void Draw(Vector4 rect)
        {
            Draw(SolidTexture, rect);
        }
    }
}
