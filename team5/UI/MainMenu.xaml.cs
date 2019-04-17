using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace team5.UI
{
    public sealed partial class MainMenu : Page, IPane
    {
        private Dictionary<string, Page> Pages = new Dictionary<string, Page>();
        private Grid Container;

        public MainMenu()
        {
            this.InitializeComponent();
            Pages["Options"] = new Options();
            Pages["Load"] = new LevelSelect();
        }

        public UIElement Show()
        {
            return this;
        }

        private void ShowPage(string name)
        {
            if (Container == null) return;
            Container.Children.Clear();
            if(Pages.TryGetValue(name, out Page page))
                Container.Children.Add(page);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((ListView)FindName("SidePanel")).Focus(FocusState.Keyboard);
            Container = (Grid)FindName("Content");
        }

        private void ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (string)e.ClickedItem;
            if(item.Equals("New Game"))
            {
                System.Diagnostics.Debug.WriteLine("Loading Game...");
                Root.Current.Forward(new GamePage());
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
    }
}
