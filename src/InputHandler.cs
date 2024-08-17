using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clickless
{
    public class InputHandler
    {
        private MLClientWebsocket websocket;
        private TransparentForm transparentForm = new TransparentForm();
        string pattern_typed = "";

        enum States { FORM_CLOSED, FORM_OPEN, WAITING_SEARCH }

        private States state = States.FORM_CLOSED;

        Dictionary<HashSet<Keys>, Action> Commands;

        public MLClientWebsocket Websocket { get => websocket; set => websocket = value; }

        public void AddCommand(Keys[] keys, Action action)
        {
            Commands[keys.ToHashSet()] = action; 
        }
        public InputHandler()
        {
            Commands = new Dictionary<HashSet<Keys>, Action>(new KeySetComparer());

            //Search Command (WIN+SHIFT+S)
            AddCommand(new Keys[] { Keys.LWin, Keys.LShiftKey, Keys.F }, async () => {
                if (state == States.FORM_CLOSED)
                {
                    state = States.FORM_OPEN;
                    Console.WriteLine("Creating Window");
                    Task task = RunMLandDisplayWindow();
                    await task;
                }
            });

            //Escape, escapes from the transparent form.
            AddCommand(new Keys[] { Keys.Escape }, () => {
                    if (state == States.FORM_OPEN)
                    {
                        Console.WriteLine("Closing Window");
                        pattern_typed = "";
                        CloseWindow();
                        state = States.FORM_CLOSED;
                    }
                });
        }

        private void NoMatch(HashSet<Keys> keys)
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

                UpdateRects();
            }
            return;
        }

        private async void UpdateRects()
        {
            //Update the rects on the display
            var rects = transparentForm.Rects;
            rects = rects.Where(x => x.Text.StartsWith(pattern_typed)).ToList();

            //Handle rect logic.
            switch (rects.Count)
            {
                case 0:
                    CloseWindow();
                    break;
                case 1:
                    
                    //Used to avoid inputting keyboard command, due to it not being captured for some reason.
                    await Task.Delay(20);
                    CloseWindow();
                    ClickRect(rects.First().Rectangle);
                    break;
                default:
                    transparentForm.Rects = rects;
                    break;
            }
        }

        public void ExecuteCommand(IEnumerable<Keys> keys)
        {
            var keyset = new HashSet<Keys>(keys);
            if (Commands.ContainsKey(keyset))
            {
                Commands[keyset].Invoke();
            }
            else {
                NoMatch(keyset);
            }
        }

        private async Task<List<TextRect>> RunML()
        {
            Bitmap img = MonitorUtilities.CaptureDesktopBitmap();
            var bboxes = await Task.Run(() => MLClient.GetBboxes(img));
            var rects = TextRectGenerator.GenerateBoxesFromRects(bboxes);
            img.Dispose();
            return rects;
        }

        public async Task RunMLandDisplayWindow()
        {
            var rectsTask = RunML();

            transparentForm = new TransparentForm();
            transparentForm.Show();

            var rects = await rectsTask;
            transparentForm.Rects = rects;
            state = States.FORM_OPEN;
        }

        public void CloseWindow()
        {
            if (transparentForm != null && transparentForm.IsHandleCreated)
            {
                transparentForm.Invoke((MethodInvoker)delegate {
                    transparentForm.Close();
                });
                state = States.FORM_CLOSED;
            }
            pattern_typed = "";
        }

        public void ClickRect(Rectangle rect_selected)
        {
            if(rect_selected != null)
            {
                MouseUtilities.ClickAtRectCenter(rect_selected);
            }
        }
    }
}
