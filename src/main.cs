using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Util;



namespace Clickless.src
{
    public class Program
    {
        public const int loopDelay = 1000;

        static void Main(string[] args)
        {
            MouseController.HideCursor();
            Console.WriteLine("Mouse cursor hidden. Press Enter to restore the cursor.");
            Console.ReadLine();
            MouseController.ShowCursor();
            Run();
        }

        static void Run()
        {

            float offset = 0f;

            while (true) {
                MouseController.CURSORINFO info = MouseController.GetCursorInfo();
                var x = info.ptScreenPos.x;
                var y = info.ptScreenPos.y;

                Console.WriteLine(y + " " + x + " " + info.hCursor.ToString() + AppDomain.CurrentDomain.BaseDirectory + "  ");

                Thread.Sleep(loopDelay);
            }
        }
    }

}
