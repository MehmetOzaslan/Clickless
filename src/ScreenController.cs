using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Clickless.MouseController;

namespace Clickless.src
{


    public static class ScreenController
    {

        public static Image CaptureDesktop()
        {
            Image image = ScreenCapture.CaptureDesktop();
            return image;
        }

        /// <summary>
        /// Defaults to src/images if saving.
        /// Name: Clickless
        /// Type: Jpeg
        /// </summary>
        /// <param name="save"></param>
        /// <param name="filename"></param>
        /// <param name="path"></param>
        public static Image CaptureDesktop(string filename = "clickless")
        {
            var image = CaptureDesktop();
            var saved = new Bitmap(image);
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "images");
            Directory.CreateDirectory(path); //Safety check if the images folder doesn't exist.
            saved.Save(Path.Combine(path, filename), ImageFormat.Png);
            return image;
        }
    }

    public class ScreenCapture
    {
        public static Image CaptureDesktop()
        {
            int screenLeft = SystemInformation.VirtualScreen.Left;
            int screenTop = SystemInformation.VirtualScreen.Top;
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            // Create a bitmap of the appropriate size to receive the full-screen screenshot.
            Image bitmap = new Bitmap(screenWidth, screenHeight);
            Graphics g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(screenLeft, screenTop, 0, 0, bitmap.Size);
            return bitmap;
            
        }
    }
}
