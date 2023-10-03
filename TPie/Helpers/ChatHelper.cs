using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TPie.Helpers
{
    internal unsafe class ChatHelper : IDisposable
    {
        #region Singleton
        private ChatHelper()
        {
            _chatModulePtr = Plugin.SigScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9");
        }

        public static void Initialize() { Instance = new ChatHelper(); }

        public static ChatHelper Instance { get; private set; } = null!;

        ~ChatHelper()
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

        private IntPtr _chatModulePtr;

        public static unsafe bool IsInputTextActive()
        {
            Framework* framework = Framework.Instance();
            if (framework == null) { return false; }

            UIModule* module = framework->GetUiModule();
            if (module == null) { return false; }

            RaptureAtkModule* atkModule = module->GetRaptureAtkModule();
            if (atkModule == null) { return false; }

            return atkModule->AtkModule.IsTextInputActive();
        }

        public static void SendChatMessage(string message)
        {
            if (Instance == null)
            {
                return;
            }

            Instance.SendMessage(message);
        }

        private void SendMessage(string message)
        {
            if (message == null || message.Length == 0)
            {
                return;
            }

            // let dalamud process the command first
            if (Plugin.CommandManager.ProcessCommand(message))
            {
                return;
            }

            if (_chatModulePtr == IntPtr.Zero)
            {
                return;
            }

            // encode message
            var (text, length) = EncodeMessage(message);
            var payload = MessagePayload(text, length);

            ChatDelegate chatDelegate = Marshal.GetDelegateForFunctionPointer<ChatDelegate>(_chatModulePtr);
            chatDelegate.Invoke(Plugin.GameGui.GetUIModule(), payload, IntPtr.Zero, (byte)0);

            Marshal.FreeHGlobal(payload);
            Marshal.FreeHGlobal(text);
        }

        private static (IntPtr, long) EncodeMessage(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var mem = Marshal.AllocHGlobal(bytes.Length + 30);
            Marshal.Copy(bytes, 0, mem, bytes.Length);
            Marshal.WriteByte(mem + bytes.Length, 0);
            return (mem, bytes.Length + 1);
        }

        private static IntPtr MessagePayload(IntPtr message, long length)
        {
            var mem = Marshal.AllocHGlobal(400);
            Marshal.WriteInt64(mem, message.ToInt64());
            Marshal.WriteInt64(mem + 0x8, 64);
            Marshal.WriteInt64(mem + 0x10, length);
            Marshal.WriteInt64(mem + 0x18, 0);
            return mem;
        }
    }

    public delegate IntPtr UiModuleDelegate(IntPtr baseUiPtr);
    public delegate IntPtr ChatDelegate(IntPtr uiModulePtr, IntPtr message, IntPtr unknown1, byte unknown2);
}
