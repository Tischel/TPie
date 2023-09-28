using Dalamud.Interface.Internal;
using DelvUI.Helpers;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TPie.Helpers;
using TPie.Models.Elements;
using LuminaAction = Lumina.Excel.GeneratedSheets.Action;

namespace TPie.Config
{
    public class ActionElementWindow : RingElementWindow
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

        protected override RingElement? Element
        {
            get => ActionElement;
            set => ActionElement = value is ActionElement o ? o : null;
        }

        private List<LuminaAction> _searchResult = new List<LuminaAction>();
        private ExcelSheet<LuminaAction>? _sheet;

        public ActionElementWindow(string name) : base(name)
        {
            _sheet = Plugin.DataManager.GetExcelSheet<LuminaAction>();
        }

        public override void Draw()
        {
            if (ActionElement == null) return;

            ImGui.PushItemWidth(210 * _scale);
            if (ImGui.InputText("ID or Name ##Action", ref _inputText, 100))
            {
                SearchActions(_inputText);
            }

            FocusIfNeeded();

            ImGui.BeginChild("##Actions_List", new Vector2(284 * _scale, 200 * _scale), true);
            {
                foreach (LuminaAction data in _searchResult)
                {
                    // name
                    if (ImGui.Selectable($"\t\t\t{data.Name} (ID: {data.RowId})", false, ImGuiSelectableFlags.None, new Vector2(0, 24 * _scale)))
                    {
                        ActionElement.ActionID = data.RowId;
                    }

                    // icon
                    IDalamudTextureWrap? texture = TexturesHelper.GetTextureFromIconId(data.Icon);
                    if (texture != null)
                    {
                        ImGui.SameLine();
                        ImGui.SetCursorPosX(10 * _scale);
                        ImGui.Image(texture.ImGuiHandle, new Vector2(24 * _scale));
                    }
                }
            }
            ImGui.EndChild();

            // border
            ImGui.NewLine();
            ActionElement.Border.Draw();
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
