using ImGuiNET;
using ImGuiScene;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TPie.Helpers;

namespace TPie.Models.Elements
{
    public class ItemElement : RingElement
    {
        public readonly uint ItemID;
        public readonly bool HQ;

        public ItemElement(uint itemId, bool hq, uint iconId)
        {
            ItemID = itemId;
            HQ = hq;
            IconID = iconId;
        }

        public override void ExecuteAction()
        {
            uint id = HQ ? ItemID + 1000000 : ItemID;
            ItemsHelper.Instance?.Use(id);
        }

        public override bool IsValid()
        {
            return ItemID > 0;
        }

        public override void Draw(Vector2 position, Vector2 size, float scale, bool selected, uint color, float alpha, ImDrawListPtr drawList)
        {
            base.Draw(position, size, scale, selected, color, alpha, drawList);

            size = size * scale;

            if (HQ)
            {
                TextureWrap? hqIcon = TexturesCache.Instance?.HQIcon;
                if (hqIcon != null)
                {
                    Vector2 iconSize = new Vector2(16 * scale);
                    Vector2 iconPos = (position - size / 2f) + new Vector2(2);
                    uint c = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, alpha));

                    drawList.AddImage(hqIcon.ImGuiHandle, iconPos, iconPos + iconSize, Vector2.Zero, Vector2.One, c);
                }
            }
        }
    }
}
