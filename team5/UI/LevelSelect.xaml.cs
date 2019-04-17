using System;
using System.Collections.Generic;
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

            Previews.Add(new LevelPreview("Test", "A test level", "pack://application:,,,/Assets/Logo.scale-200.png"));
            Previews.Add(new LevelPreview("Lobby", "The introduction lobby", "pack://application:,,,/Assets/Logo.scale-200.png"));

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
        public string Name { get; }
        public string Description { get; }
        public ImageSource Preview { get; }

        public LevelPreview(string name, string description, string preview)
        {
            Name = name;
            Description = description;

            Preview = new BitmapImage(new Uri("ms-appx:///Assets/Logo.scale-200.png"));
        }
    }
}
