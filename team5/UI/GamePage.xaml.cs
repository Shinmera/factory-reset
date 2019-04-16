using Windows.UI.Xaml.Controls;
using MonoGame.Framework;
using Windows.UI.Xaml;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace team5.UI
{
    public sealed partial class GamePage : SwapChainPanel, IPane
    {
        public static GamePage Self;
        readonly Game1 Game;

        public GamePage()
        {
            this.InitializeComponent();
            Self = this;
            // Create the game.
            Game = XamlGame<Game1>.Create("", Windows.UI.Xaml.Window.Current.CoreWindow, this);
        }

        public UIElement Show()
        {
            if (!(Game.ActiveWindow is Level))
                Game.StartLevel();
            return this;
        }
    }
}
