using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace team5.UI
{
    public sealed partial class Root : Page
    {
        public static Root Current;
        private GamePage Game;
        private MainMenu Menu;
        private IPanel VisiblePage;
        
        public Root()
        {
            this.InitializeComponent();
            Current = this;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            Menu = new MainMenu();
            Game = new GamePage();
            ShowMenu();
        }

        private void Show(Page page)
        {
            VisiblePage = (IPanel)page;
            CentralGrid.Children.Clear();
            CentralGrid.Children.Add(page);
        }

        public void ShowMenu()
        {
            Show(Menu);
        }

        public void ShowGame()
        {
            Show(Game);
        }

        public void LoadGame(string level = Game1.FirstLevel)
        {
            ShowGame();
            Game.Game.LoadLevel(level);
        }
        
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            VisiblePage.Back(e);
        }
    }
}
