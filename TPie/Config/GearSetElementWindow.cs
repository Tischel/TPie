using ImGuiNET;
using ImGuiScene;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
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

                if (value != null && value.JobID > 0 &&
                    JobsHelper.JobNames.TryGetValue(value.JobID, out string? jobName) && jobName != null)
                {
                    _jobInputText = jobName;
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
        private string[] _jobNames;
        protected string _jobInputText = "";

        public GearSetElementWindow(string name) : base(name)
        {
            _jobIds = JobsHelper.JobNames.Keys.ToArray();
            _jobNames = JobsHelper.JobNames.Values.ToArray();
        }

        public override void Draw()
        {
            if (GearSetElement == null) return;

            ImGui.PushItemWidth(180 * _scale);

            FocusIfNeeded();

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

            ImGui.InputText("Job ##Gear Set", ref _jobInputText, 100);

            ImGui.BeginChild("##GearSets_List", new Vector2(284 * _scale, 206 * _scale), true);
            {
                for (int i = 0; i < _jobIds.Length; i++)
                {
                    uint jobID = _jobIds[i];
                    string jobName = _jobNames[i];

                    if (_jobInputText.Length > 0 && !jobName.Contains(_jobInputText.ToUpper())) continue;

                    // name
                    if (ImGui.Selectable($"\t\t\t{jobName}", false, ImGuiSelectableFlags.None, new Vector2(0, 24 * _scale)))
                    {
                        try
                        {
                            GearSetElement.GearSetID = uint.Parse(_inputText);
                        }
                        catch
                        {
                            GearSetElement.GearSetID = 0;
                        }
                        GearSetElement.JobID = jobID;
                    }

                    // icon
                    TextureWrap? texture = TexturesCache.Instance?.GetTextureFromIconId(62800 + jobID);
                    if (texture != null)
                    {
                        ImGui.SameLine();
                        ImGui.SetCursorPosX(10 * _scale);
                        ImGui.Image(texture.ImGuiHandle, new Vector2(24 * _scale));
                    }
                }
            }
            ImGui.EndChild();
        }
    }
}
