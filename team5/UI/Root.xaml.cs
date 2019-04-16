using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        private Stack<IPane> History = new Stack<IPane>();
        
        public Root()
        {
            this.InitializeComponent();
            Current = this;
            Forward(new MainMenu());
        }
        
        private void Show(IPane pane)
        {
            UIElement show = pane.Show();
            Grid grid = ((Grid)FindName("centralGrid"));
            grid.Children.Clear();
            grid.Children.Add(show);
        }
        
        private void Show()
        {
            Show(History.Peek());
        }

        public bool Back()
        {
            if (!History.TryPeek(out _))
                return false;
            History.Pop();
            Show();
            return true;
        }
        
        public void Forward(IPane pane)
        {
            History.Push(pane);
            Show();
        }
    }
}
