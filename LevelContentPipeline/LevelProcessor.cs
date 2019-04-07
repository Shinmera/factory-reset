using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelContentPipeline
{
    [ContentProcessor(DisplayName = "Level Processor")]
    class LevelProcessor : ContentProcessor<LevelContent, LevelData>
    {
        public override LevelData Process(LevelContent input, ContentProcessorContext context)
        {
            LevelData data = new LevelData();
            data.Json = input.Json;
            foreach (var entry in input.Textures)
            {
                var filename = entry.Key;
                var dash = filename.LastIndexOf('-');
                var dot = filename.LastIndexOf('.');
                var width = entry.Value.PixelWidth;
                var height = entry.Value.PixelHeight;
                int stride = width * 4;
                byte[] bits = new byte[height * stride];
                entry.Value.CopyPixels(bits, stride, 0);

                data.Textures.Add(new LevelData.Texture {
                    Chunk = filename.Substring(0, dash),
                    Layer = int.Parse(filename.Substring(dash + 1, dot - (dash + 1))),
                    Width = width,
                    Height = height,
                    Data = bits,
                });
            }
            return data;
        }
    }
}
