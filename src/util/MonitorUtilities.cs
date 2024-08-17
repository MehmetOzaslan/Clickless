using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Util;

namespace Clickless
{
    public static class MonitorUtilities
    {
        public static Size GetSize() { return SystemInformation.VirtualScreen.Size; }
        public static int GetLeft() { return SystemInformation.VirtualScreen.Left; }
        public static int GetTop() { return SystemInformation.VirtualScreen.Top; }

        private static string _save_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "images");
        public static string GetSavePath()
        {
            Directory.CreateDirectory(_save_path); //Safety check if the images folder doesn't exist.
            return _save_path;
        }


        /// <summary>
        /// Provides a list of points across the screen in a grid pattern. 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static List<MathUtil.Vector2> ObtainGrid(int margins=30, int spacing = 30)
        {
            int width = SystemInformation.VirtualScreen.Width;
            int height = SystemInformation.VirtualScreen.Height;
            int numRow = (width - margins * 2) / spacing;
            int numCol = (height - margins * 2) / spacing;
            List<MathUtil.Vector2> outList = new List<MathUtil.Vector2>();

            float dx = 1.0f / numRow;
            float dy = 1.0f / numCol;

            //i and j are bound between 0 and 1, representing the % position on the screen.
            for (float i = 0; i <= 1; i+=dx)
            {
                for (float j = 0; j <= 1; j += dy)
                {
                    float x = MathUtil.Lerp(margins, width - margins, i);
                    float y = MathUtil.Lerp(margins, height - margins, j);
                    outList.Add(new MathUtil.Vector2(x, y));
                }
            }
            return outList;
        }

        public static List<MathUtil.Vector2> ObtainGrid(RECT rect, int margins = 30, int spacing = 30)
        {
            int width = Math.Abs(rect.Left-rect.Right);
            int height = Math.Abs(rect.Top-rect.Bottom);
            int bottomOffset = rect.Bottom;
            int leftOffset = rect.Left;

            int numRow = (width - margins * 2) / spacing;
            int numCol = (height - margins * 2) / spacing;
            List<MathUtil.Vector2> outList = new List<MathUtil.Vector2>();

            float dx = 1.0f / numRow;
            float dy = 1.0f / numCol;

            //i and j are bound between 0 and 1, representing the % position on the screen.
            for (float i = 0; i <= 1; i += dx)
            {
                for (float j = 0; j <= 1; j += dy)
                {
                    float x = MathUtil.Lerp(margins, width - margins, i) + leftOffset;
                    float y = MathUtil.Lerp(margins, height - margins, j) + bottomOffset;
                    outList.Add(new MathUtil.Vector2(x, y));
                }
            }
            return outList;
        }




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

        public static Bitmap CaptureDesktopBitmap()
        {
            int screenLeft = SystemInformation.VirtualScreen.Left;
            int screenTop = SystemInformation.VirtualScreen.Top;
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            // Create a bitmap of the appropriate size to receive the full-screen screenshot.
            var bitmap = new Bitmap(screenWidth, screenHeight);
            Graphics g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(screenLeft, screenTop, 0, 0, bitmap.Size);
            return bitmap;
        }


        public static Image CaptureRegion(Rectangle rect)
        {
            // Create a bitmap of the appropriate size to receive the full-screen screenshot.
            Image bitmap = new Bitmap(rect.Width, rect.Height);
            Graphics g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(rect.Left, rect.Top, rect.Right, rect.Bottom, bitmap.Size);
            return bitmap;
        }

        public static Image CaptureSquare(int x, int y, int length = 10, bool centered = false)
        {
            Rectangle rect;
            if (centered)
            {
               rect = new Rectangle(x,y, length, length);
            }
            else
            {
                rect = new Rectangle(x-(length/2), y-(length / 2), length, length);
            }

            return CaptureRegion(rect);
        }

        public static void SaveImage(Image image, string filename = "clickless")
        {
            var saved = new Bitmap(image);
            saved.Save(Path.Combine(_save_path, filename), ImageFormat.Png);
        }
    }
   
}
