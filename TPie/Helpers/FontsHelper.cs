using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.IO;
using static Dalamud.Interface.Utility.Raii.ImRaii;

namespace TPie.Helpers
{
    internal static class FontsHelper
    {
        public static bool DefaultFontBuilt { get; private set; }
        public static IFontHandle? DefaultFont { get; private set; } = null!;

        private static bool _fontPushed = false;

        public static void ClearFont()
        {
            DefaultFont?.Dispose();
            DefaultFont = null;
        }

        public static unsafe void LoadFont()
        {
            DefaultFontBuilt = false;

            string path = Path.Combine(Path.GetDirectoryName(Plugin.AssemblyLocation) ?? "", "Media", "Expressway.ttf");

            try
            {
                DefaultFont = Plugin.UiBuilder.FontAtlas.NewDelegateFontHandle
                (
                    e => e.OnPreBuild
                    (
                        tk => tk.AddFontFromFile
                        (
                            path,
                            new SafeFontConfig
                            {
                                SizePx = Plugin.Settings.FontSize
                            }
                        )
                    )
                );

                DefaultFontBuilt = true;
            }
            catch (Exception e)
            {
                Plugin.Logger.Error("Font failed to load: " + e.Message);
            }
        }

        public static void PushFont(float scale)
        {
            _fontPushed = false;
            ImGui.SetWindowFontScale(scale);

            if (!DefaultFontBuilt || !Plugin.Settings.UseCustomFont)
            {
                return;
            }

            DefaultFont?.Push();
            _fontPushed = true;            
        }

        public static void PopFont()
        {
            ImGui.SetWindowFontScale(1);

            if (_fontPushed)
            {
                DefaultFont?.Pop();
                _fontPushed = false;
            }
        }
    }
}
