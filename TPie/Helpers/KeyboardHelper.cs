using Dalamud.Logging;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TPie.Helpers
{
    internal class KeyboardHelper
    {
        #region singleton
        private KeyboardHelper()
        {
            _keyStates = new byte[256];
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

            Instance = null!;
        }
        #endregion

        public void Update()
        {
            GetKeyboardState(_keyStates);
        }

        public bool IsKeyPressed(int key)
        {
            if (key < 0 || key >= 256) return false;

            return _keyStates[key] > 1;
        }

        public int GetKeyPressed()
        {
            for (int i = (int)Keys.Space; i < _keyStates.Length; i++)
            {
                if (_keyStates[i] > 1)
                {
                    return i;
                }
            }

            return 0;
        }

        private byte[] _keyStates;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetKeyboardState(byte[] keyStates);
    }
}
