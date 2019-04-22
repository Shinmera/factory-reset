using System.IO;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace team5
{
    public class TilemapEngine
    {
        private Game1 Game;
        private ContentManager Content;
        private VertexBuffer VertexBuffer;
        private IndexBuffer IndexBuffer;
        private Effect TileEffect;
        private readonly Dictionary<string, Texture2D> SetCache = new Dictionary<string, Texture2D>();
        
        public TilemapEngine(Game1 game)
        {
            Game = game;
        }
        
        public void LoadContent(ContentManager content)
        {
            Content = content;
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
            TileEffect = content.Load<Effect>("Shaders/tile");
            
            SetCache.Add("solid", CreateChunkTileset());
        }
        
        public void UnloadContent()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
            TileEffect.Dispose();
            // FIXME: remove SetCache in favour of generic texture cache.
        }
        
        public Texture2D Tileset(string tileset)
        {
            if(!SetCache.ContainsKey(tileset))
                SetCache.Add(tileset, Content.Load<Texture2D>("Textures/"+tileset));
            return SetCache[tileset];
        }
        
        public Texture2D CreateChunkTileset()
        {
            Texture2D tex = new Texture2D(Game.GraphicsDevice, 256*Chunk.TileSize, 256*Chunk.TileSize);
            Color[] data = new Color[tex.Width*tex.Height];
            // Clear to empty
            for(int i=0; i<data.Length; ++i)
            {
                data[i] = new Color(0);
            }
            // Set colors at own positions.
            uint[] colors = (uint[])System.Enum.GetValues(typeof(Chunk.Colors));
            foreach(uint c in colors)
            {
                uint r = (c & 0x000000FF) >> 0;
                uint g = (c & 0x0000FF00) >> 8;
                for(uint x=r*Chunk.TileSize; x<(r+1)*Chunk.TileSize; ++x)
                {
                    for(uint y=g*Chunk.TileSize; y<(g+1)*Chunk.TileSize; ++y)
                    {
                        if (c != (uint)Chunk.Colors.Pickup)
                        {
                            data[x + y * tex.Width] = new Color(c);
                        }
                    }
                }
            }
        
            tex.SetData(data);
            return tex;
        }

        private async void SaveTexture(Texture2D tex)
        {
            Windows.Storage.Pickers.FileSavePicker picker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeChoices.Add("Portable Network Graphics", new[] { ".png" });
            picker.SuggestedFileName = "texture";
            Windows.Storage.StorageFile file = await picker.PickSaveFileAsync();

            if (file != null)
            {
                var stream = await file.OpenStreamForWriteAsync();
                tex.SaveAsPng(stream, tex.Width, tex.Height);
            }
        }
        
        /// <summary>
        ///   Render the given tilemap using the tileset atlas.xs
        /// </summary>
        /// <param name="tilemap">The map texture, describing which tiles to render where.</param>
        /// <param name="tileset">The set texture, describing individual tiles in an atlas.</param>
        public void Draw(Texture2D tilemap, Texture2D tileset, Vector2 pos)
        {
            Game.Transforms.Push();
            Game.Transforms.Translate(pos);
            GraphicsDevice device = Game.GraphicsDevice;
            
            TileEffect.CurrentTechnique = TileEffect.Techniques["Tile"];
            TileEffect.Parameters["viewSize"].SetValue(new Vector2(device.Viewport.Width, device.Viewport.Height));
            TileEffect.Parameters["viewMatrix"].SetValue(Matrix.Invert(Game.Transforms.ViewMatrix));
            TileEffect.Parameters["modelMatrix"].SetValue(Matrix.Invert(Game.Transforms.ModelMatrix));
            TileEffect.Parameters["tileSize"].SetValue(Chunk.TileSize);
            TileEffect.Parameters["tilemap"].SetValue(tilemap);
            TileEffect.Parameters["tileset"].SetValue(tileset);
            
            device.SetVertexBuffer(VertexBuffer);
            device.Indices = IndexBuffer;
            device.BlendState = BlendState.AlphaBlend;
            foreach (EffectPass pass in TileEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }
            Game.Transforms.Pop();
        }
    }
}
