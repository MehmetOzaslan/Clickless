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

        const int NORMAL_CURSOR = 665539;

        [DllImport("user32.dll")]
        public static extern int SetCursorPos(int x, int y);

        //Move the cursor to the given coordinates.
        public static void MoveCursor(int x, int y)
        {
            SetCursorPos(x, y);
        }


        /// <summary>
        /// http://pinvoke.net/default.aspx/user32.GetCursorInfo
        /// </summary>
        /// 
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public Int32 x;
            public Int32 y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public Int32 cbSize;        // Specifies the size, in bytes, of the structure.
                                        // The caller must set this to Marshal.SizeOf(typeof(CURSORINFO)).
            public Int32 flags;         // Specifies the cursor state. This parameter can be one of the following values:
                                        //    0             The cursor is hidden.
                                        //    CURSOR_SHOWING    The cursor is showing.
                                        //    CURSOR_SUPPRESSED    (Windows 8 and above.) The cursor is suppressed. This flag indicates that the system is not drawing the cursor because the user is providing input through touch or pen instead of the mouse.
            public IntPtr hCursor;          // Handle to the cursor.
            public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor.
        }


        //Simple factory method.
        static CURSORINFO CreateCursorInfo()
        {
            CURSORINFO cr = new CURSORINFO();
            cr.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
            return cr;
        }

        /// <summary>Must initialize cbSize</summary>
        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(ref CURSORINFO pci);


        public static CURSORINFO GetCursorInfo()
        {
            CURSORINFO cr = CreateCursorInfo();

            GetCursorInfo(ref cr);

            return cr;
        }

        private const Int32 CURSOR_SHOWING = 0x00000001;
        private const Int32 CURSOR_SUPPRESSED = 0x00000002;
    }
}
