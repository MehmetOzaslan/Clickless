using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace Clickless
{
    public static class MouseController
    {
        [DllImport("user32.dll")]
        public static extern int SetCursorPos(int x, int y);

        //Move the cursor to the given coordinates.
        public static void MoveCursor(int x, int y)
        {
            SetCursorPos(x, y);
        }
    }
}
