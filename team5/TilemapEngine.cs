using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    public class TilemapEngine
    {
        private Game1 Game;
        private VertexBuffer VertexBuffer;
        private IndexBuffer IndexBuffer;
        private Effect TileEffect;
        
        public TilemapEngine(Game1 game)
        {
            Game = game;
        }
        
        public void LoadContent(ContentManager content)
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[] 
            { 
                new VertexPositionTexture(new Vector3(+1, -1, 0), Vector2.One),
                new VertexPositionTexture(new Vector3(-1, -1, 0), Vector2.UnitY),
                new VertexPositionTexture(new Vector3(-1, +1, 0), Vector2.Zero),
                new VertexPositionTexture(new Vector3(+1, +1, 0), Vector2.UnitX)
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
        
        /// <summary>
        ///   Render the given tilemap using the tileset atlas.xs
        /// </summary>
        /// <param name="tilemap">The map texture, describing which tiles to render where.</param>
        /// <param name="tileset">The set texture, describing individual tiles in an atlas.</param>
        public void Draw(Texture2D tilemap, Texture2D tileset)
        {
            GraphicsDevice device = Game.GraphicsDevice;
            
            TileEffect.CurrentTechnique = TileEffect.Techniques["Tile"];
            TileEffect.Parameters["viewSize"].SetValue(new Vector2(device.Viewport.Width, device.Viewport.Height));
            TileEffect.Parameters["viewMatrix"].SetValue(Game.Transforms.ViewMatrix);
            TileEffect.Parameters["modelMatrix"].SetValue(Game.Transforms.ModelMatrix);
            TileEffect.Parameters["tileSize"].SetValue(Chunk.TileSize);
            TileEffect.Parameters["tileset"].SetValue(tileset);
            TileEffect.Parameters["tilemap"].SetValue(tilemap);
            
            device.SetVertexBuffer(VertexBuffer);
            device.Indices = IndexBuffer;
            device.BlendState = BlendState.AlphaBlend;
            foreach (EffectPass pass in TileEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }
        }
    }
}
