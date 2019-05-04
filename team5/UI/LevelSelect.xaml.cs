using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace team5.UI
{
    public sealed partial class LevelSelect : Page
    {
        Dictionary<string, LevelPreview> LevelNames = new Dictionary<string, LevelPreview>();

        ObservableCollection<LevelPreview> Previews = new ObservableCollection<LevelPreview>();

        public LevelSelect()
        {
            this.InitializeComponent();
            LevelList.ItemsSource = Previews;
            LoadLevels();
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count <= 0) return;
            var level = (LevelPreview)e.AddedItems[0];
            Description.Text = level.Description;
        }

        private void LevelSelected(object sender, ItemClickEventArgs e)
        {
            String level = ((LevelPreview)e.ClickedItem).FileName;
            Root.Current.Game.InvokeAction((game) => game.UpdateLoadName(level));
            Root.Current.ShowGame();
            Root.Current.Game.QueueAction((game)=>game.LoadLevel(level));
        }

        private async void LoadLevels()
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder content = await appInstalledFolder.GetFolderAsync("Content");
            StorageFolder levels = await content.GetFolderAsync("Levels");

            var UnsortedPreviews = new List<LevelPreview>();

            foreach(var file in await levels.GetFilesAsync())
            {
                if (file.Name.EndsWith(".zip"))
                {
                    var preview = new LevelPreview(file.Name.Substring(0, file.Name.Length - 4));
                    UnsortedPreviews.Add(preview);
                    LevelNames[file.Name.Substring(0, file.Name.Length - 4)] = preview;
                }
            }

            var previewEnum = Previews.GetEnumerator();

            for (int i = 0; i < UnsortedPreviews.Count; ++i)
            {
                var level = UnsortedPreviews[i];
                if(level.Next != null)
                {
                    int nextLevelPos = UnsortedPreviews.IndexOf(LevelNames[level.Next]);

                    if(nextLevelPos == -1)
                    {
                        continue;
                    }

                    if(nextLevelPos < i)
                    {
                        UnsortedPreviews.RemoveAt(i);

                        UnsortedPreviews.Insert(nextLevelPos, level);
                    }
                }
            }
                
            foreach (var level in UnsortedPreviews){
                Previews.Add(level);
            }

            Root.Current.Game.InvokeAction((game) => game.FirstLevel = Previews[0].FileName);
        }

        private void Sideload(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            string url = SideloadUrl.Text;
            Root.Current.Game.InvokeAction((game) => game.UpdateLoadName(null));
            Root.Current.ShowGame();
            Root.Current.Game.QueueAction((game)=>game.LoadLevel(new Uri(url)));
        }
    }

    public class LevelPreview
    {
        public readonly string FileName;
        public string Name => Content.name;
        public string Description => Content.description;
        public string Next => Content.next;
        public SoftwareBitmapSource Preview { get; }
        private LevelContent Content;


        public LevelPreview(string fileName)
        {
            FileName = fileName;
            Content = LevelContent.Read(fileName, true);
            Preview = new SoftwareBitmapSource();
            if (Content.previewData != null)
                LoadPreview(Content.previewData);
        }

        // This dumbass workaround is necessary as the direct usage of
        // BitmapImage.SetSource does not produce a bitmap at all, and 
        // BitmapImage.SetSourceAsync crashes with an access violation.
        // Thanks for the cool shit, Microsoft.
        private async void LoadPreview(Stream preview)
        {
            var decoder = await BitmapDecoder.CreateAsync(preview.AsRandomAccessStream());
            var bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            await Preview.SetBitmapAsync(bitmap);
        }
    }
}
