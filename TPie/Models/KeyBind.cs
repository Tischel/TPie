using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Bindings.ImGui;
using System.Collections.Generic;
using System.Linq;
using TPie.Helpers;

namespace TPie.Models
{
    public class KeyBind
    {
        public int Key;
        public bool Ctrl;
        public bool Alt;
        public bool Shift;

        public bool Toggle = false;

        public HashSet<uint> Jobs;
        public bool IsGlobal => Jobs.Count == 0 || Jobs.Count == JobsHelper.JobNames.Count;

        private bool _waitingForRelease;
        private bool _active;

        public KeyBind(int key, bool ctrl = false, bool alt = false, bool shift = false)
        {
            Ctrl = ctrl;
            Alt = alt;
            Shift = shift;
            Key = key;
            Jobs = new HashSet<uint>();
        }

        public override string ToString()
        {
            string ctrl = Ctrl ? "Ctrl + " : "";
            string alt = Alt ? "Alt + " : "";
            string shift = Shift ? "Shift + " : "";
            string key = ((Keys)Key).ToString();

            return ctrl + alt + shift + key;
        }

        public string Description()
        {
            string toggleStringPrefix = Toggle ? "[" : "";
            string toggleStringSufix = Toggle ? "]" : "";
            string jobsString = "";

            if (Jobs.Count > 0)
            {
                List<string> sortedJobs = Jobs.Select(jobId => JobsHelper.JobNames[jobId]).ToList();
                sortedJobs.Sort();
                jobsString = " (" + string.Join(", ", sortedJobs.ToArray()) + ")";
            }

            return toggleStringPrefix + ToString() + toggleStringSufix + jobsString;
        }

        public bool IsActive()
        {
            if (Chat.IsInputTextActive || ImGui.GetIO().WantTextInput)
            {
                return Toggle && _active;
            }

            ImGuiIOPtr io = ImGui.GetIO();
            bool isCtrlDown = (KeyboardHelper.GetAsyncKeyState((int)Keys.ControlKey) & 0x8000) != 0 || io.KeyCtrl;
            bool isAltDown = (KeyboardHelper.GetAsyncKeyState((int)Keys.Menu) & 0x8000) != 0 || io.KeyAlt;
            bool isShiftDown = (KeyboardHelper.GetAsyncKeyState((int)Keys.ShiftKey) & 0x8000) != 0 || io.KeyShift;

            bool ctrl = Ctrl ? isCtrlDown : !isCtrlDown;
            bool alt = Alt ? isAltDown : !isAltDown;
            bool shift = Shift ? isShiftDown : !isShiftDown;
            bool key = KeyboardHelper.Instance?.IsKeyPressed(Key) == true;
            bool active = ctrl && alt && shift && key;

            // check job
            IPlayerCharacter? player = Plugin.ObjectTable.LocalPlayer;
            if (player != null && Jobs.Count > 0)
            {
                active &= Jobs.Contains(player.ClassJob.RowId);
            }

            // block keybind for the game?
            if (active && !Plugin.Settings.KeybindPassthrough)
            {
                try
                {
                    Plugin.KeyState[Key] = false;
                }
                catch { }
            }

            if (Toggle)
            {
                if (active && !_waitingForRelease)
                {
                    _active = !_active;
                    _waitingForRelease = true;
                }
                else if (!active)
                {
                    _waitingForRelease = false;
                }

                return _active;
            }

            return active;
        }

        /// <summary>A modifier that isn't part of this binding is held, e.g. Alt from an Alt-Tab.</summary>
        public bool ExtraModifierHeld()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            bool isCtrlDown = (KeyboardHelper.GetAsyncKeyState((int)Keys.ControlKey) & 0x8000) != 0 || io.KeyCtrl;
            bool isAltDown = (KeyboardHelper.GetAsyncKeyState((int)Keys.Menu) & 0x8000) != 0 || io.KeyAlt;
            bool isShiftDown = (KeyboardHelper.GetAsyncKeyState((int)Keys.ShiftKey) & 0x8000) != 0 || io.KeyShift;

            return (!Ctrl && isCtrlDown) || (!Alt && isAltDown) || (!Shift && isShiftDown);
        }

        public void Deactivate()
        {
            _active = false;
            _waitingForRelease = false;
        }

        public bool Draw(string id, float width)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            string dispKey = ToString();

            ImGui.PushItemWidth(width);
            ImGui.InputText($"##{id}_Keybind", ref dispKey, 200, ImGuiInputTextFlags.ReadOnly);
            DrawHelper.SetTooltip("Backspace to clear");

            bool changed = false;
            if (ImGui.IsItemActive())
            {
                if (KeyboardHelper.Instance?.IsKeyPressed((int)Keys.Back) == true)
                {
                    Reset();
                    changed = true;
                }
                else
                {
                    int keyPressed = KeyboardHelper.Instance?.GetKeyPressed() ?? 0;
                    if (keyPressed > 0)
                    {
                        Ctrl = (KeyboardHelper.GetAsyncKeyState((int)Keys.ControlKey) & 0x8000) != 0 || io.KeyCtrl;
                        Alt = (KeyboardHelper.GetAsyncKeyState((int)Keys.Menu) & 0x8000) != 0 || io.KeyAlt;
                        Shift = (KeyboardHelper.GetAsyncKeyState((int)Keys.ShiftKey) & 0x8000) != 0 || io.KeyShift;
                        Key = keyPressed;
                        changed = true;
                    }
                }
            }

            ImGui.PopItemWidth();
            return changed;
        }

        public void Reset()
        {
            Key = 0;
            Ctrl = false;
            Alt = false;
            Shift = false;
        }

        public bool Equals(KeyBind bind)
        {
            return Key == bind.Key &&
                   Ctrl == bind.Ctrl &&
                   Alt == bind.Alt &&
                   Shift == bind.Shift;
        }
    }
}
