using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace LevelContentPipeline
{
    [ContentTypeWriter]
    class LevelWriter : ContentTypeWriter<LevelData>
    {
        protected override void Write(ContentWriter output, LevelData value)
        {
            output.Write(value.Json);
            output.Write(value.Textures.Count);
            foreach(var texture in value.Textures)
            {
                output.Write(texture.Chunk);
                output.Write(texture.Layer);
                output.Write(texture.Width);
                output.Write(texture.Height);
                output.Write(texture.Data);
            }
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "team5.LevelContent, team5";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "team5.LevelReader, team5";
        }
    }
}
