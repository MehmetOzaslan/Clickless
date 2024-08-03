using Clickless.src.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Util;



namespace Clickless.src
{
    public class Program
    {
        public const int loopDelay = 1000;

        [STAThread]
        static void Main(string[] args)
        {
            Thread messageThread = new Thread(RunMessageLoop);

            // Start the Windows Forms message loop on a separate thread
            Application.EnableVisualStyles();
            messageThread.SetApartmentState(ApartmentState.STA); // Set the apartment state to STA
            messageThread.Start();

        }

        static void RunMessageLoop()
        {
            var keyboardHook = KeyboardHook.Instance;
            InputHandler keyMatcher = new InputHandler();

            keyboardHook.KeyDown += (sender, e) => {
                var keys = KeyboardHook.Instance.CurrentlyPressedKeys;
                Console.WriteLine("====== KEYS =====");
                foreach (var item in keys)
                {
                    Console.Write($"{item} || ");
                }
                Console.WriteLine();
                Console.WriteLine("=================");
                keyMatcher.ExecuteCommand(keys);
            };
            Application.Run();
        }

    }


}
