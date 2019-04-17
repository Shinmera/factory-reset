using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace team5.UI
{
    public sealed partial class MainMenu : Page, IPanel
    {
        private Dictionary<string, Page> Pages = new Dictionary<string, Page>();

        public MainMenu()
        {
            this.InitializeComponent();
            Pages["Options"] = new Options();
            Pages["Load"] = new LevelSelect();
        }

        private void ShowPage(string name)
        {
            if (Content == null) return;
            Content.Children.Clear();
            if(Pages.TryGetValue(name, out Page page))
                Content.Children.Add(page);
        }

        private new void Loaded(object sender, RoutedEventArgs e)
        {
            SidePanel.Focus(FocusState.Keyboard);
        }

        private void ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (string)e.ClickedItem;
            if(item.Equals("New Game"))
            {
                System.Diagnostics.Debug.WriteLine("Loading Game...");
                Root.Current.LoadGame();
            }
            else if(item.Equals("Quit"))
            {
                Application.Current.Exit();
            }
            else
            {
                Pages[item].Focus(FocusState.Keyboard);
            }
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (string)e.AddedItems[0];
            ShowPage(item);
        }

        public void Back(BackRequestedEventArgs e)
        {

        }
    }
}
