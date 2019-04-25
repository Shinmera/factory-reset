using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    public class TextureCache
    {
        private readonly Game1 Game;
        private readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();
        
        public TextureCache(Game1 game)
        {
            Game = game;
        }
        
        public Texture2D Get(string texture)
        {
            if(!Cache.ContainsKey(texture))
            {
                Game1.Log("TextureCache","Loading {0}",texture);
                Cache.Add(texture, Game.Content.Load<Texture2D>("Textures/"+texture));
                // Callback to advance load screen
                Game.AdvanceLoad();
            }
            return Cache[texture];
        }
        
        public void UnloadContent()
        {
            foreach(var texture in Cache)
            {
                Game1.Log("TextureCache","Unloading {0}",texture.Key);
                texture.Value.Dispose();
            }
            Cache.Clear();
        }
        
        public Texture2D this[string texture]
        {
            get { return Get(texture); }
        }
    }
}
