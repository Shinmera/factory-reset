using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
#pragma warning disable 0649
    class LevelContent
    {
        public class Chunk{
            public string name;
            public float[] position;
            public int layers;
            public string tileset;
            public String[] storyItems;
            public Texture2D[] maps;
        };
        
        public string name;
        public string description;
        public bool startChase;
        public int startChunk;
        public Chunk[] chunks;
    }
#pragma warning restore 0649
}
