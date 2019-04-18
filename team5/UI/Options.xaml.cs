using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace team5.UI
{
    public sealed partial class Options : Page
    {
        public Options()
        {
            this.InitializeComponent();
        }

        private void MasterVolume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SoundEngine.Volume = (float)((Slider)sender).Value/100;
        }
        
        private new void Loaded(object sender, RoutedEventArgs e)
        {
            MasterVolume.Value = SoundEngine.Volume*100;
        }
    }
}
