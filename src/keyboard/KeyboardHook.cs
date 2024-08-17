using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Clickless
{
    public class KeyboardHook
    {
        private static readonly object lockObject = new object(); // Lock object for thread safety

        private static KeyboardHook _instance;
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

        private KeyboardHook()
        {
            //initialize the hook.
            _hookID = SetHook(_proc);
        }

        public void Unhook()
        {
            UnhookWindowsHookEx(_hookID);
        }

        ~KeyboardHook()
        {
            Unhook();
        }

        //Taken from the following links:
        //https://learn.microsoft.com/en-us/windows/win32/winmsg/about-hooks?redirectedfrom=MSDN
        //https://learn.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644985(v=vs.85)
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private Dictionary<Keys, bool> pressedKeys = new Dictionary<Keys, bool>();

        public Keys[] CurrentlyPressedKeys { get {
                var keys = pressedKeys.Where(x => x.Value == true).Select(x=> x.Key).ToArray();
                return keys;
            } }


        public event EventHandler<KeyPressedEventArgs> KeyUp;
        public event EventHandler<KeyPressedEventArgs> KeyDown;
        public event EventHandler<KeyPressedEventArgs> KeyPress;

        public class KeyPressedEventArgs : EventArgs
        {
            public Keys key { get; set; }
            public KeyPressedEventArgs(Keys key)
            {
                this.key = key;
            }
        }

        static protected void OnKeyUp(KeyPressedEventArgs e)
        {
            lock (lockObject)
            {
                Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId);
                Instance.KeyUp?.Invoke(Instance, e);
            }            

        }

        static protected void OnKeyDown(KeyPressedEventArgs e)
        {
            lock (lockObject)
            {
                Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId);
                Instance.KeyDown?.Invoke(Instance, e);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        //https://learn.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644985(v=vs.85)
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                if(wParam == (IntPtr)WM_KEYDOWN)
                {
                    Instance.pressedKeys[key] = true;
                    OnKeyDown(new KeyPressedEventArgs(key));
                }

                if(wParam == (IntPtr)WM_KEYUP)
                {
                    Instance.pressedKeys[key] = false;
                    OnKeyUp(new KeyPressedEventArgs(key));
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }

}
