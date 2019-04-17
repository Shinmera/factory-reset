using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace team5
{
    #pragma warning disable 0649
    class LevelContent : IDisposable
    {
        public class Chunk{
            public string name;
            public float[] position;
            public String[] layers;
            public string tileset;
            public String[] storyItems;
            public Texture2D[] maps;
        };
        
        public string name;
        public string description;
        public string preview;
        public bool startChase;
        public int startChunk;
        public Chunk[] chunks;
        public Stream previewData;
        public Dictionary<string, Stream> textures = new Dictionary<string, Stream>();
        
        public void Resolve(GraphicsDevice device)
        {
            foreach(var chunk in chunks)
            {
                if(chunk.maps != null && 0 < chunk.maps.Length) continue;
                chunk.maps = new Texture2D[chunk.layers.Length];
                for(int i=0; i<chunk.layers.Length; ++i)
                {
                    Stream stream = textures[chunk.layers[i]];
                    chunk.maps[i] = Texture2D.FromStream(device, stream);
                }
            }
        }
        
        public void Dispose()
        {
            if(previewData != null) 
                previewData.Close();
            foreach(var entry in textures)
                entry.Value.Close();
        }
        
        public static LevelContent Read(String level, bool readMetadata = false)
        {
            return Task.Run(()=>ReadAsync(level, readMetadata)).Result;
        }
        
        public static LevelContent Read(Uri uri, bool readMetadata = false)
        {
            return Task.Run(()=>ReadAsync(uri, readMetadata)).Result;
        }
        
        public static Task<LevelContent> ReadAsync(String level, bool readMetadata = false)
        {
            return ReadAsync(new Uri("ms-appx:///Content/Levels/"+level+".zip"), readMetadata);
        }

        public static async Task<LevelContent> ReadAsync(Uri uri, bool readMetadata = false)
        {
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            var asyncStream = await file.OpenSequentialReadAsync();
            using (var stream = asyncStream.AsStreamForRead())
                return Read(stream, readMetadata);
        }
        
        public static LevelContent Read(Stream stream, bool readMetadata = false)
        {
            LevelContent content = null;

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                // Load base content metadata
                var entry = archive.GetEntry("level.json");
                using (var jsonStream = entry.Open())
                using (var reader = new StreamReader(jsonStream))
                using (var json = new JsonTextReader(reader))
                {
                    content = new JsonSerializer().Deserialize<LevelContent>(json);
                }

                MemoryStream readZipEntry(string file)
                {
                    var localEntry = archive.GetEntry(file);
                    using (var localStream = localEntry.Open())
                    {
                        var memory = new MemoryStream();
                        localStream.CopyTo(memory);
                        return memory;
                    }
                }

                // Load texture files
                if (readMetadata)
                {
                    if (content.preview != null)
                        content.previewData = readZipEntry(content.preview);
                }
                else
                {
                    foreach (var chunk in content.chunks)
                        foreach (var layer in chunk.layers)
                            content.textures[layer] = readZipEntry(layer);
                }
            }
            
            return content;
        }
    }
    #pragma warning restore 0649
}
