using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TPie.Helpers
{
    public class KeyboardHelper
    {
        #region singleton
        private KeyboardHelper()
        {
            _keyStates = new bool[256];

            ulong processId = (ulong)Process.GetCurrentProcess().Id;

            IntPtr hWnd = IntPtr.Zero;
            do
            {
                hWnd = FindWindowExW(IntPtr.Zero, hWnd, "FFXIVGAME", null);
                if (hWnd == IntPtr.Zero) { return; }

                ulong wndProcessId = 0;
                GetWindowThreadProcessId(hWnd, ref wndProcessId);

                if (wndProcessId == processId)
                {
                    break;
                }

            } while (hWnd != IntPtr.Zero);

            if (hWnd != IntPtr.Zero)
            {
                _wndProcDelegate = WndProcDetour;
                _wndProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
                _imguiWndProcPtr = SetWindowLongPtr(hWnd, GWL_WNDPROC, _wndProcPtr);
            }
        }

        public static void Initialize() { Instance = new KeyboardHelper(); }

        public static KeyboardHelper Instance { get; private set; } = null!;

        ~KeyboardHelper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            // give imgui the control of inputs again
            IntPtr windowHandle = Process.GetCurrentProcess().MainWindowHandle;
            SetWindowLongPtr(windowHandle, GWL_WNDPROC, _imguiWndProcPtr);

            Instance = null!;
        }
        #endregion

        public bool IsKeyPressed(int key)
        {
            if (key < 0 || key >= 256) return false;

            return _keyStates[key];
        }

        private bool[] _keyStates;
        private WndProcDelegate _wndProcDelegate = null!;
        private IntPtr _wndProcPtr = IntPtr.Zero;
        private IntPtr _imguiWndProcPtr = IntPtr.Zero;

        private IntPtr WndProcDetour(IntPtr hWnd, uint msg, ulong wParam, long lParam)
        {
            if (wParam < 256)
            {
                if (msg == WM_KEYDOWN)
                {
                    _keyStates[wParam] = true;
                }
                else if (msg == WM_KEYUP)
                {
                    _keyStates[wParam] = false;
                }
            }

            // call imgui's wnd proc
            return (IntPtr)CallWindowProc(_imguiWndProcPtr, hWnd, msg, wParam, lParam);
        }

        #region user32
        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, ulong wParam, long lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
        public static extern long CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, ulong wParam, long lParam);
        [DllImport("user32.dll", EntryPoint = "FindWindowExW", SetLastError = true)]
        public static extern IntPtr FindWindowExW(IntPtr hWndParent, IntPtr hWndChildAfter, [MarshalAs(UnmanagedType.LPWStr)] string? lpszClass, [MarshalAs(UnmanagedType.LPWStr)] string? lpszWindow);

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId", SetLastError = true)]
        public static extern ulong GetWindowThreadProcessId(IntPtr hWnd, ref ulong id);

        private const uint WM_KEYDOWN = 256;
        private const uint WM_KEYUP = 257;

        private const int GWL_WNDPROC = -4;
        #endregion
    }
}
