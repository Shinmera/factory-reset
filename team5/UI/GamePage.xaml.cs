using Windows.UI.Xaml.Controls;
using MonoGame.Framework;
using Windows.UI.Xaml;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace team5.UI
{
    public sealed partial class GamePage : Page, IPanel
    {
        public static GamePage Self;
        public readonly Game1 Game;

        public GamePage()
        {
            this.InitializeComponent();
            Self = this;
            Game = XamlGame<Game1>.Create("", Windows.UI.Xaml.Window.Current.CoreWindow, Renderer);
        }

        public void Back(BackRequestedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
