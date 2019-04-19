using Windows.UI.Xaml.Controls;
using MonoGame.Framework;
using Windows.UI.Xaml;
using Windows.UI.Core;
using System.Collections.Generic;

namespace team5.UI
{
    public sealed partial class GamePage : Page, IPanel
    {
        public readonly Game1 Game;

        public GamePage()
        {
            this.InitializeComponent();
            Game = XamlGame<Game1>.Create("", Windows.UI.Xaml.Window.Current.CoreWindow, Renderer);
        }

        public void Back(BackRequestedEventArgs e)
        {
            Paused = false;
            e.Handled = true;
        }

        public bool Paused {
            get { return PauseMenu.Shown; }
            set
            {
                if (value == Paused) return;
                PauseMenu.Shown = value;
                Game.Paused = value;
            }
        }

        public void ShowScore(Dictionary<string, string> score)
        {
            ScoreScreen.Show(score);
        }
    }
}
