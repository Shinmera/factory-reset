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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace team5.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Options : Page, IPane
    {
        public Options()
        {
            this.InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Root.Current.Back();
        }

        public UIElement Show()
        {
            return this;
        }

        private void MasterVolume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SoundEngine.Volume = (float)((Slider)sender).Value/100;
        }
        
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((Slider)FindName("masterVolume")).Value = SoundEngine.Volume*100;
        }
    }
}
