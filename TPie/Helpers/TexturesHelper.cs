using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Lumina.Excel;
using TPie;

namespace DelvUI.Helpers
{
    public class TexturesHelper
    {
        public static IDalamudTextureWrap? GetTexture<T>(uint rowId, bool highQuality = false, uint stackCount = 0, bool hdIcon = true) where T : struct, IExcelRow<T>
        {
            var sheet = Plugin.DataManager.GetExcelSheet<T>();
            return sheet == null ? null : GetTexture<T>(sheet.GetRow(rowId), highQuality, stackCount, hdIcon);
        }

        public static IDalamudTextureWrap? GetTexture<T>(dynamic? row, bool highQuality = false, uint stackCount = 0, bool hdIcon = true) where T : struct, IExcelRow<T>
        {
            if (row == null)
            {
                return null;
            }

            var iconId = row.Icon;
            return GetTextureFromIconId(iconId, highQuality, stackCount, hdIcon);
        }

        public static ISharedImmediateTexture? GetTextureFromIconId(uint iconId, bool highQuality = false, uint stackCount = 0, bool hdIcon = true)
        {
            try
            {
                GameIconLookup gameIconLookup = new GameIconLookup
                {
                    IconId = iconId,
                    ItemHq = hdIcon,
                };

                return Plugin.TextureProvider.GetFromGameIcon(gameIconLookup);

            }
            catch
            {
                return null;
            }
        }

        public static ISharedImmediateTexture? GetTextureFromPath(string path)
        {
            try
            {
                return Plugin.TextureProvider.GetFromFile(path);
            }
            catch
            {
                return null;
            }
        }
    }
}
