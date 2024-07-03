using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Clickless.MouseController;
using static Util.MathUtil;

namespace Clickless.src
{

    /// <summary>
    /// Factory style class. Converts tracked positions into an image for sending to the ML backend.
    /// </summary>
    public class CursorImageGenerator
    {

        private int _width = 0, _height = 0;
        private int rect_width = 10;
        private int rect_height = 10;

        private Dictionary<CURSORINFO, Color> _colorMapping;
        private CursorStateTracker _tracker;


        /// <summary>
        /// Initializes colors and determines the resulting image size.
        /// </summary>
        /// <param name="tracker"></param>
        public CursorImageGenerator(CursorStateTracker tracker)
        {
            _tracker = tracker;

            _colorMapping = new Dictionary<CURSORINFO, Color>();

            foreach (var item in tracker.GetPositionStates())
            {
                var position = item.Key;
                var type = item.Value;

                //Colormap
                if (!_colorMapping.ContainsKey(type))
                {
                    _colorMapping.Add(type, ObjToColor(type));
                }

                //Sets the image size to the extents
                if (_width < position.x)
                {
                    _width = (int)position.x;
                }
                if (_height < position.y)
                {
                    _height = (int)position.y;
                }
            }
        }


        public void CreateImage(string filepath)
        {

            // Create a new bitmap image
            using (Bitmap bitmap = new Bitmap(_width, _height))
            {
                // Use a graphics object to draw on the bitmap
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    // Clear the canvas with a background color
                    graphics.Clear(Color.White);

                    foreach (var entry in _tracker.GetPositionStates())
                    {
                        Vector2 position = entry.Key;
                        Color color = _colorMapping[entry.Value];
                        // Draws a rect centered on the mouse location
                        graphics.FillRectangle(new SolidBrush(color),
                            position.x - rect_height / 2, position.y - rect_height / 2,
                            rect_width, rect_height);
                    }
                }

                // Save the image to the specified file path
                bitmap.Save(filepath);
            }
        }

        /// <summary>
        /// Hashes the bytes in an object to a color. Ideally pass the value directly here
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Color ObjToColor(object obj)
        {
            byte[] objectBytes = ObjectToByteArray(obj);

            //Hashes the bytes
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(objectBytes);

                int r = hashBytes[0];
                int g = hashBytes[1];
                int b = hashBytes[2];

                return Color.FromArgb(r, g, b);
            }
        }

        public static Color ObjToColor(CURSORINFO obj)
        {

            var cursorobj = (int)obj.hCursor;

            byte[] objectBytes = ObjectToByteArray(cursorobj);

            //Hashes the bytes
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(objectBytes);

                int r = hashBytes[0];
                int g = hashBytes[1];
                int b = hashBytes[2];

                return Color.FromArgb(r, g, b);
            }
        }


        /// <summary>
        /// Obtains a byte array of an object using a binary formatter.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

    }
}
