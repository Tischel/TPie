using Dalamud.Interface.Internal;
using DelvUI.Helpers;
using ImGuiNET;
using ImGuiScene;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using TPie.Helpers;
using TPie.Models.Elements;

namespace TPie.Config
{
    public class GearSetElementWindow : RingElementWindow
    {
        private GearSetElement? _gearSetElement = null;
        public GearSetElement? GearSetElement
        {
            get => _gearSetElement;
            set
            {
                _gearSetElement = value;
                _inputText = value != null ? $"{value.GearSetID}" : "";
                _nameInputText = value?.GearSetName ?? "";

                if (value != null && value.JobID > 0 &&
                    JobsHelper.JobNames.TryGetValue(value.JobID, out string? jobName) && jobName != null)
                {
                    _jobInputText = jobName;
                    if (value.UseID)
                    {
                        _nameInputText = jobName;
                        value.GearSetName = jobName;
                    }
                }
                else
                {
                    _jobInputText = "";
                }
            }
        }

        protected override RingElement? Element
        {
            get => GearSetElement;
            set => GearSetElement = value is GearSetElement o ? o : null;
        }

        private uint[] _jobIds;
        private List<string> _jobNames;
        private string _jobInputText = "";
        private string _nameInputText = "";

        public GearSetElementWindow(string name) : base(name)
        {
            _jobIds = JobsHelper.JobNames.Keys.ToArray();
            _jobNames = JobsHelper.JobNames.Values.ToList();
        }

        public override void Draw()
        {
            if (GearSetElement == null) return;

            ImGui.PushItemWidth(120 * _scale);

            if (ImGui.RadioButton("Use Set Number", GearSetElement.UseID))
            {
                GearSetElement.UseID = true;
                ImGui.SetKeyboardFocusHere(0);
            }

            ImGui.SameLine();
            if (ImGui.RadioButton("Use Set Name", !GearSetElement.UseID))
            {
                GearSetElement.UseID = false;
                ImGui.SetKeyboardFocusHere(0);
            }

            ImGui.PushItemWidth(180 * _scale);
            FocusIfNeeded();

            if (GearSetElement.UseID)
            {
                string str = _inputText;
                if (ImGui.InputText("Gear Set Number ##GearSet", ref str, 100, ImGuiInputTextFlags.CharsDecimal))
                {
                    _inputText = Regex.Replace(str, @"[^\d]", "");

                    try
                    {
                        GearSetElement.GearSetID = uint.Parse(_inputText);
                    }
                    catch { }
                }
            }
            else
            {
                if (ImGui.InputText("Gear Set Name ##GearSet", ref _nameInputText, 100))
                {
                    GearSetElement.GearSetName = _nameInputText;

                    if (_nameInputText.Length >= 3)
                    {
                        string firstThreeLetters = _nameInputText.Substring(0, 3).ToUpper();
                        int index = _jobNames.IndexOf(firstThreeLetters);
                        if (index >= 0)
                        {
                            GearSetElement.JobID = _jobIds[index];
                            _jobInputText = firstThreeLetters;
                        }
                    }
                    DrawHelper.SetTooltip("Your gear set name should start with this (case sensitive)");
                }
            }

            ImGui.InputText("Job ##Gear Set", ref _jobInputText, 100);

            ImGui.BeginChild("##GearSets_List", new Vector2(284 * _scale, 130 * _scale), true);
            {
                for (int i = 0; i < _jobIds.Length; i++)
                {
                    uint jobID = _jobIds[i];
                    string jobName = _jobNames[i];

                    if (_jobInputText.Length > 0 && !jobName.Contains(_jobInputText.ToUpper())) continue;

                    // name
                    if (ImGui.Selectable($"\t\t\t{jobName}", false, ImGuiSelectableFlags.None, new Vector2(0, 24 * _scale)))
                    {
                        GearSetElement.JobID = jobID;
                    }

                    // icon
                    ISharedImmediateTexture texture = TexturesHelper.GetTextureFromIconId(62800 + jobID);
                    if (texture != null)
                    {
                        ImGui.SameLine();
                        ImGui.SetCursorPosX(10 * _scale);
                        ImGui.Image(texture.GetWrapOrEmpty().ImGuiHandle, new Vector2(24 * _scale));
                    }
                }
            }
            ImGui.EndChild();

            // draw text
            ImGui.NewLine();
            ImGui.Checkbox("Draw Text", ref GearSetElement.DrawText);

            if (GearSetElement.DrawText)
            {
                ImGui.SameLine();
                ImGui.Checkbox("Only When Selected", ref GearSetElement.DrawTextOnlyWhenSelected);
            }

            // border
            ImGui.NewLine();
            GearSetElement.Border.Draw();
        }
    }
}
