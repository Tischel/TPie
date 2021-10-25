using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TPie.Helpers;

namespace TPie.Models.Elements
{
    public class GearSetElement : RingElement
    {
        public readonly uint GearSetID;
        public readonly uint JobID;

        public GearSetElement(uint gearSetId, uint jobId)
        {
            GearSetID = gearSetId;
            JobID = jobId;
            IconID = 62800 + jobId;
        }

        public override void ExecuteAction()
        {
            ChatHelper.SendChatMessage($"/gs change {GearSetID}");
        }

        public override bool IsValid()
        {
            return true;
        }

        public override void Draw(Vector2 position, Vector2 size, float scale, bool selected, uint color, float alpha, ImDrawListPtr drawList)
        {
            base.Draw(position, size, scale, selected, color, alpha, drawList);

            size = size * scale;

            DrawHelper.DrawOutlinedText($"{GearSetID}", position + size / 2 - new Vector2(2 * scale), true, scale, drawList);
        }
    }
}
