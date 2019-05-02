using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace team5.UI
{
    public sealed partial class PauseMenu : StackPanel
    {
        public PauseMenu()
        {
            this.InitializeComponent();
            PauseMenuList.ItemsSource = new string[] { "Continue", "Restart", "Quit to Menu" };
        }

        public bool Shown
        {
            get { return (this.Visibility == Visibility.Visible); }
            set
            {
                this.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed;
                PauseMenuList.Focus(FocusState.Keyboard);
                PauseMenuList.SelectedIndex = 0;
            }
        }

        private void PauseItemClick(object sender, ItemClickEventArgs e)
        {
            if(!Shown) return;
            
            var item = (string)e.ClickedItem;
            switch ((string)e.ClickedItem)
            {
                case "Continue":
                    Root.Current.Game.Paused = false;
                    break;
                case "Restart":
                    Shown = false;
                    Root.Current.Game.QueueAction((game)=>game.ReloadLevel());
                    break;
                case "Quit to Menu":
                    Shown = false;
                    Root.Current.Game.QueueAction((game)=>game.UnloadLevel());
                    Root.Current.ShowMenu();
                    break;
            }
        }
    }
}
