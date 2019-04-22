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
            }
        }

        private void PauseItemClick(object sender, ItemClickEventArgs e)
        {
            if(!Shown) return;
            Shown = false;

            var item = (string)e.ClickedItem;
            switch ((string)e.ClickedItem)
            {
                case "Continue":
                    Root.Current.Game.Game.Paused = false;
                    break;
                case "Restart":
                    Root.Current.Game.Game.ReloadLevel();
                    break;
                case "Quit to Menu":
                    Root.Current.Game.Game.UnloadLevel();
                    Root.Current.ShowMenu();
                    break;
            }
        }
    }
}
