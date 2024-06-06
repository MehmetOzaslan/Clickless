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
using Util;
using static Clickless.MouseController;

namespace Clickless.src
{


    public static class ScreenController
    {
        public static Size GetSize() { return SystemInformation.VirtualScreen.Size; }
        public static int GetLeft() { return SystemInformation.VirtualScreen.Left; }
        public static int GetTop() { return SystemInformation.VirtualScreen.Top; }


        /// <summary>
        /// Provides a list of points across the screen in a grid pattern. 
        /// TODO: This can be cached.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static List<MathUtil.Vector2> ObtainGrid(int margins=30, int spacing = 30)
        {
            int width = SystemInformation.VirtualScreen.Width;
            int height = SystemInformation.VirtualScreen.Height;
            float numRow = (width - margins * 2) / spacing;
            float numCol = (height - margins * 2) / spacing;
            List<MathUtil.Vector2> outList = new List<MathUtil.Vector2>();

            for (float i = 0; i <= numRow; i+= 1f)
            {
                for (float j = 0; j <= numCol; j+=1f)
                {
                    float x = i / numRow * spacing + margins;
                    float y = j / numRow * spacing + margins;

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
   
}
