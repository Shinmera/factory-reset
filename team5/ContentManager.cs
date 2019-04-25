using System;

namespace team5
{
    public class ContentManager : Microsoft.Xna.Framework.Content.ContentManager
    {
        public ContentManager(IServiceProvider provider) : base(provider)
        { }

        /// <summary>
        ///   This method allows us to read a fresh asset always. The default Load that
        ///   the contentmanager provides actually performs caching that we want to
        ///   circumvent, as we use our own resource tracking scheme that's more fine-
        ///   grained than what this allows by default.
        /// </summary>
        public T ReadAsset<T>(string assetName)
        {
            return base.ReadAsset<T>(assetName, null);
        }
    }
}
