using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;
using TPie.Helpers;
using TPie.Models;

namespace TPie.Config
{
    public class KeyBindWindow : Window
    {
        public Ring? Ring;
        private float _scale => ImGuiHelpers.GlobalScale;
        private int _selectedRole = 0;
        private string[] _roleNames;
        private bool _needsFocus = false;

        public KeyBindWindow(string name) : base(name)
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse;
            Size = new Vector2(300, 390);

            PositionCondition = ImGuiCond.Appearing;

            _roleNames = new string[]
            {
                JobsHelper.RoleNames[JobRoles.Tank],
                JobsHelper.RoleNames[JobRoles.Healer],
                JobsHelper.RoleNames[JobRoles.DPSMelee],
                JobsHelper.RoleNames[JobRoles.DPSRanged],
                JobsHelper.RoleNames[JobRoles.DPSCaster],
                JobsHelper.RoleNames[JobRoles.Crafter],
                JobsHelper.RoleNames[JobRoles.Gatherer]
            };
        }

        public override void OnOpen()
        {
            _needsFocus = true;
        }

        public override void OnClose()
        {
        }

        public override void Draw()
        {
            if (Ring == null) return;
            KeyBind keyBind = Ring.KeyBind;
            GamepadBind gamepadBind = Ring.GamepadBind;

            // main
            ImGui.BeginChild("##KeyBind_Main", new Vector2(280 * _scale, 154 * _scale), true);
            {
                if (_needsFocus)
                {
                    ImGui.SetKeyboardFocusHere(0);
                    _needsFocus = false;
                }

                if (keyBind.Draw(Ring.Name, 250))
                {
                    Plugin.Settings.ValidateKeyBind(Ring);
                }

                ImGui.Checkbox("Toggleable", ref keyBind.Toggle);
                DrawHelper.SetTooltip("When enabled, this keybind will behave as a toggle instead of \"press and hold\".\nOn this mode, once an item is selected, you can either press the keybind again or just click to activate it.");

                
                if (gamepadBind.Draw(Ring.Name, 250))
                {
                    Plugin.Settings.ValidateKeyBind(Ring);
                }

                ImGui.Checkbox("Gamepad Toggleable", ref gamepadBind.Toggle);
                DrawHelper.SetTooltip("When enabled, this Gamepad bind will behave as a toggle instead of \"press and hold\".\nOn this mode, once an item is selected, you must press the keybind again to activate it.");

                bool isGlobal = keyBind.IsGlobal;
                if (ImGui.Checkbox("Use for all jobs", ref isGlobal))
                {
                    if (isGlobal)
                    {
                        keyBind.Jobs.Clear();
                        gamepadBind.Jobs.Clear();
                    }

                    Plugin.Settings.ValidateKeyBind(Ring);
                }
            }
            ImGui.EndChild();

            // jobs
            ImGui.NewLine();
            ImGui.Text("Use for specific jobs:");
            ImGui.BeginChild("##KeyBind_Jobs", new Vector2(280 * _scale, 144 * _scale), true);
            {
                ImGui.Combo("Role", ref _selectedRole, _roleNames, _roleNames.Length);
                DrawJobs((JobRoles)_selectedRole);
            }
            ImGui.EndChild();
        }

        private void DrawJobs(JobRoles role)
        {
            if (Ring == null) return;
            KeyBind keyBind = Ring.KeyBind;
            GamepadBind gamepadBind = Ring.GamepadBind;

            List<uint> roleJobs = JobsHelper.JobsByRole[role];
            bool hasAll = true;

            foreach (uint job in roleJobs)
            {
                if (!keyBind.Jobs.Contains(job))
                {
                    hasAll = false;
                    break;
                }
            }

            if (ImGui.Checkbox("All", ref hasAll))
            {
                foreach (uint job in roleJobs)
                {
                    if (hasAll)
                    {
                        keyBind.Jobs.Add(job);
                        gamepadBind.Jobs.Add(job);
                    }
                    else
                    {
                        keyBind.Jobs.Remove(job);
                        gamepadBind.Jobs.Remove(job);
                    }
                }

                Plugin.Settings.ValidateKeyBind(Ring);
            }

            ImGui.NewLine();

            int index = 0;
            int count = 0;

            while (index < roleJobs.Count)
            {
                uint job = roleJobs[index];

                bool hasJob = keyBind.Jobs.Contains(job);
                if (ImGui.Checkbox(JobsHelper.JobNames[job], ref hasJob))
                {
                    if (hasJob)
                    {
                        keyBind.Jobs.Add(job);
                        gamepadBind.Jobs.Add(job);
                    }
                    else
                    {
                        keyBind.Jobs.Remove(job);
                        gamepadBind.Jobs.Remove(job);
                    }

                    Plugin.Settings.ValidateKeyBind(Ring);
                }

                count++;
                if (count < 4)
                {
                    ImGui.SameLine();
                }
                else
                {
                    count = 0;
                }

                index++;
            }
        }
    }
}
