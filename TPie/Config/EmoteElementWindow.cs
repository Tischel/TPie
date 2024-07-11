using Dalamud.Interface.Internal;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using TPie.Models.Elements;

namespace TPie.Config
{
    public class EmoteElementWindow : RingElementWindow
    {
        private EmoteElement? _emoteElement = null;
        public EmoteElement? EmoteElement
        {
            get => _emoteElement;
            set
            {
                _emoteElement = value;
                _inputText = "";
                _searchResult.Clear();

                if (value != null)
                {
                    _inputText = value.Name;
                    _needsSearch = true;
                }
            }
        }

        protected override RingElement? Element
        {
            get => EmoteElement;
            set => EmoteElement = value is EmoteElement o ? o : null;
        }

        private bool _acquired = false;
        private bool _needsSearch = false;

        private List<Emote> _searchResult = new List<Emote>();
        private ExcelSheet<Emote>? _sheet;

        public EmoteElementWindow(string name) : base(name)
        {
            _sheet = Plugin.DataManager.GetExcelSheet<Emote>();
        }

        public override unsafe void Draw()
        {
            if (EmoteElement == null) return;

            ImGui.PushItemWidth(240 * _scale);
            if (ImGui.InputText("Name ##Emote", ref _inputText, 100) || _needsSearch)
            {
                SearchEmotes(_inputText);
                _needsSearch = false;
            }

            if (ImGui.Checkbox("Acquired", ref _acquired))
            {
                SearchEmotes(_inputText);
                _needsSearch = false;
            }

            FocusIfNeeded();

            ImGui.BeginChild("##Items_List", new Vector2(284 * _scale, 170 * _scale), true);
            {
                foreach (Emote data in _searchResult)
                {
                    if (data.Icon == 0) { continue; }

                    // check if acquired
                    bool unlocked = data.UnlockLink == 0 || UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(data.UnlockLink, 1);
                    if (_acquired && !unlocked)
                    {
                        continue;
                    }

                    ImGui.PushStyleColor(ImGuiCol.Text, unlocked ? 0xFFFFFFFF : 0xFF4444FF);

                    // name
                    if (ImGui.Selectable($"\t\t\t{data.Name}", false, ImGuiSelectableFlags.None, new Vector2(0, 24)))
                    {
                        EmoteElement.Name = data.Name;
                        EmoteElement.Command = data.TextCommand.Value?.Command.ToString() ?? "";
                        EmoteElement.IconID = data.Icon;
                    }

                    ImGui.PopStyleColor();

                    // icon
                    ISharedImmediateTexture? texture = TexturesHelper.GetTextureFromIconId(data.Icon);
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
            ImGui.Checkbox("Draw Text", ref EmoteElement.DrawText);

            if (EmoteElement.DrawText)
            {
                ImGui.SameLine();
                ImGui.Checkbox("Only When Selected", ref EmoteElement.DrawTextOnlyWhenSelected);
            }

            // border
            ImGui.NewLine();
            EmoteElement.Border.Draw();
        }

        private void SearchEmotes(string text)
        {
            if (_sheet == null) { return; }

            if (_inputText.Length == 0)
            {
                _searchResult = _sheet.Where(row => row.Name.ToString().Length > 0).ToList();
            }
            else
            {
                _searchResult = _sheet.Where(row => row.Name.ToString().ToUpper().Contains(text.ToUpper())).ToList();
            }
            
            _searchResult.Sort((a, b) => a.Name.ToString().CompareTo(b.Name.ToString()));
        }
    }
}
