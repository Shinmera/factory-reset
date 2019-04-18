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
        ObservableCollection<LevelPreview> Previews = new ObservableCollection<LevelPreview>();

        public LevelSelect()
        {
            this.InitializeComponent();
            LevelList.ItemsSource = Previews;
            LoadLevels();
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var level = (LevelPreview)e.AddedItems[0];
            Description.Text = level.Description;
        }

        private void LevelSelected(object sender, ItemClickEventArgs e)
        {
            Root.Current.LoadGame(((LevelPreview)e.ClickedItem).FileName);
        }

        private async void LoadLevels()
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder content = await appInstalledFolder.GetFolderAsync("Content");
            StorageFolder levels = await content.GetFolderAsync("Levels");
            foreach(var file in await levels.GetFilesAsync())
            {
                if (file.Name.EndsWith(".zip"))
                    Previews.Add(new LevelPreview(file.Name.Substring(0, file.Name.Length - 4)));
            }
        }
    }

    public class LevelPreview
    {
        public readonly string FileName;
        public string Name => Content.name;
        public string Description => Content.description;
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
