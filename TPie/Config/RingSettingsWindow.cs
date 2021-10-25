using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TPie.Helpers;
using TPie.Models;
using TPie.Models.Elements;

namespace TPie.Config
{
    public class RingSettingsWindow : Window
    {
        public Ring? Ring = null;

        private bool _preview = true;
        private int _selectedIndex = -1;

        public RingSettingsWindow(string name) : base(name)
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse;
            Size = new Vector2(400, 394);
        }

        public override void Draw()
        {
            if (Ring == null) return;

            // ring preview
            if (_preview)
            {
                Vector2 windowPos = ImGui.GetWindowPos();
                Vector2 margin = new Vector2(20);
                Vector2 ringCenter = windowPos + new Vector2(Size!.Value.X + Ring.Radius + margin.X, Size!.Value.Y / 2f);
                Ring.Preview(ringCenter);
            }

            // info
            ImGui.Text("Info");
            ImGui.BeginChild("##Ring_Info", new Vector2(384, 120), true);
            {
                ImGui.PushItemWidth(320);
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

            if (ImGui.BeginTable("##Item_Table", 3, flags, new Vector2(384, 210)))
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
                        ImGui.Text(item.Description());
                    }
                }

                ImGui.EndTable();
            }

        }

        public override void OnClose()
        {
            if (_preview)
            {
                Ring?.EndPreview();
            }

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
