using ImGuiNET;
using System.Linq;
using System.Numerics;
using TPie.Helpers;

namespace TPie.Models.Elements
{
    public class NestedRingElement : RingElement
    {
        public string RingName;
        public float ActivationTime;
        public bool DrawText;
        public bool KeepCenter;
        public bool ClickToActivate;

        public NestedRingElement(string ringName, float activationTime, bool drawText, bool keepCenter, bool clickToActivate, uint iconId)
        {
            RingName = ringName;
            ActivationTime = activationTime;
            DrawText = drawText;
            KeepCenter = keepCenter;
            ClickToActivate = clickToActivate;
            IconID = iconId;
        }

        public NestedRingElement() : this("Ring Name", 0.5f, true, true, false, 66319) { }

        public override string Description()
        {
            return RingName;
        }

        public override void ExecuteAction()
        {
        }

        public override string InvalidReason()
        {
            return $"Couldn't find a ring named \"{RingName}\"";
        }

        public override bool IsValid()
        {
            return GetRing() != null;
        }

        public Ring? GetRing() => Plugin.Settings.Rings.FirstOrDefault(ring => ring.Name == RingName);

        public override void Draw(Vector2 position, Vector2 size, float scale, bool selected, uint color, float alpha, bool tooltip, ImDrawListPtr drawList)
        {
            base.Draw(position, size, scale, selected, color, alpha, tooltip, drawList);

            // name
            if (DrawText)
            {
                DrawHelper.DrawOutlinedText($"{RingName}", position, true, scale, drawList);
            }
        }
    }
}
