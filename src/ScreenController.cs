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
        public static Image CaptureDesktop(string filename = "clickless", string path = "")
        {
            var image = CaptureDesktop();
            if (path == "")
            {
                path = Path.Combine(Environment.CurrentDirectory, "images", filename + ".png");
                image.Save(path, ImageFormat.Png);
            }
            return image;
        }
    }

    public class ScreenCapture
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetDesktopWindow();

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        public static Image CaptureDesktop()
        {
            return CaptureWindow(GetDesktopWindow());
        }

        public static Bitmap CaptureActiveWindow()
        {
            return CaptureWindow(GetForegroundWindow());
        }

        public static Bitmap CaptureWindow(IntPtr handle)
        {
            var rect = new Rect();
            GetWindowRect(handle, ref rect);
            var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            var result = new Bitmap(bounds.Width, bounds.Height);

            using (var graphics = Graphics.FromImage(result))
            {
                graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }

            return result;
        }
    }
}
