using Windows.UI.Xaml.Controls;
using MonoGame.Framework;
using Windows.UI.Xaml;
using Windows.UI.Core;

namespace team5.UI
{
    public sealed partial class GamePage : Page, IPanel
    {
        public readonly Game1 Game;

        public GamePage()
        {
            this.InitializeComponent();
            Game = XamlGame<Game1>.Create("", Windows.UI.Xaml.Window.Current.CoreWindow, Renderer);
            PauseMenuList.ItemsSource = new string[]{"Continue","Restart","Quit to Menu"};
        }

        public void Back(BackRequestedEventArgs e)
        {
            Paused = false;
            e.Handled = true;
        }

        public bool Paused {
            get { return (PauseMenu.Visibility == Visibility.Visible); }
            set
            {
                if (value == Paused) return;
                PauseMenu.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed;
                PauseMenuList.Focus(FocusState.Keyboard);
                Game.Paused = value;
            }
        }

        private void PauseItemClick(object sender, ItemClickEventArgs e)
        {
            if (PauseMenu.Visibility != Visibility.Visible) return;
            Paused = false;

            var item = (string)e.ClickedItem;
            switch ((string)e.ClickedItem)
            {
                case "Continue":
                    Game.Paused = false;
                    break;
                case "Restart":
                    break;
                case "Quit to Menu":
                    Game.UnloadLevel();
                    Root.Current.ShowMenu();
                    break;
            }
        }
    }
}
