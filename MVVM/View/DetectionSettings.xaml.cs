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
