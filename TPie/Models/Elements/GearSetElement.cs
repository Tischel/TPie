using ImGuiNET;
using Newtonsoft.Json;
using System.Numerics;
using TPie.Helpers;

namespace TPie.Models.Elements
{
    public class GearSetElement : RingElement
    {
        public uint GearSetID;

        private uint _jobId;

        [JsonProperty]
        public uint JobID
        {
            get => _jobId;
            set
            {
                _jobId = value;
                IconID = 62800 + value;
            }
        }

        public GearSetElement(uint gearSetId, uint jobId)
        {
            GearSetID = gearSetId;
            JobID = jobId;
        }

        public GearSetElement() : this(1, Plugin.ClientState.LocalPlayer?.ClassJob.Id ?? JobIDs.GLD) { }

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
