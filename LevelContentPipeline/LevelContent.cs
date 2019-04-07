using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace LevelContentPipeline
{
    public class LevelContent
    {
        public readonly Dictionary<string, BitmapSource> Textures = new Dictionary<string, BitmapSource>();
        public String Json;
    }
}
