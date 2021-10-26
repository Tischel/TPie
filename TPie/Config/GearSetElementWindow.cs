using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Linq;
using System.Numerics;
using TPie.Helpers;
using TPie.Models.Elements;

namespace TPie.Config
{
    public class GearSetElementWindow : Window
    {
        private GearSetElement? _gearSetElement = null;
        public GearSetElement? GearSetElement
        {
            get => _gearSetElement;
            set
            {
                _gearSetElement = value;
                _inputText = value != null ? $"{value.GearSetID}" : "";
            }
        }

        public Action<RingElement?>? Callback = null;

        private uint[] _jobIds;
        private string[] _jobNames;

        private string _inputText = "";
        private bool _needsFocus = false;

        public GearSetElementWindow(string name) : base(name)
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse;
            Size = new Vector2(300, 300);

            PositionCondition = ImGuiCond.Appearing;

            _jobIds = JobsHelper.JobNames.Keys.ToArray();
            _jobNames = JobsHelper.JobNames.Values.ToArray();
        }

        public override void OnOpen()
        {
            _needsFocus = true;
        }

        public override void OnClose()
        {
            Callback?.Invoke(GearSetElement);
            Callback = null;
            GearSetElement = null;
        }

        public override void PreDraw()
        {
            if (GearSetElement == null)
            {
                uint jobId = Plugin.ClientState.LocalPlayer?.ClassJob.Id ?? JobIDs.GLD;
                GearSetElement = new GearSetElement(1, jobId);
            }
        }

        public override void Draw()
        {
            if (GearSetElement == null) return;

            ImGui.PushItemWidth(180);
            if (ImGui.InputText("Gear Set Number ##GearSet", ref _inputText, 100, ImGuiInputTextFlags.CharsDecimal))
            {
                try
                {
                    GearSetElement.GearSetID = uint.Parse(_inputText);
                }
                catch { }
            }

            if (_needsFocus)
            {
                ImGui.SetKeyboardFocusHere(0);
                _needsFocus = false;
            }

            ImGui.BeginChild("##GearSets_List", new Vector2(284, 236), true);
            {
                for (int i = 0; i < _jobIds.Length; i++)
                {
                    uint jobID = _jobIds[i];
                    string jobName = _jobNames[i];

                    // name
                    if (ImGui.Selectable($"\t\t\t{jobName}", false, ImGuiSelectableFlags.None, new Vector2(0, 24)))
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

                        Callback?.Invoke(GearSetElement);
                        Callback = null;
                        IsOpen = false;
                        return;
                    }

                    // icon
                    TextureWrap? texture = TexturesCache.Instance?.GetTextureFromIconId(62800 + jobID);
                    if (texture != null)
                    {
                        ImGui.SameLine();
                        ImGui.SetCursorPosX(10);
                        ImGui.Image(texture.ImGuiHandle, new Vector2(24));
                    }
                }
            }
            ImGui.EndChild();
        }
    }
}
