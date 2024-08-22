using System;
using System.Collections.Generic;
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



        // Event handler for the Update button
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Parse input values
            if (double.TryParse(DensityInput.Text, out double density) &&
                double.TryParse(DistanceInput.Text, out double distance))
            {
                // Example: Generate or update the image based on density and distance
                // This is where you would implement your image processing logic.
                // For this example, we'll just display a placeholder image.

                BitmapImage bitmapImage = new BitmapImage(new Uri("pack://application:,,,/YourPlaceholderImage.png"));
                ResultImage.Source = bitmapImage;
            }
            else
            {
                MessageBox.Show("Please enter valid numeric values for Density and Distance.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
