using Microsoft.Xna.Framework.Content.Pipeline;

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
                string filename = entry.Key;
                int dash = filename.LastIndexOf('-');
                int dot = filename.LastIndexOf('.');

                data.Textures.Add(new LevelData.Texture {
                    Chunk = filename.Substring(0, dash),
                    Layer = int.Parse(filename.Substring(dash + 1, dot - (dash + 1))),
                    Data = entry.Value,
                });
            }
            return data;
        }
    }
}
