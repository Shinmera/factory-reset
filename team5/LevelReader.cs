using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace team5
{
    class LevelReader : ContentTypeReader<LevelContent>
    {
        protected override LevelContent Read(ContentReader input, LevelContent existing)
        {
            var device = ((IGraphicsDeviceService)input.ContentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
            var content = JsonConvert.DeserializeObject<LevelContent>(input.ReadString());
            // Init map arrays
            foreach(var chunk in content.chunks)
                chunk.maps = new Texture2D[chunk.layers];
            
            // Load textures and tie in
            var textureCount = input.ReadInt32();
            for (int i=0; i<textureCount; ++i)
            {
                string name = input.ReadString();
                int layer = input.ReadInt32();
                int width = input.ReadInt32();
                int height = input.ReadInt32();
                byte[] data = input.ReadBytes(width*height*4);
                Texture2D texture = new Texture2D(device, width, height);
                texture.SetData(data);
                foreach(var chunk in content.chunks)
                    if(chunk.name.Equals(name))
                        chunk.maps[layer] = texture;
            }
            
            return content;
        }
    }
}
