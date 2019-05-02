using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace team5.UI
{
    public sealed partial class Options : Page
    {
        private ApplicationDataContainer Settings;
        
        public Options()
        {
            this.InitializeComponent();
            Settings = ApplicationData.Current.RoamingSettings;
            T setDefault<T>(string key, T value){
                if(!Settings.Values.ContainsKey(key))
                    Settings.Values[key] = value;
                return (T)Settings.Values[key];
            }
            // Set defaults and restore in one.
            SoundEngine.Volume = setDefault("MasterVolume", SoundEngine.Volume);
            Camera.ScreenShakeMultiplier = setDefault("ScreenShake", Camera.ScreenShakeMultiplier);
            Controller.VibrationMultiplier = setDefault("Vibration", Controller.VibrationMultiplier);
            DialogBox.TimePerLetter = setDefault("TextSpeed", DialogBox.TimePerLetter);
            Chunk.DrawSolids = setDefault("ShowSolids", Chunk.DrawSolids);
            setDefault("TvMode", 1);
        }

        private new void Loaded(object sender, RoutedEventArgs e)
        {
            // Apply control defaults
            MasterVolume.Value = SoundEngine.Volume * 100;
            ScreenShake.Value = Camera.ScreenShakeMultiplier * 100;
            Vibration.Value = Controller.VibrationMultiplier * 100;
            TextSpeed.Value = 100-(DialogBox.TimePerLetter*100/0.05f);
            ShowSolids.IsChecked = Chunk.DrawSolids;
            TvMode.SelectedIndex = (int)Settings.Values["TvMode"];
        }

        private void MasterVolume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SoundEngine.Volume = (float)MasterVolume.Value / 100;
            Settings.Values["MasterVolume"] = SoundEngine.Volume;
        }

        private void TextSpeed_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            DialogBox.TimePerLetter = (float)(100-TextSpeed.Value)*0.05f/100;
            Settings.Values["TextSpeed"] = DialogBox.TimePerLetter;
        }

        private void ScreenShake_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Camera.ScreenShakeMultiplier = (float)ScreenShake.Value / 100;
            Settings.Values["ScreenShake"] = Camera.ScreenShakeMultiplier;
        }

        private void Vibration_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Controller.VibrationMultiplier = (float)Vibration.Value / 100;
            Settings.Values["Vibration"] = Controller.VibrationMultiplier;
        }

        private void ShowSolids_Click(object sender, RoutedEventArgs e)
        {
            Chunk.DrawSolids = ShowSolids.IsChecked.Value;
            Settings.Values["ShowSolids"] = Chunk.DrawSolids;
        }

        private void TvMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Root.Current.Menu == null) return;

            var view = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            int width, height;
            
            switch(TvMode.SelectedIndex){
                case 0:
                    view.SetDesiredBoundsMode(Windows.UI.ViewManagement.ApplicationViewBoundsMode.UseVisible);
                    // KLUDGE: I can't fucking figure out how to do this properly.
                    width = 1728; height = 972;
                    break;
                case 1: 
                    view.SetDesiredBoundsMode(Windows.UI.ViewManagement.ApplicationViewBoundsMode.UseCoreWindow);
                    width = 1920; height = 1080;
                    break;
                default: return;
            }

            Root.Current.Game.QueueAction((game) => {
                game.DeviceManager.PreferredBackBufferWidth = width;
                game.DeviceManager.PreferredBackBufferHeight = height;
                game.DeviceManager.ApplyChanges();
                game.Resize(width, height);
            });
            
            Settings.Values["TvMode"] = TvMode.SelectedIndex;
        }
    }
}
