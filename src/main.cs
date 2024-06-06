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
            Run();
        }


        static void Run()
        {

            float offset = 0f;

            while (true) {
                MouseController.CURSORINFO info = MouseController.GetCursorInfo();
                var x = info.ptScreenPos.x;
                var y = info.ptScreenPos.y;

                int num_pts = 5;
                int radius = 100;


                MathUtil.Vector2[] points= MathUtil.GenerateCirclePoints(num_pts, radius, offset);

                foreach (var point in points)
                {
                    var move_x = (x + point.x);
                    var move_y = (y + point.y);

                    MouseController.MoveCursor((int)(x +point.x), (int)(y +point.y));
                    Thread.Sleep(loopDelay);
                    
                    var last_movement = MouseController.GetCursorInfo();
                    var delta_x = last_movement.ptScreenPos.x - move_x;
                    var delta_y = last_movement.ptScreenPos.y - move_y;

                    x += (int) delta_x;
                    y += (int) delta_y;
                }
                MouseController.MoveCursor(x, y);


                //Console.WriteLine(y + " " + x + " " + info.hCursor.ToString() + AppDomain.CurrentDomain.BaseDirectory + "  ");

                foreach (var item in ScreenController.ObtainGrid())
                {
                    Console.Write(item.x);
                }

                Thread.Sleep(loopDelay);
            }
        }
    }

}
