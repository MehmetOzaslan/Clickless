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
            Application.EnableVisualStyles();

            var keyboardHook = KeyboardHook.Instance;

            // Start the Windows Forms message loop on a separate thread
            Thread uiThread = new Thread(RunMessageLoop);
            uiThread.SetApartmentState(ApartmentState.STA); // Set the apartment state to STA
            uiThread.Start();

            float offset = 0f;
            var websocket = new MLClientWebsocket();

            Task task = websocket.ConnectAsync(new Uri(MLClientWebsocket.imagemlURL));
            task.Wait();

            Assert.AreEqual(WebSocketState.Open, websocket.Client.State);

            KeyMatcher keyMatcher = new KeyMatcher(websocket);
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
        }

        static void RunMessageLoop()
        {
            
            
            Application.Run();
        }

    }


}
