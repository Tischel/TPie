using Dalamud.Game;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Lumina.Excel;
using TPie;
using static Dalamud.Plugin.Services.ITextureProvider;

namespace DelvUI.Helpers
{
    public class TexturesHelper
    {
        public static IDalamudTextureWrap? GetTexture<T>(uint rowId, bool highQuality = false, uint stackCount = 0, bool hdIcon = true) where T : ExcelRow
        {
            var sheet = Plugin.DataManager.GetExcelSheet<T>();
            return sheet == null ? null : GetTexture<T>(sheet.GetRow(rowId), highQuality, stackCount, hdIcon);
        }

        public static IDalamudTextureWrap? GetTexture<T>(dynamic? row, bool highQuality = false, uint stackCount = 0, bool hdIcon = true) where T : ExcelRow
        {
            if (row == null)
            {
                return null;
            }

            var iconId = row.Icon;
            return GetTextureFromIconId(iconId, highQuality, stackCount, hdIcon);
        }

        public static ISharedImmediateTexture GetTextureFromIconId(uint iconId, bool highQuality = false, uint stackCount = 0, bool hdIcon = true)
        {
            GameIconLookup gameIconLookup = new GameIconLookup
            {
                IconId = iconId,
                ItemHq = hdIcon,
            };

            return Plugin.TextureProvider.GetFromGameIcon(gameIconLookup);
        }

        public static ISharedImmediateTexture GetTextureFromPath(string path)
        {
            return Plugin.TextureProvider.GetFromFile(path);
        }
    }
}
