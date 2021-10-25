using Dalamud.Logging;
using ImGuiNET;
using System.Windows.Forms;
using TPie.Helpers;

namespace TPie.Models
{
    public class KeyBind
    {
        public int Key;
        public bool Ctrl;
        public bool Alt;
        public bool Shift;

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
            string key = ((Keys)Key).ToString();

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

        public void Draw(string id)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            string dispKey = ToString();

            ImGui.InputText($"##{id}_Keybind", ref dispKey, 200, ImGuiInputTextFlags.ReadOnly);
            DrawHelper.SetTooltip("Escape to clear");

            int keyPressed = KeyboardHelper.Instance.GetKeyPressed();
            if (ImGui.IsItemActive() && keyPressed > 0)
            {
                Ctrl = io.KeyCtrl;
                Alt = io.KeyAlt;
                Shift = io.KeyShift;
                Key = keyPressed;
            }

            if (KeyboardHelper.Instance.IsKeyPressed((int)Keys.Escape))
            {
                Key = 0;
            }
        }
    }
}
