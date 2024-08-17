using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Clickless
{
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

    [StructLayout(LayoutKind.Sequential)]
    struct INPUT
    {
        public uint type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ICONINFO
    {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    public partial class MouseUtilities
    {

        private static MouseUtilities instance = null;
        private static IntPtr originalCursor;
        private static readonly object padlock = new object();


        //Constants used in relation to the API.
        const uint OCR_NORMAL = 32512;
        const uint INPUT_MOUSE = 0;
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const Int32 CURSOR_SHOWING = 0x00000001;
        private const Int32 CURSOR_SUPPRESSED = 0x00000002;

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        public static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursorFromFile(string lpFileName);

        [DllImport("user32.dll")]
        static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll")]
        public static extern bool SetSystemCursor(IntPtr hcur, uint id);

        [DllImport("user32.dll")]
        static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(ref CURSORINFO pci);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr CreateIconIndirect(ref ICONINFO iconInfo);
        [DllImport("gdi32.dll", SetLastError = true)]
        static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);

        MouseUtilities() { }

        public static MouseUtilities Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MouseUtilities();
                    }
                    return instance;
                }
            }
        }

        public static void IterateOverLocations(IEnumerable<MathUtilities.Vector2> locations, Action preAction = null, Action postAction = null)
        {
            foreach (var item in locations)
            {
                preAction?.Invoke();
                MoveCursor((int)item.x, (int)item.y);
                postAction?.Invoke();
            }
        }

        public static void IterateOverLocations(IEnumerable<MathUtilities.Vector2> locations, Action<MathUtilities.Vector2> preAction = null, Action<MathUtilities.Vector2> postAction = null, bool parallel = false)
        {

            if (parallel)
            {
                Parallel.ForEach(locations, item =>
                {
                    preAction?.Invoke(item);
                    MoveCursor((int)item.x, (int)item.y);
                    postAction?.Invoke(item);
                });
            }
            else { 
                foreach (var item in locations)
                {
                    preAction?.Invoke(item);
                    MoveCursor((int)item.x, (int)item.y);
                    postAction?.Invoke(item);
                }
            }
        }

        //Move the cursor to the given coordinates.
        public static void MoveCursor(int x, int y)
        {
            SetCursorPos(x, y);
        }

        //Simple factory method.
        static CURSORINFO CreateCursorInfo()
        {
            CURSORINFO cr = new CURSORINFO();
            cr.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
            return cr;
        }

        public static CURSORINFO GetCursorInfo()
        {
            CURSORINFO cr = CreateCursorInfo();

            GetCursorInfo(ref cr);

            return cr;
        }

        public static void HideCursor()
        {
            originalCursor = CopyIcon(LoadCursor(IntPtr.Zero, (int)OCR_NORMAL));
            if (originalCursor == IntPtr.Zero)
            {
                Console.WriteLine("Failed to copy original cursor.");
                return;
            }

            // Load the cursor from file
            IntPtr hCursor = LoadCursorFromFile("C:/Users/mytur/Downloads/transparent.cur");
            if (hCursor == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load cursor from file.");
                return;
            }

            if (!SetSystemCursor(hCursor, OCR_NORMAL))
            {
                Console.WriteLine("Failed to set system cursor.");
                return;
            }
        }

        // Restore the default cursor before exiting
        public static void ShowCursor()
        {
            // Restore the original cursor before exiting
            SetSystemCursor(originalCursor, OCR_NORMAL);
            IntPtr defaultCursor = LoadCursor(IntPtr.Zero, (int)OCR_NORMAL); 
            SetSystemCursor(defaultCursor, OCR_NORMAL);
        }

        public static void ClickAtRectCenter(Rectangle rect)
        {
            var center = GetRectCenter(rect);
            MoveCursor(center.X, center.Y);
            DoMouseClick();
        }

        private static Point GetRectCenter(Rectangle rect)
        {
            int centerX = rect.X + rect.Width / 2;
            int centerY = rect.Y + rect.Height / 2;
            return new Point(centerX, centerY);
        }


        public static void DoMouseClick()
        {
            INPUT[] inputs = new INPUT[2];
            // Mouse left button down
            inputs[0] = new INPUT
            {
                type = INPUT_MOUSE,
                u = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dwFlags = MOUSEEVENTF_LEFTDOWN
                    }
                }
            };

            // Mouse left button up
            inputs[1] = new INPUT
            {
                type = INPUT_MOUSE,
                u = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dwFlags = MOUSEEVENTF_LEFTUP
                    }
                }
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}

