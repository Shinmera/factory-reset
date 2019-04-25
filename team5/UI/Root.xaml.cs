using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace team5.UI
{
    public sealed partial class Root : Page
    {
        public static Root Current;
        public readonly GamePage Game;
        public readonly MainMenu Menu;
        private IPanel VisiblePage;
        
        public Root()
        {
            this.InitializeComponent();
            Current = this;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            Game = new GamePage();
            Menu = new MainMenu();
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
        
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            VisiblePage.Back(e);
        }
        
        public void QueueAction(Action<Root> action)
        {
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                                                                                                  ()=>action(this)).AsTask();
        }
    }
}
