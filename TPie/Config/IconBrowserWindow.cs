using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using Lumina.Data.Files;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TPie.Helpers;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace TPie.Config
{
    public class IconBrowserWindow : Window
    {
        private float _scale => ImGuiHelpers.GlobalScale;
        private int _columns;
        private Vector2 IconSize = new Vector2(50);

        private int _searchTypeIndex = 0;
        private string _searchTerm = "";
        private HashSet<uint> _searchResults = new HashSet<uint>();

        private TexturesCache _cache = new TexturesCache();
        private BrowsableIcons _browsableIcons = new BrowsableIcons();

        public uint? _selectedId = null;
        public Action<uint>? OnSelect = null;

        public IconBrowserWindow(string name) : base(name)
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse;
            Size = new Vector2(500, 500);

            _columns = (int)(500 / (IconSize.X + ImGui.GetStyle().ItemSpacing.X));

            PositionCondition = ImGuiCond.Appearing;
        }

        public override void OnClose()
        {
            _cache.Clear();
        }


        public override void Draw()
        {
            if (!ImGui.BeginTabBar("##TPie_Icon_Browser_TabBar"))
            {
                return;
            }

            // General
            if (ImGui.BeginTabItem("Browse ##TPie_Icon_Browse_Tab"))
            {
                DrawBrowseTab();
                ImGui.EndTabItem();
            }

            // Rings
            if (ImGui.BeginTabItem("Search ##TPie_Icon_Search_Tab"))
            {
                DrawSearchTab();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        private void DrawSearchTab()
        {
            if (ImGui.Combo("Type", ref _searchTypeIndex, _searchTypes, _searchTypes.Length))
            {
                if (_searchTerm.Length <= 2)
                {
                    _searchResults.Clear();
                    return;
                }

                _searchResults = new HashSet<uint>(SearchIcons());
            }

            if (ImGui.InputText("Search", ref _searchTerm, 64))
            {
                if (_searchTerm.Length <= 2)
                {
                    _searchResults.Clear();
                    return;
                }

                _searchResults = new HashSet<uint>(SearchIcons());
            }

            ImGui.BeginChild($"##IconSearch");
            DrawIconGrid(_searchResults);
            ImGui.EndChild();
        }

        private List<uint> SearchIcons()
        {
            List<uint> result = new List<uint>();

            switch (_searchTypeIndex)
            {
                case 0:
                    {
                        ExcelSheet<Action>? sheet = Plugin.DataManager.GetExcelSheet<Action>();
                        if (sheet == null) return result;

                        result = sheet
                            .Where(row => row.Name.ToString().ToUpper().Contains(_searchTerm.ToUpper()))
                            .Select(row => (uint)row.Icon)
                            .ToList();
                        break;
                    }

                case 1:
                    {
                        ExcelSheet<Item>? sheet = Plugin.DataManager.GetExcelSheet<Item>();
                        if (sheet == null) return result;

                        result = sheet
                            .Where(row => row.Name.ToString().ToUpper().Contains(_searchTerm.ToUpper()))
                            .Select(row => (uint)row.Icon)
                            .ToList();
                        break;
                    }

                case 2:
                    {
                        ExcelSheet<EventItem>? sheet = Plugin.DataManager.GetExcelSheet<EventItem>();
                        if (sheet == null) return result;

                        result = sheet
                            .Where(row => row.Name.ToString().ToUpper().Contains(_searchTerm.ToUpper()))
                            .Select(row => (uint)row.Icon)
                            .ToList();
                        break;
                    }

                case 3:
                    {
                        ExcelSheet<Mount>? sheet = Plugin.DataManager.GetExcelSheet<Mount>();
                        if (sheet == null) return result;

                        result = sheet
                            .Where(row => row.Singular.ToString().ToUpper().Contains(_searchTerm.ToUpper()))
                            .Select(row => (uint)row.Icon)
                            .ToList();
                        break;
                    }

                case 4:
                    {
                        ExcelSheet<Companion>? sheet = Plugin.DataManager.GetExcelSheet<Companion>();
                        if (sheet == null) return result;

                        result = sheet
                            .Where(row => row.Singular.ToString().ToUpper().Contains(_searchTerm.ToUpper()))
                            .Select(row => (uint)row.Icon)
                            .ToList();
                        break;
                    }
            }

            return result;
        }

        private unsafe void DrawBrowseTab()
        {
            ImGui.BeginChild($"##IconBrowser");
            DrawIconGrid(_browsableIcons.IDs);
            ImGui.EndChild();
        }

        private unsafe void DrawIconGrid(IEnumerable<uint> icons)
        {
            int count = icons.Count();
            int index = 0;

            ImGuiListClipperPtr clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
            clipper.Begin(count / _columns + 1, (IconSize.Y * _scale) + ImGui.GetStyle().ItemSpacing.Y);

            while (clipper.Step())
            {
                for (int row = clipper.DisplayStart; row < clipper.DisplayEnd; row++)
                {
                    int start = row * _columns;
                    int end = Math.Min(start + _columns, count);

                    bool first = true;

                    for (int i = start; i < end; i++)
                    {
                        if (i >= count) break;
                        uint iconId = icons.ElementAt(i);

                        TextureWrap? texture = _cache.GetTextureFromIconId(iconId) ?? _cache.GetTextureFromIconId(61502);
                        if (texture != null)
                        {
                            if (!first)
                            {
                                ImGui.SameLine();
                            }
                            first = false;

                            Vector2 cursorPos = ImGui.GetCursorPos();

                            // selected
                            if (_selectedId.HasValue && _selectedId.Value == iconId)
                            {
                                Vector2 offset = new Vector2(2);
                                ImGui.PushStyleColor(ImGuiCol.Button, 0xAAFFFFFF);
                                ImGui.SetCursorPos(cursorPos - offset);
                                ImGui.Button("", IconSize * _scale + offset * 2);
                                ImGui.PopStyleColor();
                            }

                            ImGui.SetCursorPos(cursorPos);
                            ImGui.Image(texture.ImGuiHandle, IconSize * _scale);
                        }

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip($"{iconId}");

                            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                            {
                                _selectedId = iconId;
                                OnSelect?.Invoke(iconId);
                            }
                        }

                        index++;
                    }
                }
            }

            clipper.Destroy();
        }

        private static string[] _searchTypes =
        {
            "Action",
            "Item",
            "Key Item",
            "Mount",
            "Companion"
        };
    }

    internal class BrowsableIcons
    {
        private static Range[] _ranges = {
            new Range(66000, 66399), // macro
            new Range(62000, 62599),
            new Range(62801, 62899), // gear set
            new Range(1, 99), // system
            new Range(61200, 61250), // marks
            new Range(61290, 61390), // marks
            new Range(65001, 65900), // currency
            new Range(91000, 94999), // symbols
        };

        private static List<uint> _excluded = new List<uint>()
        {
            66000, 66020, 66040, 66060, 66080, 66100, 66120, 66140, 66160, 66180, 66300,
            62000, 62100, 62200, 62250, 62275, 62300, 62400, 62500, 62520, 62580, 61200,
            61210, 61220, 61230, 61240, 61250, 61290, 61300, 61310, 61320, 61330, 61340,
            61350, 61370, 61377, 61390
        };

        public List<uint> IDs = new List<uint>();

        public BrowsableIcons()
        {
            foreach (Range range in _ranges)
            {
                for (int i = range.Start.Value; i <= range.End.Value; i++)
                {
                    if (!IsIDValid(i) || _excluded.Contains((uint)i)) continue;

                    IDs.Add((uint)i);
                }
            }
        }

        private bool IsIDValid(int id)
        {
            try
            {
                var path = $"ui/icon/{id / 1000 * 1000:000000}/{id:000000}_hr1.tex";
                return Plugin.DataManager.GetFile<TexFile>(path) != null;
            }
            catch
            {
                return false;
            }
        }
    }
}