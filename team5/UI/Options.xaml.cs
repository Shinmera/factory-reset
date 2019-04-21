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

        private new void Loaded(object sender, RoutedEventArgs e)
        {
            MasterVolume.Value = SoundEngine.Volume * 100;
            // Map 0.0 ... 0.05 to 100 ... 0
            TextSpeed.Value = 100-(DialogBox.TimePerLetter*100/0.05f);
        }

        private void MasterVolume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SoundEngine.Volume = (float)((Slider)sender).Value / 100;
        }

        private void TextSpeed_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            DialogBox.TimePerLetter = (100-((float)((Slider)sender).Value))*0.05f/100;
        }
    }
}
