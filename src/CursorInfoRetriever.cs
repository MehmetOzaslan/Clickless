﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clickless.src
{

    /// <summary>
    /// Utility class intended to differentiate between cursor types when hovering over the desktop.
    /// </summary>
    public class CursorInfoRetriever
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

        [StructLayout(LayoutKind.Sequential)]
        public struct ICONINFO
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        private static readonly Dictionary<IntPtr, string> SystemCursors = new Dictionary<IntPtr, string>
        {
        { new IntPtr(65567), "Hand" }, //Manual entry, default hand does not work.
        //{ Cursors.Arrow.Handle, "Arrow" }, // Conflicts with another.
        { Cursors.AppStarting.Handle, "AppStarting" },
        { Cursors.Cross.Handle, "Cross" },
        { Cursors.Default.Handle, "Default" },
        { Cursors.Hand.Handle, "Hand" },
        { Cursors.Help.Handle, "Help" },
        { Cursors.HSplit.Handle, "HSplit" },
        { Cursors.IBeam.Handle, "IBeam" },
        { Cursors.No.Handle, "No" },
        { Cursors.NoMove2D.Handle, "NoMove2D" },
        { Cursors.NoMoveHoriz.Handle, "NoMoveHoriz" },
        { Cursors.NoMoveVert.Handle, "NoMoveVert" },
        { Cursors.PanEast.Handle, "PanEast" },
        { Cursors.PanNE.Handle, "PanNE" },
        { Cursors.PanNorth.Handle, "PanNorth" },
        { Cursors.PanNW.Handle, "PanNW" },
        { Cursors.PanSE.Handle, "PanSE" },
        { Cursors.PanSouth.Handle, "PanSouth" },
        { Cursors.PanSW.Handle, "PanSW" },
        { Cursors.PanWest.Handle, "PanWest" },
        { Cursors.SizeAll.Handle, "SizeAll" },
        { Cursors.SizeNESW.Handle, "SizeNESW" },
        { Cursors.SizeNS.Handle, "SizeNS" },
        { Cursors.SizeNWSE.Handle, "SizeNWSE" },
        { Cursors.SizeWE.Handle, "SizeWE" },
        { Cursors.UpArrow.Handle, "UpArrow" },
        { Cursors.VSplit.Handle, "VSplit" },
        { Cursors.WaitCursor.Handle, "WaitCursor" }
    };

        public static string IdentifyCursorType(IntPtr hCursor)
        {
            if (SystemCursors.TryGetValue(hCursor, out string cursorName))
            {
                return cursorName;
            }
            return "Unknown: " + hCursor;
        }
    }
}