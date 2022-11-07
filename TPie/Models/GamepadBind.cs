using System.Collections.Generic;
using Dalamud.Game.ClientState.GamePad;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ImGuiNET;
using TPie.Helpers;

namespace TPie.Models
{
    public class GamepadBind
    {
        public int Button;
        public bool L2;
        public bool R2;

        public bool Toggle;

        public HashSet<uint> Jobs { get; set; }
        public bool IsGlobal => Jobs.Count == 0 || Jobs.Count == JobsHelper.JobNames.Count;

        private bool _waitingForRelease;
        private bool _active;

        public GamepadBind(int button, bool l2 = false,  bool r2 = false )
        {
            Button = button;
            L2 = l2;
            R2 = r2;
            Jobs = new HashSet<uint>();
        }

        public override string ToString()
        {
            string button = L2 ? "L2 + " : "";
            button += R2 ? "R2 + " : "";
            button += ((GamepadButtons)Button).ToString();

            return button;
        }

        public string Description()
        {
            string toggleStringPrefix = Toggle ? "[" : "";
            string toggleStringSufix = Toggle ? "]" : "";

            return toggleStringPrefix + ToString() + toggleStringSufix;
        }

        public bool IsActive()
        {
            bool pressed = Plugin.GamepadState.Raw(GamepadButtons.L2) > 0.1f; //todo : replace with Trigger deadzone config
            bool l2 = L2 ? pressed : !pressed;
            pressed = Plugin.GamepadState.Raw(GamepadButtons.R2) > 0.1f; //todo : replace with Trigger deadzone config
            bool r2 = R2 ? pressed : !pressed;
            bool button = Plugin.GamepadState.Raw((GamepadButtons)Button)>0.1f; //todo : replace with deadzone config
            bool active = l2 && r2 && button;

            // check job
            PlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            if (player != null && Jobs.Count > 0)
            {
                active &= Jobs.Contains(player.ClassJob.Id);
            }

            // block keybind for the game?
            if (active && !Plugin.Settings.KeybindPassthrough)
            {
                try
                {
                    ImGuiIOPtr io = ImGui.GetIO();
                    //io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
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

        public void Deactivate()
        {
            _active = false;
            _waitingForRelease = false;
            try
            {
                ImGuiIOPtr io = ImGui.GetIO();
                //io.ConfigFlags &= ~ImGuiConfigFlags.NavEnableGamepad;
            }
            catch { }
        }

        public bool Draw(string id, float width)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            string dispKey = ToString();

            ImGui.PushItemWidth(width);
            ImGui.InputText($"##{id}_GamepadBind", ref dispKey, 200, ImGuiInputTextFlags.ReadOnly);
            DrawHelper.SetTooltip("Backspace to clear");

            if (ImGui.IsItemActive())
            {
                if (KeyboardHelper.Instance?.IsKeyPressed((int)Keys.Back) == true)
                {
                    Reset();
                }
                else
                {
                    var ButtonPressed = GetKeyPressed();
                    if (ButtonPressed != GamepadButtons.None)
                    {
                        L2 = Plugin.GamepadState.Raw(GamepadButtons.L2) > 0.1f;
                        R2 = Plugin.GamepadState.Raw(GamepadButtons.R2) > 0.1f;
                        Button = (int)ButtonPressed;
                        return true;
                    }
                }
            }

            return false;
        }
        
        public GamepadButtons GetKeyPressed()
        {
            for (int i = 0; i < _supportedButtons.Length; i++)
            {
                if (Plugin.GamepadState.Raw(_supportedButtons[i]) > 0.1f)
                {
                    return _supportedButtons[i];
                }
            }

            return GamepadButtons.None;
        }

        private GamepadButtons[] _supportedButtons = new GamepadButtons[]
        {
            GamepadButtons.L1,
            GamepadButtons.L3,
            GamepadButtons.R1,
            GamepadButtons.R3,
            GamepadButtons.Select,
            GamepadButtons.Start,
            GamepadButtons.DpadLeft,
            GamepadButtons.DpadUp,
            GamepadButtons.DpadRight,
            GamepadButtons.DpadDown,
            GamepadButtons.West,
            GamepadButtons.North,
            GamepadButtons.East,
            GamepadButtons.South
        };

        public void Reset()
        {
            L2 = false;
            R2 = false;
            Button = (int)GamepadButtons.None;
        }

        public bool Equals(GamepadBind bind)
        {
            return L2 == bind.L2 
                 && R2 == bind.R2 
                 && Button == bind.Button;
        }
        
    }
}