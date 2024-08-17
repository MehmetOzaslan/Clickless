using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Clickless
{
    /// <summary>
    /// Based on the following:
    /// http://pinvoke.net/default.aspx/user32.GetCursorInfo
    /// </summary>
    /// 
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public Int32 x;
        public Int32 y;

        public POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // Helper cast method.
        public static explicit operator Point(POINT p)
        {
            return new Point(p.x, p.y);
        }

        // Helper cast method.
        public static explicit operator POINT(Point p)
        {
            return new POINT(p.X, p.Y);
        }

    }
}
