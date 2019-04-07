using System;
using System.Collections.Generic;

namespace LevelContentPipeline
{
    public class LevelContent
    {
        public readonly Dictionary<string, byte[]> Textures = new Dictionary<string, byte[]>();
        public String Json;
    }
}
