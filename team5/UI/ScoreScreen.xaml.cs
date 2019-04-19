using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace team5.UI
{
    public sealed partial class ScoreScreen : StackPanel
    {
        public ScoreScreen()
        {
            this.InitializeComponent();
        }

        public bool Shown
        {
            get { return (this.Visibility == Visibility.Visible); }
            set
            {
                this.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed;
                Continue.Focus(FocusState.Keyboard);
            }
        }

        public void Show(Dictionary<string,string> score)
        {
            Shown = true;
            List<ScoreItem> items = new List<ScoreItem>();
            foreach (var entry in score)
                items.Add(new ScoreItem(entry.Key, entry.Value));
            ScoreList.ItemsSource = items;
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            Shown = false;
            // TODO: Advance level
        }
    }

    class ScoreItem
    {
        public readonly string Label;
        public readonly string Value;

        public ScoreItem(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }
}
