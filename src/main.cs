using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Clickless.src
{
    public class Program
    {
        public const int loopDelay = 10;

        static void Main(string[] args)
        {
            Console.WriteLine(args.Length);
            MouseController.MoveCursor(1, 1);

            Run();
        }


        static void Run()
        {
            while (true) {


                MouseController.CURSORINFO info = MouseController.CursorInfo();
                var x = info.ptScreenPos.x;
                var y = info.ptScreenPos.y;
                
                Console.WriteLine(y + " " + x + " " + info.hCursor.ToString());



                Thread.Sleep(loopDelay);
            }
        }
    }

}
