using Clickless.Core;
using Clickless.MVVM.Models;
using PropertyChanged;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

namespace Clickless.MVVM.ViewModel
{

    [AddINotifyPropertyChangedInterface]
    class GridSettingsViewModel : ObservableObject
    {
        public RelayCommand SaveSettingsCommand { get; set; }
        public ImageSource ImageSource { get; set; }
        private Bitmap image {  get; set; }


        private GridSettingsModel _gridSearchSettings;
        public GridSettingsModel GridSearchSettings
        {
            get => _gridSearchSettings;
            set
            {
                _gridSearchSettings = value;
                UpdateImage();
                SaveSettings();
            }
        }


        public void UpdateImage()
        {

            image ??= new Bitmap(MonitorUtilities.GetSize().Width, MonitorUtilities.GetSize().Height);
            //var bg = MonitorUtilities.CaptureDesktopBitmap();

            using (Graphics g = Graphics.FromImage(image)) {
                g.Clear(Color.Black);
                //g.DrawImage(bg, 0, 0);

                //Draw the cells.
                var cells = MonitorUtilities.ObtainRectGrid(0, GridSearchSettings.cellSize);
                g.DrawRectangles(new Pen( new SolidBrush(Color.Aqua), 10), cells.ToArray());

                var lastRect = cells[0];
                //Draw the inner cells for the upper left.
                for (int i = 0; i < GridSearchSettings.recursionCount; i++)
                {
                    var recursedRects = GridClient.RecurseOnGridBox(lastRect, GridSearchSettings.n);
                    g.DrawRectangles(new Pen(new SolidBrush(Color.White), i + 2), recursedRects.ToArray());
                    lastRect = recursedRects[0];
                }
            }

            ImageSource = BitmapToImageSource(image);
        }

        public void SaveSettings()
        {

            ObjectSerializer.SaveData(_gridSearchSettings);
        }


        public GridSettingsViewModel()
        {
            _gridSearchSettings = ObjectSerializer.LoadDataOrDefault<GridSettingsModel>();

            SaveSettingsCommand = new RelayCommand( (o) => { SaveSettings(); UpdateImage(); });
        }

        //TODO: Move into utility class.
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
