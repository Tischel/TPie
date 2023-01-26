﻿using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.GamePad;
using TPie.Helpers;
using TPie.Models;
using TPie.Models.Elements;

namespace TPie.Config
{
    internal class SettingsWindow : Window
    {
        private Settings Settings => Plugin.Settings;
        private List<Ring> Rings => Settings.Rings;

        private string[] _fontSizes;
        private string[] _animationNames;

        private Vector2 _windowPos = Vector2.Zero;
        private Vector2 RingWindowPos => _windowPos + new Vector2(410 * _scale, 0);

        private Ring? _removingRing = null;
        private bool _applyingGlobalBorderSettings = false;

        private float _scale => ImGuiHelpers.GlobalScale;

        public SettingsWindow(string name) : base(name)
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse;
            Size = new Vector2(400, 464);

            _fontSizes = new string[40 - 13];
            for (int i = 14; i <= 40; i++)
            {
                _fontSizes[i - 14] = $"{i}";
            }

            _animationNames = new string[]
            {
                "None", "Spiral", "Sequential", "Fade"
            };
        }

        public override void OnClose()
        {
            Settings.Save(Settings);
        }

        public override void Draw()
        {
            _windowPos = ImGui.GetWindowPos();

            if (!ImGui.BeginTabBar("##TPie_Settings_TabBar"))
            {
                return;
            }

            // General
            if (ImGui.BeginTabItem("General ##TPie_Settings"))
            {
                DrawGeneralTab();
                ImGui.EndTabItem();
            }

            // Global Border Settings
            if (ImGui.BeginTabItem("Global Border Settings ##TPie_Settings"))
            {
                DrawGlobalBorderSettingsTab();
                ImGui.EndTabItem();
            }

            // Rings
            if (ImGui.BeginTabItem("Rings ##TPie_Settings"))
            {
                DrawRingsTab();
                ImGui.EndTabItem();
            }

            // donate button
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(255f / 255f, 94f / 255f, 91f / 255f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(255f / 255f, 94f / 255f, 91f / 255f, .85f));

            ImGui.SetCursorPos(new Vector2(280 * _scale, 26 * _scale));
            if (ImGui.Button("Support on Ko-fi", new Vector2(104 * _scale, 24 * _scale)))
            {
                OpenUrl("https://ko-fi.com/Tischel");
            }

            ImGui.PopStyleColor(2);

            ImGui.EndTabBar();
        }

        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                try
                {
                    // hack because of this: https://github.com/dotnet/corefx/issues/10361
                    if (RuntimeInformation.IsOSPlatform(osPlatform: OSPlatform.Windows))
                    {
                        url = url.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Process.Start("xdg-open", url);
                    }
                }
                catch (Exception e)
                {
                    PluginLog.Error("Error trying to open url: " + e.Message);
                }
            }
        }

        private void DrawGeneralTab()
        {
            // position
            ImGui.Text("Position");
            ImGui.BeginChild("##Position", new Vector2(384 * _scale, 70 * _scale), true);
            {
                if (ImGui.RadioButton("Center at Cursor", Settings.AppearAtCursor))
                {
                    Settings.AppearAtCursor = true;
                }

                if (ImGui.RadioButton("Set Position", !Settings.AppearAtCursor))
                {
                    Settings.AppearAtCursor = false;
                }
                DrawHelper.SetTooltip("(0,0) is the center of the screen");

                if (!Settings.AppearAtCursor)
                {
                    ImGui.SameLine();
                    ImGui.PushItemWidth(140 * _scale);
                    ImGui.DragFloat2("##Position", ref Settings.CenterPositionOffset, 0.5f, -4000, 4000);
                    DrawHelper.SetTooltip("(0,0) is the center of the screen");

                    ImGui.SameLine();
                    ImGui.Checkbox("Center Cursor", ref Settings.AutoCenterCursor);
                    DrawHelper.SetTooltip("Your cursor will automatically move to the center of the ring when activated.");
                }
            }
            ImGui.EndChild();

            // font
            ImGui.Spacing();
            ImGui.Text("Font");
            ImGui.BeginChild("##Font", new Vector2(384 * _scale, 40 * _scale), true);
            {
                ImGui.Checkbox("Use Custom Font", ref Settings.UseCustomFont);
                DrawHelper.SetTooltip("Enable to use the Expressway font that comes with TPie.\nDisable to use the system font.");

                if (Settings.UseCustomFont)
                {
                    ImGui.SameLine();
                    ImGui.Text("\t");
                    ImGui.SameLine();

                    ImGui.PushItemWidth(80 * _scale);
                    int fontIndex = Settings.FontSize - 14;
                    if (ImGui.Combo("Size", ref fontIndex, _fontSizes, _fontSizes.Length))
                    {
                        Settings.FontSize = fontIndex + 14;
                        Plugin.UiBuilder.RebuildFonts();
                    }
                }
            }
            ImGui.EndChild();

            // keybinds
            ImGui.Spacing();
            ImGui.Text("Keybinds");
            ImGui.BeginChild("##Keybinds", new Vector2(384 * _scale, 40 * _scale), true);
            {
                ImGui.Checkbox("Keybind Passthrough", ref Settings.KeybindPassthrough);
                DrawHelper.SetTooltip("When enabled, TPie wont prevent the game from receiving a key press asssigned for a ring.");
            }
            ImGui.EndChild();

            // style
            ImGui.Spacing();
            ImGui.Text("Style");
            ImGui.BeginChild("##Style", new Vector2(384 * _scale, 70 * _scale), true);
            {
                ImGui.Checkbox("Draw Rings Background", ref Settings.DrawRingBackground);

                ImGui.SameLine();
                ImGui.Checkbox("Resize Icons When Hovered", ref Settings.AnimateIconSizes);

                ImGui.Checkbox("Show Cooldowns", ref Settings.ShowCooldowns);

                ImGui.SameLine();
                ImGui.Checkbox("Show Remaining Item Count", ref Settings.ShowRemainingItemCount);
            }
            ImGui.EndChild();

            // animation
            ImGui.Spacing();
            ImGui.Text("Animation");
            ImGui.BeginChild("##Animation", new Vector2(384 * _scale, 40 * _scale), true);
            {
                ImGui.PushItemWidth(100 * _scale);
                int animIndex = (int)Settings.AnimationType;
                if (ImGui.Combo("##AnimationType", ref animIndex, _animationNames, _animationNames.Length))
                {
                    Settings.AnimationType = (RingAnimationType)animIndex;
                }

                ImGui.SameLine();
                ImGui.Text("\t");

                ImGui.PushItemWidth(80);
                ImGui.SameLine();
                ImGui.DragFloat("Duration", ref Settings.AnimationDuration, 0.1f, 0, 5);
                DrawHelper.SetTooltip("In seconds");
            }
            ImGui.EndChild();
        }

        private void DrawGlobalBorderSettingsTab()
        {
            ImGui.Text("These are the default border settings that will be");
            ImGui.Text("used when creating a new ring element.");
            ImGui.NewLine();

            ImGui.BeginChild("##GlobalBorderSettings", new Vector2(272 * _scale, 93 * _scale), true);
            {
                Settings.GlobalBorderSettings.Draw();
            }
            ImGui.EndChild();

            ImGui.NewLine();
            if (ImGui.Button("Apply to all existing elements", new Vector2(272, 30)))
            {
                _applyingGlobalBorderSettings = true;
            }

            if (_applyingGlobalBorderSettings)
            {
                var (didConfirm, didClose) = DrawHelper.DrawConfirmationModal("Apply?", "Are you sure you want to apply these border", "settings to all existing elements?", "There is no way to undo this!");

                if (didConfirm)
                {
                    foreach (Ring ring in Settings.Rings)
                    {
                        foreach (RingElement element in ring.Items)
                        {
                            element.Border = ItemBorder.GlobalBorderSettingsCopy();
                        }
                    }

                    Settings.Save(Settings);
                }

                if (didConfirm || didClose)
                {
                    _applyingGlobalBorderSettings = false;
                }
            }
        }

        private void DrawRingsTab()
        {
            // options
            ImGui.BeginChild("##Options", new Vector2(384 * _scale, 40 * _scale), true);
            {
                ImGui.SameLine();
                ImGui.Text("Create New");
                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString()))
                {
                    Ring newRing = new Ring($"Ring{Rings.Count + 1}", Vector4.One, new KeyBind(0), new GamepadBind((int)GamepadButtons.None), 150f, new Vector2(40));
                    Plugin.Settings.AddRing(newRing);
                }
                ImGui.PopFont();
                DrawHelper.SetTooltip("Adds a new empty Ring");

                ImGui.SameLine();
                ImGui.Text("\t\t\tImport");
                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.Download.ToIconString()))
                {
                    string importString = ImGui.GetClipboardText();
                    List<Ring> newRings = ImportExportHelper.ImportRings(importString);

                    foreach (Ring ring in newRings)
                    {
                        Plugin.Settings.AddRing(ring);
                    }
                }
                ImGui.PopFont();
                DrawHelper.SetTooltip("Adds new Rings by importing them from the clipboard");

                ImGui.SameLine();
                ImGui.Text("\t\t\tExport all");
                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.Upload.ToIconString()))
                {
                    string exportString = ImportExportHelper.GenerateExportString(Rings);
                    ImGui.SetClipboardText(exportString);
                }
                ImGui.PopFont();
                DrawHelper.SetTooltip("Exports all Rings to the clipboard");
            }
            ImGui.EndChild();

            var flags =
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.BordersInner |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.SizingFixedSame;

            // rings
            if (ImGui.BeginTable("##Rings_Table", 5, flags, new Vector2(384 * _scale, 354 * _scale)))
            {
                ImGui.TableSetupColumn("Color", ImGuiTableColumnFlags.WidthStretch, 8, 0);
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 25, 1);
                ImGui.TableSetupColumn("Keybind", ImGuiTableColumnFlags.WidthStretch, 29, 2);
                ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthStretch, 24, 3);
                ImGui.TableSetupColumn("Move", ImGuiTableColumnFlags.WidthStretch, 14, 4);

                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                for (int i = 0; i < Rings.Count; i++)
                {
                    Ring ring = Rings[i];

                    ImGui.PushID(i.ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);

                    // color
                    if (ImGui.TableSetColumnIndex(0))
                    {
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3 * _scale);

                        Vector3 color = new Vector3(ring.Color.X, ring.Color.Y, ring.Color.Z);
                        if (ImGui.ColorEdit3("", ref color, ImGuiColorEditFlags.NoInputs))
                        {
                            ring.Color = new Vector4(color.X, color.Y, color.Z, 1);
                        }
                    }

                    // name
                    if (ImGui.TableSetColumnIndex(1))
                    {
                        ImGui.Text(ring.Name);
                    }

                    // keybind
                    if (ImGui.TableSetColumnIndex(2))
                    {
                        if (ImGui.Button(ring.BindingDescription(), new Vector2(100, 24)))
                        {
                            Plugin.ShowKeyBindWindow(ImGui.GetMousePos(), ring);
                        }
                    }

                    // actions
                    if (ImGui.TableSetColumnIndex(3))
                    {
                        ImGui.PushFont(UiBuilder.IconFont);
                        if (ImGui.Button(FontAwesomeIcon.Pen.ToIconString()))
                        {
                            Plugin.ShowRingSettingsWindow(RingWindowPos, ring);
                        }
                        ImGui.PopFont();
                        DrawHelper.SetTooltip("Edit Elements");

                        ImGui.SameLine();
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 3 * _scale);
                        ImGui.PushFont(UiBuilder.IconFont);
                        if (ImGui.Button(FontAwesomeIcon.Upload.ToIconString()))
                        {
                            string exportString = ImportExportHelper.GenerateExportString(ring);
                            ImGui.SetClipboardText(exportString);
                        }
                        ImGui.PopFont();
                        DrawHelper.SetTooltip("Export to clipboard");

                        ImGui.SameLine();
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 3 * _scale);
                        ImGui.PushFont(UiBuilder.IconFont);
                        if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString()))
                        {
                            _removingRing = ring;
                        }
                        ImGui.PopFont();
                        DrawHelper.SetTooltip("Delete");
                    }

                    // move
                    if (ImGui.TableSetColumnIndex(4))
                    {
                        ImGui.PushFont(UiBuilder.IconFont);
                        if (ImGui.Button(FontAwesomeIcon.ArrowUp.ToIconString()))
                        {
                            Ring tmp = Rings[i];

                            // circular?
                            if (i == 0)
                            {
                                Rings.Remove(tmp);
                                Rings.Add(tmp);
                            }
                            else
                            {
                                Rings[i] = Rings[i - 1];
                                Rings[i - 1] = tmp;
                            }
                        }
                        ImGui.PopFont();
                        DrawHelper.SetTooltip("Move up");

                        ImGui.SameLine();
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 3 * _scale);
                        ImGui.PushFont(UiBuilder.IconFont);
                        if (ImGui.Button(FontAwesomeIcon.ArrowDown.ToIconString()))
                        {
                            Ring tmp = Rings[i];

                            // circular?
                            if (i == Rings.Count - 1)
                            {
                                Rings.Remove(tmp);
                                Rings.Insert(0, tmp);
                            }
                            else
                            {
                                Rings[i] = Rings[i + 1];
                                Rings[i + 1] = tmp;
                            }
                        }
                        ImGui.PopFont();
                        DrawHelper.SetTooltip("Move down");
                    }
                }

                ImGui.EndTable();
            }

            if (_removingRing != null)
            {
                var (didConfirm, didClose) = DrawHelper.DrawConfirmationModal("Delete?", $"Are you sure you want to delete \"{_removingRing.Name}\"");

                if (didConfirm)
                {
                    Rings.Remove(_removingRing);
                }

                if (didConfirm || didClose)
                {
                    _removingRing = null;
                }
            }
        }
    }
}