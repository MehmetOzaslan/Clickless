using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Clickless.src
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
