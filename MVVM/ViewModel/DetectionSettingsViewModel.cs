using Clickless.Core;
using PropertyChanged;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Clickless.MVVM.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    class DetectionSettingsViewModel
    {
        private DetectionSettings _detectionSettings;
        public RelayCommand SaveSettingsCommand { get; set; }

        public ImageSource ResultImage { get; set; }
        public DetectionSettings DetectionSettings
        {
            get => _detectionSettings;
            set
            {
                _detectionSettings = value;
                MLClient.UpdateSettings(_detectionSettings);
                SaveSettings();
            }
        }
        public WindowState WindowState {  get; set; }

        private void Minimize()
        {
            WindowState = WindowState.Minimized;
        }

        private void Restore()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
        }

        public DetectionSettingsViewModel()
        {
            _detectionSettings = LoadSettings();
            MLClient.UpdateSettings(_detectionSettings);
            SaveSettingsCommand = new RelayCommand((obj) => { SaveSettings(); }, (obj) => { return true; });
        }

        private DetectionSettings LoadSettings()
        {
            return ObjectSerializer.LoadDataOrDefault<DetectionSettings>();
        }

        private void CaptureBGExcludeForm()
        {
            Minimize();
            MonitorUtilities.CaptureDesktopBitmap();
            MLClient.GetBboxes(MonitorUtilities.CaptureDesktopBitmap());
            Restore();
        }

        private void SaveSettings()
        {
            //GetRects(MonitorUtilities.CaptureDesktopBitmap());
            CaptureBGExcludeForm();
            ResultImage = BitmapToImageSource( MLClient.GetEngineImagePasses()[0]);
            ObjectSerializer.SaveData(_detectionSettings);
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }
    }
}
