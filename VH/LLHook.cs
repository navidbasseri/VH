using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace VH
{
    public static class LLHook
    {
        public delegate void EventHandler(Event event_);
        public static event EventHandler EventTrigger;
        const Int32 CURSOR_SHOWING = 0x00000001;
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;

        private static IntPtr _KeyboardhookID = IntPtr.Zero;
        private static IntPtr _MousehookID = IntPtr.Zero;

        static LowLevelMouseProc MouseHookGCRootedDelegate = MouseHookCallback;
        static LowLevelKeyboardProc KeyboardHookGCRootedDelegate = KeyboardHookCallback;

        private static bool MouseLock = false;
        private static bool KeyboardLock = false;
        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        public enum KeyboardState
        {
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101,
            WM_CHAR = 0x0102,
            WM_SYSKEYDOWN = 0x0104,
            WM_SYSKEYUP = 0x0105,
            NA = 0,
        }

        public enum MouseState : uint
        {
            WM_MOUSEMOVE = 0x0200,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_RBUTTONDBLCLK = 0x0206,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
            WM_MBUTTONDBLCLK = 0x0209,

            WM_MOUSEWHEEL = 0x020A,
            WM_MOUSEHWHEEL = 0x020E,

            WM_NCMOUSEMOVE = 0x00A0,
            WM_NCLBUTTONDOWN = 0x00A1,
            WM_NCLBUTTONUP = 0x00A2,
            WM_NCLBUTTONDBLCLK = 0x00A3,
            WM_NCRBUTTONDOWN = 0x00A4,
            WM_NCRBUTTONUP = 0x00A5,
            WM_NCRBUTTONDBLCLK = 0x00A6,
            WM_NCMBUTTONDOWN = 0x00A7,
            WM_NCMBUTTONUP = 0x00A8,
            WM_NCMBUTTONDBLCLK = 0x00A9
        }

        public enum MouseEvents : uint
        {
            WM_MOUSEMOVE = 0x0001,
            WM_LBUTTONDOWN = 0x0002,
            WM_LBUTTONUP = 0x0004,
            WM_RBUTTONDOWN = 0x0008,
            WM_RBUTTONUP = 0x0010,
            WM_MBUTTONDOWN = 0x0020,
            WM_MBUTTONUP = 0x0040,
            WM_MOUSEWHEEL = 0x0800,
            WM_MOUSEHWHEEL = 0x01000,
            WM_ABSOLUTE = 0x8000,
            WM_XDOWN = 0x0080,
            WM_XUP = 0x0100,
        }


        public enum KeyboardEvents : int
        {
            WM_KEYDOWN = 0x0000,
            WM_EXTENDEDKEY = 0x0001,
            WM_KEYUP = 0x0002,
            WM_UNICODE = 0x0004,
            WM_SCANCODE =  0x0008,
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public int vkCode; // Specifies a virtual-key code
            public int scanCode; // Specifies a hardware scan code for the key
            public int flags;
            public int time; // Specifies the time stamp for this message
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt; // The x and y coordinates in screen coordinates. 
            public int mouseData; // The mouse wheel and button info.
            public int flags;
            public int time; // Specifies the time stamp for this message. 
            public int dwExtraInfo;
        }


        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);



        public static IntPtr SetMouseHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx((int)HookType.WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public static IntPtr SetKeyboardHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx((int)HookType.WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        [Serializable]
        public abstract class Event
        {
            public HookType HookType;
            public Int64 state;
            public int nCode;
            public DateTime time;
            public bool capslock;
            public bool shift;
        }

        [Serializable]
        public class KeyboardEvent : Event, IDisposable
        {
            public KBDLLHOOKSTRUCT kbdllhookstruct;
            public void Dispose()
            {
            }
        }

        [Serializable]
        public class MouseEvent : Event, IDisposable
        {
            public MSLLHOOKSTRUCT msllhookstruct;
            public void Dispose()
            {
            }
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        public static extern int MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int MapVirtualKey(int uCode, uint uMapType);

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);[DllImport("user32.dll", CharSet = CharSet.Unicode)]

        public static extern int ToUnicode(
            uint virtualKeyCode,
            uint scanCode,
            byte[] keyboardState,
            StringBuilder receivingBuffer,
            int bufferSize,
            uint flags
        );

        public static string GetCharsFromKeys(Keys keys, bool shift)
        {
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            if (shift)
            {
                keyboardState[(int)Keys.ShiftKey] = 0xff;
            }
            ToUnicode((uint)keys, 0, keyboardState, buf, 256, 0);
            return buf.ToString();
        }

        public static void InstallHook()
        {
            _KeyboardhookID = SetKeyboardHook(KeyboardHookGCRootedDelegate);
            _MousehookID = SetMouseHook(MouseHookGCRootedDelegate);
        }
        public static bool UninstallHook()
        {
            UnhookWindowsHookEx(_KeyboardhookID);
            UnhookWindowsHookEx(_MousehookID);
            return true;
        }

        private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                using (MouseEvent mouseevent = new MouseEvent())
                {
                    mouseevent.HookType = HookType.WH_MOUSE_LL;
                    mouseevent.state = wParam.ToInt64();
                    mouseevent.nCode = nCode;
                    mouseevent.msllhookstruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    mouseevent.time = DateTime.Now;
                    mouseevent.capslock = (((ushort)GetKeyState(0x14)) & 0xffff) != 0;
                    mouseevent.shift = (((ushort)GetKeyState(0x10)) & 0x1000) != 0;
                    EventTrigger(mouseevent);
                }
                if (MouseLock)
                    return new IntPtr(1);
            }


            return CallNextHookEx(_MousehookID, nCode, wParam, lParam);
        }


        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                using (KeyboardEvent keyboardevent = new KeyboardEvent())
                {
                    keyboardevent.HookType = HookType.WH_KEYBOARD_LL;
                    keyboardevent.state = wParam.ToInt64();
                    keyboardevent.nCode = nCode;
                    keyboardevent.kbdllhookstruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                    keyboardevent.time = DateTime.Now;
                    keyboardevent.capslock = (((ushort)GetKeyState(0x14)) & 0xffff) != 0;
                    keyboardevent.shift = (((ushort)GetKeyState(0x10)) & 0x1000) != 0;
                    EventTrigger(keyboardevent);
                }
                if (KeyboardLock)
                    return new IntPtr(1);
            }

            return CallNextHookEx(_KeyboardhookID, nCode, wParam, lParam);
        }


        public static bool GetCursorPos(out CURSORINFO pci)
        {
            pci.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CURSORINFO));

            if (GetCursorInfo(out pci) && pci.flags == CURSOR_SHOWING)
                return true;

            return false;
        }

        public static POINT GetCursorPos()
        {
            CURSORINFO pci = new CURSORINFO();
            pci.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CURSORINFO));

            if (GetCursorInfo(out pci) && pci.flags == CURSOR_SHOWING)
                return pci.ptScreenPos;

            return new POINT(0, 0);
        }


        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int X, int Y);

        public static bool MoveMouse(POINT point, int steps = 100)
        {
            POINT mpoint = new POINT();
            int acc = 0;
            GetCursorPos(out mpoint);
            if (mpoint.Equals(point))
                return true;
            while (mpoint.x != point.x && mpoint.y != point.y)
            {
                acc++;
                steps--;
                if (steps == 0)
                {
                    SetCursorPos(point.x, point.y);
                    return true;
                }
                else
                {
                    mpoint.x = mpoint.x + ((point.x - mpoint.x) * acc / steps);
                    mpoint.y = mpoint.y + ((point.y - mpoint.y) * acc / steps);
                    SetCursorPos(mpoint.x, mpoint.y);
                }
                Thread.Sleep(10);
            }

            return true;
        }

        public static void run_event(in MouseEvent me)
        {
            uint message = 0;
            switch ((MouseState)me.state)
            {
                case MouseState.WM_LBUTTONDOWN:
                case MouseState.WM_NCLBUTTONDOWN:
                    message = (uint)(LLHook.MouseEvents.WM_LBUTTONDOWN);
                    break;
                case MouseState.WM_LBUTTONUP:
                case MouseState.WM_NCLBUTTONUP:
                    message = (uint)(LLHook.MouseEvents.WM_LBUTTONUP);
                    break;
                case MouseState.WM_LBUTTONDBLCLK:
                case MouseState.WM_NCLBUTTONDBLCLK:
                    MouseEvent extra = new MouseEvent();
                    for (int i = 0; i < 2; i++)
                    {
                        extra = me;
                        extra.state = (long)MouseState.WM_LBUTTONDOWN;
                        run_event(extra);
                        extra.state = (long)MouseState.WM_LBUTTONUP;
                        run_event(extra);
                    }
                    break;

                case MouseState.WM_RBUTTONDOWN:
                case MouseState.WM_NCRBUTTONDOWN:
                    message = (uint)(LLHook.MouseEvents.WM_RBUTTONDOWN);
                    break;
                case MouseState.WM_RBUTTONUP:
                case MouseState.WM_NCRBUTTONUP:
                    message = (uint)(LLHook.MouseEvents.WM_RBUTTONUP);
                    break;

                case MouseState.WM_RBUTTONDBLCLK:
                case MouseState.WM_NCRBUTTONDBLCLK:
                    extra = new MouseEvent();
                    extra = me;
                    for (int i = 0; i < 2; i++)
                    {
                        extra.state = (long)MouseState.WM_RBUTTONDOWN;
                        run_event(extra);
                        extra.state = (long)MouseState.WM_RBUTTONUP;
                        run_event(extra);
                    }
                    break;

                case MouseState.WM_MBUTTONDOWN:
                case MouseState.WM_NCMBUTTONDOWN:
                    message = (uint)(LLHook.MouseEvents.WM_MBUTTONDOWN);
                    break;
                case MouseState.WM_MBUTTONUP:
                case MouseState.WM_NCMBUTTONUP:
                    message = (uint)(LLHook.MouseEvents.WM_MBUTTONUP);
                    break;

                case MouseState.WM_MBUTTONDBLCLK:
                case MouseState.WM_NCMBUTTONDBLCLK:
                    extra = new MouseEvent();
                    for (int i = 0; i < 2; i++)
                    {
                        extra = me;
                        extra.state = (long)MouseState.WM_MBUTTONDOWN;
                        run_event(extra);
                        extra.state = (long)MouseState.WM_MBUTTONUP;
                        run_event(extra);
                    }
                    break;

                case MouseState.WM_MOUSEWHEEL:
                    message = (uint)(LLHook.MouseEvents.WM_MOUSEWHEEL);
                    break;

                case MouseState.WM_MOUSEHWHEEL:
                    message = (uint)(LLHook.MouseEvents.WM_MOUSEHWHEEL);
                    break;
            }

            mouse_event(message, (uint)me.msllhookstruct.pt.x, (uint)me.msllhookstruct.pt.y, 0, 0);
        }


        public static void LockHook(double ms, bool mouse = true, bool keyboad = true)
        {
            Task locking = Task.Run(() =>
            {
                if (mouse) MouseLock = true;
                if (keyboad) KeyboardLock = true;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                while (stopwatch.ElapsedMilliseconds<ms)
                {
                   Application.DoEvents();
                }
                stopwatch.Stop();
                if (mouse) MouseLock = false;
                if (keyboad) KeyboardLock = false;
            });
        }

        public static void UnlockHook(bool mouse = true, bool keyboad = true)
        {
            if (mouse) MouseLock = false;
            if (keyboad) KeyboardLock = false;
        }


        

        public static void run_event(in KeyboardEvent ke)
        {
            
            int flags = ke.kbdllhookstruct.flags;
            switch ((KeyboardState)ke.state)
            {
                case KeyboardState.WM_KEYDOWN:
                case KeyboardState.WM_SYSKEYDOWN:
                    flags = (int)LLHook.KeyboardEvents.WM_KEYDOWN;
                    break;
                case KeyboardState.WM_KEYUP:
                case KeyboardState.WM_SYSKEYUP:
                    flags |= (int)LLHook.KeyboardEvents.WM_KEYUP;
                    keybd_event((byte)ke.kbdllhookstruct.vkCode, (byte)ke.kbdllhookstruct.scanCode, ke.kbdllhookstruct.flags, ke.kbdllhookstruct.dwExtraInfo);
                    break;
            }


            //TODO : execute keyboard events
/*
//                uint scanCode =(uint) ke.kbdllhookstruct.scanCode;
                uint scanCode =(uint) MapVirtualKey((uint)ke.kbdllhookstruct.vkCode, 0);
                uint lParam;
                //KEY DOWN
                lParam = (0x00000001 | (scanCode << 16));
                if (true)
                {
                    lParam |= 0x01000000;
                }
                PostMessage(GetForegroundWindow(), (uint)KeyboardState.WM_KEYDOWN, (IntPtr)(ke.kbdllhookstruct.vkCode),new IntPtr(lParam));

                //KEY UP
                lParam |= 0xC0000000;  // set previous key and transition states (bits 30 and 31)
                PostMessage(GetForegroundWindow(), (uint)KeyboardState.WM_KEYUP, (IntPtr)(ke.kbdllhookstruct.vkCode), new IntPtr(lParam));
*/
            //const int key = (int)Keys.A;

            //PostMessage(GetForegroundWindow(), (uint) KeyboardState.WM_KEYDOWN, (IntPtr)(key - 0x020), IntPtr.Zero);
            //SendMessage(GetForegroundWindow(), (uint) KeyboardState.WM_KEYDOWN, (IntPtr)(key - 0x020), IntPtr.Zero);
            //PostMessage(GetForegroundWindow(), (uint) KeyboardState.WM_CHAR, (IntPtr)key, IntPtr.Zero);
            //SendMessage(GetForegroundWindow(), (uint) KeyboardState.WM_CHAR, (IntPtr)key, IntPtr.Zero);
            //PostMessage(GetForegroundWindow(), (uint) KeyboardState.WM_KEYUP, (IntPtr)(key - 0x020), IntPtr.Zero);

            Thread.Sleep(50);
        }

    }
}