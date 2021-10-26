using ImGuiNET;
using System.Numerics;
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

        public override string InvalidReason()
        {
            return "";
        }

        public override string Description()
        {
            if (JobsHelper.JobNames.TryGetValue(JobID, out string? value) && value != null)
            {
                return $"{value} ({GearSetID})";
            }

            return "";
        }

        public override void Draw(Vector2 position, Vector2 size, float scale, bool selected, uint color, float alpha, ImDrawListPtr drawList)
        {
            base.Draw(position, size, scale, selected, color, alpha, drawList);

            size = size * scale;

            DrawHelper.DrawOutlinedText($"{GearSetID}", position + size / 2 - new Vector2(2 * scale), true, scale, drawList);
        }
    }
}
