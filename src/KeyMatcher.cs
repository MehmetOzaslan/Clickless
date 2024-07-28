using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clickless.src
{
    /// <summary>
    /// Ensures that the dictionary is consistent whenever we put in a hashset. 
    /// </summary>
    public class KeySetComparer : IEqualityComparer<HashSet<Keys>>
    {
        public bool Equals(HashSet<Keys> x, HashSet<Keys> y)
        {
            return x.SetEquals(y);
        }

        public int GetHashCode(HashSet<Keys> obj)
        {
            int hash = 19;
            foreach (var key in obj.OrderBy(k => k))
            {
                hash = hash * 31 + key.GetHashCode();
            }
            return hash;
        }
    }



    public class KeyMatcher
    {
        private MLClientWebsocket websocket;
        private TransparentForm transparentForm;
        string pattern_typed = "";


        //TODO: Convert to proper state machine
        enum States { FORM_CLOSED, FORM_OPEN, WAITING_SEARCH }

        private States state = States.FORM_CLOSED;

        Dictionary<HashSet<Keys>, Action> Commands;

        public KeyMatcher(MLClientWebsocket conn)
        {
            this.websocket = conn;

            Commands = new Dictionary<HashSet<Keys>, Action>(new KeySetComparer())
            {
                {
                    new HashSet<Keys> { Keys.LWin, Keys.LShiftKey, Keys.F },
                    () => {
                        if(state == States.FORM_CLOSED) {                        
                            state=States.FORM_OPEN;
                            Console.WriteLine("Creating Window");
                            Task task = RunMLandDisplayWindow();
                        }
                    }
                },
                {
                    new HashSet<Keys> { Keys.Enter},
                    () => {
                        if(state == States.FORM_OPEN) {
                            Console.WriteLine("Clicking Mouse" +
                                "");
                            InputUserCommand();
                        }
                    }
                },
                {
                    new HashSet<Keys> { Keys.Escape},
                    () => {
                        if(state == States.FORM_OPEN)
                        {
                            Console.WriteLine("Closing Window");
                            pattern_typed = "";
                            CloseWindow();
                            state = States.FORM_CLOSED;
                        }
                    }
                }
            };
        }

        private void noMatch(HashSet<Keys> keys)
        {
            Console.WriteLine("No match");

            if(state == States.FORM_OPEN)
            {
                //Add the keys to the string.
                var key_strs = keys.Select(x => x.ToString());

                foreach (var item in key_strs)
                {
                    pattern_typed += item;
                }
                Console.Write(pattern_typed);

                //Update the rects on the display.
                var rects = transparentForm.Rects;
                rects = rects.Where(x => x.Text.StartsWith(pattern_typed)).ToList();

                if(rects.Count == 0)
                {
                    pattern_typed = "";
                    CloseWindow();
                    state = States.FORM_CLOSED;
                }
                else
                {
                    transparentForm.Rects = rects;
                }
            }
            return;
        }

        public void ExecuteCommand(Keys[] keys)
        {
                Console.WriteLine("CURR_STATE: " + state.ToString());
                ExecuteCommand(new HashSet<Keys>(keys));
        }

        public void ExecuteCommand(HashSet<Keys> keys)
        {
            if (Commands.ContainsKey(keys)) {
                Commands[keys].Invoke();
            }
            else
            {
                noMatch(keys);
            }
        }

        public async Task RunMLandDisplayWindow()
        {
            state = States.FORM_CLOSED;
            Console.WriteLine($"Sending image");

            if (websocket == null)
            {
                websocket = new MLClientWebsocket();
                await websocket.Abort();
                await websocket.ConnectAsync(new Uri(MLClientWebsocket.imagemlURL));
            }

            if ( websocket.Client.State != WebSocketState.Open)
            {
                Console.WriteLine($"Reconnecting");
                await websocket.Abort();
                await websocket.ConnectAsync(new Uri(MLClientWebsocket.imagemlURL));
            }

            Image img = ScreenController.CaptureDesktop();

            await websocket.SendImageAsync(img);
            string result = await websocket.ReceiveMessageAsync();
            Assert.Greater(result.Length, 1);

            Console.WriteLine("Rcvd " + result);

            var rects = TextRectGenerator.GenerateBoxesFromResponse(result);

            Console.WriteLine("Creating window.");
            transparentForm = new TransparentForm();
            transparentForm.Rects = rects;
            state = States.FORM_OPEN;
            transparentForm.ShowDialog();
        }

        public void CloseWindow()
        {
            transparentForm.Invoke((MethodInvoker)delegate {
                transparentForm.Close();
            });
            state = States.FORM_CLOSED;
        }

        public void InputUserCommand()
        {
            var rect_selected = transparentForm.Rects.FirstOrDefault(x => x.Text == this.pattern_typed);
            if(rect_selected != null)
            {
                var center = GetRectCenter(rect_selected.Rectangle);
                CloseWindow();
                MouseController.MoveCursor(center.X, center.Y);
                MouseController.DoMouseClick();
            }
            pattern_typed = "";
        }

        private Point GetRectCenter(Rectangle rect)
        {
            int centerX = rect.X + rect.Width / 2;
            int centerY = rect.Y + rect.Height / 2;
            return new Point(centerX, centerY);
        }
    }
}
