using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Clickless.MouseController;

namespace Clickless.src
{
    /// <summary>
    /// Obtains information of windows.
    /// </summary>
    internal class WindowInfoRetriever
    {
        private const int _windowCharLength = 256;

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        public static string GetWindowTextAtPoint(POINT point)
        {
            StringBuilder lpString = new StringBuilder(_windowCharLength);
            IntPtr hWnd = WindowFromPoint(point);
            GetWindowText(hWnd, lpString, lpString.Capacity);
            return lpString.ToString();
        }

        public static RECT GetWindowRectAtPoint(POINT point)
        {
            RECT lpRect;
            IntPtr hWnd = WindowFromPoint(point);
            GetWindowRect(hWnd, out lpRect);
            return lpRect;
        }
    }
}
