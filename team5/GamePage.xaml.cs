using Windows.UI.Xaml.Controls;
using MonoGame.Framework;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace team5
{
    public sealed partial class GamePage : SwapChainPanel
    {
        public static GamePage Self;
        readonly Game1 Game;

        public GamePage(string launchArguments)
        {
            this.InitializeComponent();
            Self = this;
            // Create the game.
            Game = XamlGame<Game1>.Create(launchArguments, Windows.UI.Xaml.Window.Current.CoreWindow, this);
        }
    }
}
