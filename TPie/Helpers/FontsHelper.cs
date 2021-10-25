using Dalamud.Logging;
using ImGuiNET;
using System;
using System.IO;

namespace TPie.Helpers
{
    internal static class FontsHelper
    {
        public static bool DefaultFontBuilt { get; private set; }
        public static ImFontPtr DefaultFont { get; private set; } = null;

        private static bool _fontPushed = false;

        public static void LoadFont()
        {
            DefaultFontBuilt = false;

            string path = Path.Combine(Path.GetDirectoryName(Plugin.AssemblyLocation) ?? "", "Media", "Expressway.ttf");

            try
            {
                DefaultFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(path, Plugin.Settings.FontSize);
                DefaultFontBuilt = true;

            }
            catch (Exception e)
            {
                PluginLog.Error("Font failed to load: " + e.Message);
            }
        }

        public static void PushFont(float scale)
        {
            if (DefaultFontBuilt)
            {
                DefaultFont.Scale = scale;
                ImGui.PushFont(DefaultFont);
                _fontPushed = true;
                return;
            }

            _fontPushed = false;
        }

        public static void PopFont()
        {
            if (_fontPushed)
            {
                ImGui.PopFont();
                DefaultFont.Scale = 1;
                _fontPushed = false;
            }
        }
    }
}
