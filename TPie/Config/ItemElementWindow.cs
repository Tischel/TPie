using ImGuiNET;
using ImGuiScene;
using Lumina.Excel;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TPie.Helpers;
using TPie.Models.Elements;
using LuminaItem = Lumina.Excel.GeneratedSheets.Item;
using LuminaKeyItem = Lumina.Excel.GeneratedSheets.EventItem;

namespace TPie.Config
{
    public class ItemElementWindow : RingElementWindow
    {
        private ItemElement? _itemElement = null;
        public ItemElement? ItemElement
        {
            get => _itemElement;
            set
            {
                _itemElement = value;
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
            get => ItemElement;
            set => ItemElement = value is ItemElement o ? o : null;
        }

        private bool _hq = false;
        private bool _onlyInIventory = true;

        private List<ItemSearchData> _searchResult = new List<ItemSearchData>();

        private ExcelSheet<LuminaItem>? _itemsSheet;
        private ExcelSheet<LuminaKeyItem>? _keyItemsSheet;

        private bool _needsSearch = false;

        public ItemElementWindow(string name) : base(name)
        {
            _itemsSheet = Plugin.DataManager.GetExcelSheet<LuminaItem>();
            _keyItemsSheet = Plugin.DataManager.GetExcelSheet<LuminaKeyItem>();
        }

        public override void Draw()
        {
            if (ItemElement == null) return;

            ImGui.PushItemWidth(240 * _scale);
            if (ImGui.InputText("Name ##Item", ref _inputText, 100) || _needsSearch)
            {
                SearchItems(_inputText);
                _needsSearch = false;
            }

            FocusIfNeeded();

            ImGui.Checkbox("In Inventory", ref _onlyInIventory);

            ImGui.SameLine();
            ImGui.Checkbox("High Quality", ref _hq);

            ImGui.BeginChild("##Items_List", new Vector2(284 * _scale, 170 * _scale), true);
            {
                foreach (ItemSearchData data in _searchResult)
                {
                    if (_onlyInIventory && data.UsableItem == null) continue;
                    if (_hq && !data.HQ) continue;

                    // name
                    string countString = data.UsableItem?.Count > 0 && data.UsableItem?.IsKey == false ? $" x{data.UsableItem.Count}" : "";
                    string hqString = data.HQ == true ? " (HQ)" : "";

                    ImGui.PushStyleColor(ImGuiCol.Text, data.UsableItem?.Count > 0 ? 0xFF44FF44 : 0xFFFFFFFF);

                    if (ImGui.Selectable($"\t\t\t{data.Name}{hqString} (ID: {data.ItemID}){countString}", false, ImGuiSelectableFlags.None, new Vector2(0, 24)))
                    {
                        ItemElement.ItemID = data.ItemID;
                        ItemElement.HQ = _hq;
                        ItemElement.Name = data.Name;
                        ItemElement.IconID = data.IconID;
                    }

                    ImGui.PopStyleColor();

                    // icon
                    TextureWrap? texture = Plugin.TexturesCache.GetTextureFromIconId(data.IconID);
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
            ItemElement.Border.Draw();
        }

        private void SearchItems(string text)
        {
            if (_inputText.Length == 0 || _itemsSheet == null || _keyItemsSheet == null)
            {
                _searchResult.Clear();
                return;
            }

            List<ItemSearchData> items = new List<ItemSearchData>();
            List<ItemSearchData> keyItems = new List<ItemSearchData>();

            text = text.ToUpper();

            List<LuminaItem> searchResult = _itemsSheet.Where(row => row.Name.ToString().ToUpper().Contains(text)).ToList();
            foreach (LuminaItem row in searchResult)
            {
                items.Add(new ItemSearchData(row.RowId, false, row.Name, row.Icon));

                if (row.CanBeHq)
                {
                    items.Add(new ItemSearchData(row.RowId, true, row.Name, row.Icon));
                }
            }

            keyItems = _keyItemsSheet.Where(row => row.Name.ToString().ToUpper().Contains(text))
                .Select(row => new ItemSearchData(row.RowId, false, row.Name, row.Icon))
                .ToList();

            _searchResult.Clear();
            _searchResult.AddRange(items);
            _searchResult.AddRange(keyItems);
        }

        internal struct ItemSearchData
        {
            public uint ItemID;
            public bool HQ;
            public string Name;
            public uint IconID;
            public UsableItem? UsableItem;

            public ItemSearchData(uint itemId, bool hq, string name, uint iconId) : this()
            {
                ItemID = itemId;
                HQ = hq;
                Name = name;
                IconID = iconId;
                UsableItem = ItemsHelper.Instance?.GetUsableItem(itemId, hq);
            }
        }
    }
}
