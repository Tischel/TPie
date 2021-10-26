using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TPie.Helpers;
using TPie.Models.Elements;
using LuminaAction = Lumina.Excel.GeneratedSheets.Action;

namespace TPie.Config
{
    public class ActionElementWindow : Window
    {
        private ActionElement? _actionElement = null;
        public ActionElement? ActionElement
        {
            get => _actionElement;
            set
            {
                _actionElement = value;
                _inputText = "";
                _searchResult.Clear();

                if (value?.Data != null)
                {
                    _inputText = value.Data.Name.ToString();
                    _searchResult.Add(value.Data);
                }
            }
        }

        public Action<RingElement?>? Callback = null;

        private string _inputText = "";
        private List<LuminaAction> _searchResult = new List<LuminaAction>();
        private ExcelSheet<LuminaAction>? _sheet;

        private bool _needsFocus = false;

        public ActionElementWindow(string name) : base(name)
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse;
            Size = new Vector2(300, 300);

            PositionCondition = ImGuiCond.Appearing;

            _sheet = Plugin.DataManager.GetExcelSheet<LuminaAction>();
        }

        public override void OnOpen()
        {
            _needsFocus = true;
        }

        public override void OnClose()
        {
            Callback?.Invoke(null);
            Callback = null;
            ActionElement = null;
        }

        public override void PreDraw()
        {
            if (ActionElement == null)
            {
                ActionElement = new ActionElement(0);
            }
        }

        public override void Draw()
        {
            if (ActionElement == null) return;

            ImGui.PushItemWidth(210);
            if (ImGui.InputText("ID or Name ##Action", ref _inputText, 100))
            {
                SearchActions(_inputText);
            }

            if (_needsFocus)
            {
                ImGui.SetKeyboardFocusHere(0);
                _needsFocus = false;
            }

            ImGui.BeginChild("##Actions_List", new Vector2(284, 236), true);
            {
                foreach (LuminaAction data in _searchResult)
                {
                    // name
                    if (ImGui.Selectable($"\t\t\t{data.Name} (ID: {data.RowId})", false, ImGuiSelectableFlags.None, new Vector2(0, 24)))
                    {
                        ActionElement.ActionID = data.RowId;
                        Callback?.Invoke(ActionElement);
                        Callback = null;
                        IsOpen = false;
                        return;
                    }

                    // icon
                    TextureWrap? texture = TexturesCache.Instance?.GetTextureFromIconId(data.Icon);
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

        private void SearchActions(string text)
        {
            if (_inputText.Length == 0 || _sheet == null)
            {
                _searchResult.Clear();
                return;
            }

            int intValue = 0;
            try
            {
                intValue = int.Parse(text);
            }
            catch { }

            if (intValue > 0)
            {
                _searchResult = _sheet.Where(row => row.RowId == intValue).ToList();
                return;
            }

            _searchResult = _sheet.Where(row => row.Name.ToString().ToUpper().Contains(text.ToUpper())).ToList();
        }
    }
}
