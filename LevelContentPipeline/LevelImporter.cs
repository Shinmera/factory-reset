using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;

namespace LevelContentPipeline
{
    [ContentImporter(".zip", DefaultProcessor = "LevelProcessor", DisplayName = "Level Importer")]
    public class LevelImporter : ContentImporter<LevelContent>
    {
        public override LevelContent Import(string filename, ContentImporterContext context)
        {
            context.Logger.LogMessage("Importing Level: {0}", filename);
            LevelContent level = new LevelContent();

            using(var stream = new FileStream(filename, FileMode.Open))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.Equals("level.json"))
                        using (var jsonStream = entry.Open())
                        using (var reader = new StreamReader(jsonStream))
                        {
                            level.Json = reader.ReadToEnd();
                        }
                    else if (entry.FullName.EndsWith(".png"))
                        using (var pngStream = entry.Open())
                        using (var memory = new MemoryStream())
                        {
                            pngStream.CopyTo(memory);
                            level.Textures.Add(entry.Name, memory.ToArray());
                        }
                    else
                        context.Logger.LogImportantMessage("Unknown file in archive: {0}", entry.FullName);
                }
            }

            return level;
        }
    }
}
