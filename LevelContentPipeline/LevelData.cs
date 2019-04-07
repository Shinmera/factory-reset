using System;
using System.Collections.Generic;

namespace LevelContentPipeline
{
    class LevelData
    {
        public struct Texture
        {
            public String Chunk;
            public int Layer;
            public byte[] Data;
        }
        public readonly List<Texture> Textures = new List<Texture>();
        public String Json;
    }
}
