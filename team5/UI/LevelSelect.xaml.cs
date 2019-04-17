using System;
using System.Collections.Generic;
using System.IO;
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
            Root.Current.LoadGame(((LevelPreview)e.ClickedItem).Name);
        }
    }

    public class LevelPreview
    {
        public string Name => Content.name;
        public string Description => Content.description;
        public BitmapImage Preview { get; }
        private LevelContent Content;

        public LevelPreview(string name)
        {
            Content = LevelContent.Read(name, true);
            Preview = new BitmapImage();
            if (Content.previewData != null)
            {
                Preview.SetSource(Content.previewData.AsRandomAccessStream());
            }
        }
    }
}
