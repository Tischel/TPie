using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TPie.Helpers
{
    public static class DrawHelper
    {
        public static void DrawIcon(uint iconId, Vector2 position, Vector2 size, ImDrawListPtr drawList)
        {
            TextureWrap? texture = TexturesCache.Instance.GetTextureFromIconId(iconId);
            if (texture == null) return;

            drawList.AddImage(texture.ImGuiHandle, position, position + size, Vector2.Zero, Vector2.One);
        }
    }
}
