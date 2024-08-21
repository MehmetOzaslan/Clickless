using System;
using System.Threading;
using System.Windows.Forms;

namespace Clickless
{
    public class Program
    {

        [STAThread]
        public static void Run()
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
