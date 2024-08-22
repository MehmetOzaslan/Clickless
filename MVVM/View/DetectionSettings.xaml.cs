using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Clickless.MVVM.View
{
    /// <summary>
    /// Interaction logic for DetectionSettings.xaml
    /// </summary>
    public partial class DetectionSettings : UserControl
    {
        public DetectionSettings()
        {
            InitializeComponent();
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


        // Event handler for the Update button
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {



            int.TryParse(DistanceInput.Text, out var distance);
            int.TryParse(DensityInput.Text, out var density);

            MLClient.UpdateSettings(new Clickless.DetectionSettings() { m = density, cannythresh1 = 100, cannythresh2 = 200, iterations = 20, epsilon = distance });
            ResultImage.Source = BitmapToImageSource(MLClient.GetEngineImagePasses()[0]);

            

            //// Parse input values
            //if (double.TryParse(DensityInput.Text, out double density) &&
            //    double.TryParse(DistanceInput.Text, out double distance))
            //{

            //    BitmapImage bitmapImage = new BitmapImage(new Uri("pack://application:,,,/YourPlaceholderImage.png"));
            //    ResultImage.Source = bitmapImage;
            //}
            //else
            //{
            //    MessageBox.Show("Please enter valid numeric values for Density and Distance.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }

        // Event handler to ensure only numeric input in the TextBoxes
        private void NumericOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private static bool IsTextNumeric(string text)
        {
            return int.TryParse(text, out int res);
        }
    }
}
