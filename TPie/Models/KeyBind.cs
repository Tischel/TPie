using ImGuiNET;
using TPie.Helpers;

namespace TPie.Models
{
    public class KeyBind
    {
        public readonly int Key;
        public readonly bool Ctrl;
        public readonly bool Alt;
        public readonly bool Shift;

        public KeyBind(int key, bool ctrl = false, bool alt = false, bool shift = false)
        {
            Ctrl = ctrl;
            Alt = alt;
            Shift = shift;
            Key = key;
        }

        public override string ToString()
        {
            string ctrl = Ctrl ? "Ctrl + " : "";
            string alt = Alt ? "Alt + " : "";
            string shift = Shift ? "Shift + " : "";
            string key = ((char)Key).ToString().ToUpper();

            return ctrl + alt + shift + key;
        }

        public bool IsActive()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            bool ctrl = Ctrl ? io.KeyCtrl : !io.KeyCtrl;
            bool alt = Alt ? io.KeyAlt : !io.KeyAlt;
            bool shift = Shift ? io.KeyShift : !io.KeyShift;
            bool key = KeyboardHelper.Instance?.IsKeyPressed(Key) == true;

            return ctrl && alt && shift && key;
        }
    }
}
