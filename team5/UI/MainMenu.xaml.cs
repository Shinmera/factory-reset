using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace team5.UI
{
    public sealed partial class MainMenu : Page, IPanel
    {
        private List<MenuPage> Pages = new List<MenuPage>();

        public MainMenu()
        {
            this.InitializeComponent();
            Pages.Add(new MenuPage("New Game", null));
            Pages.Add(new MenuPage("Select Level", new LevelSelect()));
            Pages.Add(new MenuPage("Options", new Options()));
            Pages.Add(new MenuPage("Quit", null));
            SidePanel.ItemsSource = Pages;
        }

        private void ShowPage(Page page)
        {
            if (Content == null) return;
            Content.Children.Clear();
            if(page != null)
                Content.Children.Add(page);
        }

        private new void Loaded(object sender, RoutedEventArgs e)
        {
            SidePanel.Focus(FocusState.Keyboard);
        }

        private void ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (MenuPage)e.ClickedItem;
            if(item.Name.Equals("New Game"))
            {
                System.Diagnostics.Debug.WriteLine("Loading Game...");
                Root.Current.ShowGame();
                Root.Current.Game.QueueAction((game)=>game.LoadLevel(Game1.FirstLevel));
            }
            else if(item.Name.Equals("Quit"))
            {
                Application.Current.Exit();
            }
            else
            {
                item.Page.Focus(FocusState.Keyboard);
            }
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count <= 0) return;
            ShowPage(((MenuPage)e.AddedItems[0]).Page);
        }

        public void Back(BackRequestedEventArgs e)
        {

        }
    }

    class MenuPage
    {
        public readonly Page Page;
        public readonly string Name;

        public MenuPage(string name, Page page)
        {
            Page = page;
            Name = name;
        }
    }
}
