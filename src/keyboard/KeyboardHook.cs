using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Clickless
{

    /// <summary>
    /// Event arguments for which key was pressed or released by the user.
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        public Keys key { get; set; }
        public KeyPressedEventArgs(Keys key)
        {
            this.key = key;
        }
    }

    /// <summary>
    /// Provides information on when the user presses or releases a key,
    /// and on which keys are currently pressed.
    /// NOTE: Not foolproof for keyboards without NKRO.
    /// </summary>
    public class KeyboardHook
    {
        //Taken from the following links:
        //https://learn.microsoft.com/en-us/windows/win32/winmsg/about-hooks?redirectedfrom=MSDN
        //https://learn.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644985(v=vs.85)
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int DW_THREAD_ID = 0;

        //https://learn.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644985(v=vs.85)
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static KeyboardHook _instance;
        private static readonly object lockObject = new object(); // Lock object for thread safety


        private Dictionary<Keys, bool> pressedKeys = new Dictionary<Keys, bool>();

        // Primary interface.
        public Keys[] CurrentlyPressedKeys { get { return pressedKeys.Where(x => x.Value == true).Select(x => x.Key).ToArray(); } }
        public event EventHandler<KeyPressedEventArgs> KeyUp;
        public event EventHandler<KeyPressedEventArgs> KeyDown;


        // Windows API wrappers.
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


        /// <summary>
        /// Singleton pattern. 
        /// The hook exists for the duration of the program.
        /// </summary>
        public static KeyboardHook Instance
        {
            get
            {
                lock (lockObject)
                {
                    return _instance ?? (_instance = new KeyboardHook());
                }
            }
            private set
            {
                lock (lockObject)
                {
                    _instance = value;
                }
            }
        }

        //Adds the hook once this class is called.
        private KeyboardHook()
        {
            _hookID = SetHook(_proc);
        }

        //Ensures that the hook is removed after the process dies.
        ~KeyboardHook()
        {
            UnhookWindowsHookEx(_hookID);
        }

        /// <summary>
        /// When the key is released, this invokes the KeyUp event with the key that was released,
        /// and updates the currently pressed key dictionary.
        /// </summary>
        /// <param name="e">An arguement containing the keyboard value</param>
        static protected void OnKeyUp(KeyPressedEventArgs e)
        {
            lock (lockObject)
            {
                Instance.pressedKeys[e.key] = false;
                Instance.KeyUp?.Invoke(Instance, e);
            }

        }

        /// <summary>
        /// When the key is pressed, this invokes the KeyDown event with the key that was released,
        /// and updates the currently pressed key dictionary.
        /// </summary>
        /// <param name="e">An arguement containing the keyboard value</param>
        static protected void OnKeyDown(KeyPressedEventArgs e)
        {
            lock (lockObject)
            {
                Instance.pressedKeys[e.key] = true;
                Instance.KeyDown?.Invoke(Instance, e);
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), DW_THREAD_ID);
            }
        }


        /// <summary>
        /// Callback from the WH_KEYBOARD_LL hook.
        /// Information taken from Microsoft's LowLevelKeyboardProc page. 
        /// </summary>
        /// <param name="nCode"> 
        /// A code the hook procedure uses to determine how to process the message.
        /// If nCode is less than zero, the hook procedure must pass the message to the CallNextHookEx function without further processing
        /// and should return the value returned by CallNextHookEx.
        /// </param>
        /// <param name="wParam">
        /// The keydown or key up commands.
        /// </param> 
        /// <param name="lParam">
        /// Contains the virtual key code, contains data on the key pressed.
        /// See: KBDLLHOOKSTRUCT:  https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-kbdllhookstruct</param>
        /// <returns> Call to the next hook. </returns>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                if(wParam == (IntPtr)WM_KEYDOWN)
                {
                    OnKeyDown(new KeyPressedEventArgs(key));
                }

                if(wParam == (IntPtr)WM_KEYUP)
                {
                    OnKeyUp(new KeyPressedEventArgs(key));
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }

}
