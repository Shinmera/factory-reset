using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    public class TextureCache
    {
        private readonly ContentManager Content;
        private readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();
        
        public TextureCache(ContentManager content)
        {
            Content = content;
        }
        
        public Texture2D Get(string texture)
        {
            if(!Cache.ContainsKey(texture))
                Cache.Add(texture, Content.Load<Texture2D>("Textures/"+texture));
            return Cache[texture];
        }
        
        public void UnloadContent()
        {
            foreach(var texture in Cache)
                texture.Value.Dispose();
            Cache.Clear();
        }
        
        public Texture2D this[string texture]
        {
            get { return Get(texture); }
        }
    }
}
