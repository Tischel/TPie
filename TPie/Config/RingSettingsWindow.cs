using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using System;
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
                _ring?.EndPreview();
                _ring = value;
            }
        }

        private int _selectedIndex = -1;

        private Vector2 _windowPos = Vector2.Zero;
        private Vector2 ItemWindowPos => _windowPos + new Vector2(410, 0);

        public RingSettingsWindow(string name) : base(name)
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse;
            Size = new Vector2(400, 394);

            PositionCondition = ImGuiCond.Appearing;
        }

        public override void Draw()
        {
            if (Ring == null) return;

            _windowPos = ImGui.GetWindowPos();

            // ring preview
            Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 margin = new Vector2(20);
            Vector2 ringCenter = windowPos + new Vector2(Size!.Value.X + Ring.Radius + margin.X, Size!.Value.Y / 2f);
            Ring.Preview(ringCenter);

            // info
            ImGui.BeginChild("##Ring_Info", new Vector2(384, 148), true);
            {
                ImGui.PushItemWidth(310);
                ImGui.InputText("Name", ref Ring.Name, 64);

                Vector3 color = new Vector3(Ring.Color.X, Ring.Color.Y, Ring.Color.Z);
                if (ImGui.ColorEdit3("Color", ref color))
                {
                    Ring.Color = new Vector4(color.X, color.Y, color.Z, 1);
                }

                Ring.KeyBind.Draw(Ring.Name);
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 4);
                ImGui.Text("Keybind");

                ImGui.DragFloat("Radius", ref Ring.Radius, 1, 150, 500);

                ImGui.DragFloat2("Items Size", ref Ring.ItemSize, 1, 10, 500);
            }
            ImGui.EndChild();

            // items
            var flags =
                            ImGuiTableFlags.RowBg |
                            ImGuiTableFlags.Borders |
                            ImGuiTableFlags.BordersOuter |
                            ImGuiTableFlags.BordersInner |
                            ImGuiTableFlags.ScrollY |
                            ImGuiTableFlags.SizingFixedSame;

            if (ImGui.BeginTable("##Item_Table", 3, flags, new Vector2(354, 202)))
            {
                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthStretch, 15, 0);
                ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthStretch, 7, 1);
                ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch, 78, 2);

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
                        string type = UserFriendlyString(item.GetType().Name, "Element");

                        if (ImGui.Selectable(type, _selectedIndex == i, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, 24)))
                        {
                            _selectedIndex = i;
                        }
                    }

                    // icon
                    if (ImGui.TableSetColumnIndex(1))
                    {
                        TextureWrap? texture = TexturesCache.Instance?.GetTextureFromIconId(item.IconID);
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

            ImGui.SetCursorPos(new Vector2(369, 200));
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString()))
            {
                ImGui.OpenPopup("##TPie_Add_Item_Menu");
            }
            ImGui.PopFont();
            DrawHelper.SetTooltip("Add");

            if (_selectedIndex >= 0)
            {
                ImGui.SetCursorPos(new Vector2(369, 230));
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
                ImGui.SetCursorPos(new Vector2(369, 260));
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString()))
                {
                    Ring.Items.RemoveAt(_selectedIndex);
                    _selectedIndex = -1;
                }
                ImGui.PopFont();
                DrawHelper.SetTooltip("Delete");
            }

            if (_selectedIndex > 0)
            {
                ImGui.SetCursorPos(new Vector2(369, 310));
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.ArrowUp.ToIconString()))
                {
                    var tmp = Ring.Items[_selectedIndex];
                    Ring.Items[_selectedIndex] = Ring.Items[_selectedIndex - 1];
                    Ring.Items[_selectedIndex - 1] = tmp;
                    _selectedIndex--;
                }
                ImGui.PopFont();
                DrawHelper.SetTooltip("Move up");
            }

            if (_selectedIndex >= 0 && _selectedIndex < Ring.Items.Count - 1)
            {
                ImGui.SetCursorPos(new Vector2(369, 340));
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.ArrowDown.ToIconString()))
                {
                    var tmp = Ring.Items[_selectedIndex];
                    Ring.Items[_selectedIndex] = Ring.Items[_selectedIndex + 1];
                    Ring.Items[_selectedIndex + 1] = tmp;
                    _selectedIndex++;
                }
                ImGui.PopFont();
                DrawHelper.SetTooltip("Move down");
            }

            DrawAddItemMenu();
        }

        private void DrawAddItemMenu()
        {
            ImGui.SetNextWindowSize(new(80, 80));

            if (ImGui.BeginPopup("##TPie_Add_Item_Menu"))
            {
                Action<RingElement?> callback = (actionElement) =>
                {
                    if (actionElement != null)
                    {
                        if (_selectedIndex >= 0)
                        {
                            Ring?.Items.Insert(_selectedIndex, actionElement);
                        }
                        else
                        {
                            Ring?.Items.Add(actionElement);
                        }
                    }
                };

                if (ImGui.Selectable("Action"))
                {
                    Plugin.ShowActionElementWindow(ItemWindowPos, null, callback);
                }

                if (ImGui.Selectable("Item"))
                {
                    Plugin.ShowItemElementWindow(ItemWindowPos, null, callback);
                }

                if (ImGui.Selectable("Gear Set"))
                {
                    Plugin.ShowGearSetElementWindow(ItemWindowPos, null, callback);
                }

                ImGui.EndPopup();
            }
        }

        private void ShowEditItemWindow()
        {
            if (Ring == null || _selectedIndex < 0 || _selectedIndex >= Ring.Items.Count) return;

            RingElement element = Ring.Items[_selectedIndex];

            if (element is ActionElement a)
            {
                Plugin.ShowActionElementWindow(ItemWindowPos, a, null);
            }
            else if (element is ItemElement i)
            {
                Plugin.ShowItemElementWindow(ItemWindowPos, i, null);
            }
            else if (element is GearSetElement g)
            {
                Plugin.ShowGearSetElementWindow(ItemWindowPos, g, null);
            }
        }

        public override void OnClose()
        {
            Ring = null;

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
