using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelContentPipeline
{
    class LevelData
    {
        public struct Texture
        {
            public String Chunk;
            public int Layer;
            public int Width;
            public int Height;
            public byte[] Data;
        }
        public readonly List<Texture> Textures = new List<Texture>();
        public String Json;
    }
}
