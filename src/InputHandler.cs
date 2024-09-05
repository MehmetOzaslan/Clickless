using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;

namespace Clickless
{

    public class InputHandler
    {
        private RectangleDisplay transparentForm = new RectangleDisplay();
        string pattern_typed = "";
        enum States { FORM_CLOSED, FORM_OPEN, WAITING_SEARCH }
        private States state = States.FORM_CLOSED;
        Dictionary<HashSet<Keys>, Action> Commands;
        public void AddCommand(Keys[] keys, Action action)
        {
            Commands[keys.ToHashSet()] = action; 
        }
        public InputHandler()
        {
            //Initialize _rectEngine, defaulting to the mlclient for now.

            Commands = new Dictionary<HashSet<Keys>, Action>(new KeySetComparer());

            //Search Command (WIN+SHIFT+S)
            AddCommand(new Keys[] { Keys.LWin, Keys.LShiftKey, Keys.F }, async () => {
                if (state == States.FORM_CLOSED)
                {
                    state = States.FORM_OPEN;
                    Console.WriteLine("Creating Window");
                    Task task = RunRectGenerationAndDisplayWindow();
                    await task;
                }
            });

            //Escape, escapes from the transparent form.
            AddCommand(new Keys[] { Keys.Escape }, () => {
                    if (state == States.FORM_OPEN)
                    {
                        Console.WriteLine("Closing Window");
                        CloseWindow();
                        ResetPattern();
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


        int curRecursionCount = GridClient.gridSettings.recursionCount;

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
                    ResetPattern();
                    break;
                case 1:
                    ResetPattern();

                    var chosenRect = rects.First().Rectangle;

                    //TODO: Rip this out. NOT a pattern I want in my codebase.
                    if (ClientHandler.GetEngine() is GridClient)
                    {
                        curRecursionCount -= 1;
                        var GridClient = ClientHandler.GetEngine() as GridClient;

                        if (curRecursionCount <= 0)
                        {
                            //Used to avoid inputting keyboard command, due to it not being captured for some reason.
                            await Task.Delay(20);
                            CloseWindow();
                            MouseUtilities.ClickAtRectCenter(chosenRect);
                            curRecursionCount = GridClient.gridSettings.recursionCount;
                        }
                        else
                        {
                            var recursedRects = await GridClient.GenerateRects(chosenRect);
                            transparentForm.SetRects(recursedRects);
                        }
                    }
                    else
                    {
                        await Task.Delay(20);
                        CloseWindow();
                        MouseUtilities.ClickAtRectCenter(chosenRect);
                        curRecursionCount = GridClient.gridSettings.recursionCount;
                    }

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

        public async Task RunRectGenerationAndDisplayWindow()
        {
            var rectsTask = ClientHandler.GetEngine().GenerateRects();
            transparentForm = new RectangleDisplay();
            transparentForm.Show();
            transparentForm.SetRects(await rectsTask);
        }

        public void ResetPattern()
        {
            pattern_typed = "";
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
        }
    }
}
