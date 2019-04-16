using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace team5.UI
{
    public sealed partial class MainMenu : Page, IPane
    {
        public MainMenu()
        {
            this.InitializeComponent();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Loading Game...");
            Root.Current.Forward(new GamePage());
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            Root.Current.Forward(new Options());
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        public UIElement Show()
        {
            return this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((Button)this.FindName("start")).Focus(FocusState.Keyboard);
        }
    }
}
