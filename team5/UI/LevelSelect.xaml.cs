using System;
using System.Collections.Generic;
using System.IO;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace team5.UI
{
    public sealed partial class LevelSelect : Page
    {
        List<LevelPreview> Previews = new List<LevelPreview>();

        public LevelSelect()
        {
            this.InitializeComponent();

            Previews.Add(new LevelPreview("lobby"));

            LevelList.ItemsSource = Previews;
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
