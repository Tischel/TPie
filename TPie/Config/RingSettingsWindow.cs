using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using System.Numerics;
using System.Text.RegularExpressions;
using TPie.Helpers;
using TPie.Models;
using TPie.Models.Elements;

namespace TPie.Config
{
    public class RingSettingsWindow : Window
    {
        private Ring? _ring = null;
        public Ring? Ring
        {
            get => _ring;
            set
            {
                _selectedIndex = -1;
                _ring?.EndPreview();
                _ring = value;
            }
        }

        private int _selectedIndex = -1;
        private float _scale => ImGuiHelpers.GlobalScale;

        private Vector2 _windowPos = Vector2.Zero;
        private Vector2 ItemWindowPos => _windowPos + new Vector2(410 * _scale, 0);

        public RingSettingsWindow(string name) : base(name)
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse;
            Size = new Vector2(400, 464);

            PositionCondition = ImGuiCond.Appearing;
        }

        public override void PreDraw()
        {
            if (Ring == null || !Plugin.Settings.Rings.Contains(Ring))
            {
                IsOpen = false;
            }
        }

        public override void Draw()
        {
            if (Ring == null) return;

            _windowPos = ImGui.GetWindowPos();

            // ring preview
            Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 margin = new Vector2(20 * _scale);
            Vector2 ringCenter = windowPos + new Vector2(Size!.Value.X + Ring.Radius + margin.X, Size!.Value.Y / 2f);
            Ring.Preview(ringCenter);

            // info
            ImGui.BeginChild("##Ring_Info", new Vector2(384 * _scale, 178 * _scale), true);
            {
                ImGui.PushItemWidth(310 * _scale);

                ImGui.InputText("Name ##Ring_Info_Name", ref Ring.Name, 100);

                Vector3 color = new Vector3(Ring.Color.X, Ring.Color.Y, Ring.Color.Z);
                if (ImGui.ColorEdit3("Color ##Ring_Info_Color", ref color))
                {
                    Ring.Color = new Vector4(color.X, color.Y, color.Z, 1);
                }

                if (Ring.KeyBind.Draw(Ring.Name, 310 * _scale, true))
                {
                    Plugin.Settings.ValidateKeyBind(Ring);
                }

                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 4);
                ImGui.Text("Keybind");

                ImGui.PushItemWidth(128 * _scale);
                ImGui.DragFloat("Radius ##Ring_Info_Radius", ref Ring.Radius, 1, 150, 500);

                ImGui.SameLine();
                ImGui.DragFloat("Rotation ##Ring_Info_Rotation", ref Ring.Rotation, .5f, -359, 359);

                ImGui.PushItemWidth(310 * _scale);
                ImGui.DragFloat2("Items Size ##Ring_Info_ItemSize", ref Ring.ItemSize, 1, 10, 500);

                ImGui.Checkbox("Line", ref Ring.DrawLine);

                ImGui.SameLine();
                ImGui.Checkbox("Selection Background", ref Ring.DrawSelectionBackground);

                ImGui.SameLine();
                ImGui.Checkbox("Tooltips", ref Ring.ShowTooltips);
                DrawHelper.SetTooltip("This will show a tooltip with a description of an element when hovering on top of it.");
            }
            ImGui.EndChild();

            // items
            var flags = ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.BordersInner |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.SizingFixedSame;

            if (ImGui.BeginTable("##Item_Table", 3, flags, new Vector2(354 * _scale, 242 * _scale)))
            {
                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthStretch, 22, 0);
                ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthStretch, 7, 1);
                ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch, 71, 2);

                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                for (int i = 0; i < Ring.Items.Count; i++)
                {
                    RingElement item = Ring.Items[i];

                    ImGui.PushID(i.ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);

                    // type
                    if (ImGui.TableSetColumnIndex(0))
                    {
                        if (ImGui.Selectable(item.UserFriendlyName(), _selectedIndex == i, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, 24)))
                        {
                            _selectedIndex = i;
                        }
                    }

                    // icon
                    if (ImGui.TableSetColumnIndex(1))
                    {
                        TextureWrap? texture = Plugin.TexturesCache.GetTextureFromIconId(item.IconID);
                        if (texture != null)
                        {
                            ImGui.Image(texture.ImGuiHandle, new Vector2(24));
                        }
                    }

                    // description
                    if (ImGui.TableSetColumnIndex(2))
                    {
                        bool valid = item.IsValid();
                        Vector4 c = valid ? Vector4.One : new(1, 0, 0, 1);
                        ImGui.TextColored(c, item.Description());

                        if (!valid)
                        {
                            DrawHelper.SetTooltip(item.InvalidReason());
                        }
                    }
                }

                ImGui.EndTable();
            }

            ImGui.SetCursorPos(new Vector2(369 * _scale, 230 * _scale));
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString()))
            {
                ImGui.OpenPopup("##TPie_Add_Item_Menu");
            }
            ImGui.PopFont();
            DrawHelper.SetTooltip("Add");

            if (_selectedIndex >= 0)
            {
                ImGui.SetCursorPos(new Vector2(369 * _scale, 260 * _scale));
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.Pen.ToIconString()))
                {
                    ShowEditItemWindow();
                }
                ImGui.PopFont();
                DrawHelper.SetTooltip("Edit");
            }

            if (_selectedIndex >= 0)
            {
                ImGui.SetCursorPos(new Vector2(369 * _scale, 290 * _scale));
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString()))
                {
                    Ring.Items.RemoveAt(_selectedIndex);
                    _selectedIndex = -1;
                }
                ImGui.PopFont();
                DrawHelper.SetTooltip("Delete");
            }

            int count = Ring.Items.Count;
            if (count > 0)
            {
                ImGui.SetCursorPos(new Vector2(369 * _scale, 380 * _scale));
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.ArrowUp.ToIconString()))
                {
                    var tmp = Ring.Items[_selectedIndex];

                    // circular?
                    if (_selectedIndex == 0)
                    {
                        Ring.Items.Remove(tmp);
                        Ring.Items.Add(tmp);
                        _selectedIndex = count - 1;
                    }
                    else
                    {
                        Ring.Items[_selectedIndex] = Ring.Items[_selectedIndex - 1];
                        Ring.Items[_selectedIndex - 1] = tmp;
                        _selectedIndex--;
                    }
                }
                ImGui.PopFont();
                DrawHelper.SetTooltip("Move up");
            }

            if (count > 0)
            {
                ImGui.SetCursorPos(new Vector2(369 * _scale, 410 * _scale));
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.ArrowDown.ToIconString()))
                {
                    var tmp = Ring.Items[_selectedIndex];

                    // circular?
                    if (_selectedIndex == count - 1)
                    {
                        Ring.Items.Remove(tmp);
                        Ring.Items.Insert(0, tmp);
                        _selectedIndex = 0;
                    }
                    else
                    {
                        Ring.Items[_selectedIndex] = Ring.Items[_selectedIndex + 1];
                        Ring.Items[_selectedIndex + 1] = tmp;
                        _selectedIndex++;
                    }
                }
                ImGui.PopFont();
                DrawHelper.SetTooltip("Move down");
            }

            DrawAddItemMenu();
        }

        private void DrawAddItemMenu()
        {
            if (Ring == null) return;

            ImGui.SetNextWindowSize(new(94 * _scale, 140 * _scale));

            if (ImGui.BeginPopup("##TPie_Add_Item_Menu"))
            {
                RingElement? elementToAdd = null;

                if (ImGui.Selectable("Action"))
                {
                    elementToAdd = new ActionElement();
                }

                if (ImGui.Selectable("Item"))
                {
                    elementToAdd = new ItemElement();
                }

                if (ImGui.Selectable("Gear Set"))
                {
                    elementToAdd = new GearSetElement();
                }

                if (ImGui.Selectable("Command"))
                {
                    elementToAdd = new CommandElement();
                }

                if (ImGui.Selectable("Game Macro"))
                {
                    elementToAdd = new GameMacroElement();
                }

                if (ImGui.Selectable("Nested Ring"))
                {
                    elementToAdd = new NestedRingElement();
                }

                if (elementToAdd != null)
                {
                    if (Ring.Items.Count > 0 && _selectedIndex >= 0 && _selectedIndex < Ring.Items.Count - 1)
                    {
                        Ring.Items.Insert(_selectedIndex + 1, elementToAdd);
                        _selectedIndex++;
                    }
                    else
                    {
                        Ring.Items.Add(elementToAdd);
                        _selectedIndex = Ring.Items.Count - 1;
                    }

                    ShowEditItemWindow();
                }

                ImGui.EndPopup();
            }
        }

        private void ShowEditItemWindow()
        {
            if (Ring == null || _selectedIndex < 0 || _selectedIndex >= Ring.Items.Count) return;

            RingElement element = Ring.Items[_selectedIndex];
            Plugin.ShowElementWindow(ItemWindowPos, Ring, element);
        }

        public override void OnClose()
        {
            Ring = null;
            _selectedIndex = -1;

            Settings.Save(Plugin.Settings);
        }

        private static string UserFriendlyString(string str, string? remove)
        {
            string? s = remove != null ? str.Replace(remove, "") : str;

            Regex? regex = new(@"
                    (?<=[A-Z])(?=[A-Z][a-z]) |
                    (?<=[^A-Z])(?=[A-Z]) |
                    (?<=[A-Za-z])(?=[^A-Za-z])",
                RegexOptions.IgnorePatternWhitespace);

            return regex.Replace(s, " ");
        }
    }
}
