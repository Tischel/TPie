using Dalamud.Bindings.ImGui;
using System.Numerics;
using TPie.Helpers;

namespace TPie.Models.Elements
{
    public class CommandElement : RingElement
    {
        public string Name;
        public string Command;
        public bool DrawText;
        public bool DrawTextOnlyWhenSelected;

        public CommandElement(string name, string command, bool drawText, bool drawTextOnlyWhenSelected, uint iconId)
        {
            Name = name;
            Command = command;
            DrawText = drawText;
            DrawTextOnlyWhenSelected = drawTextOnlyWhenSelected;
            IconID = iconId;
        }

        public CommandElement() : this("New Command", "", true, false, 66001) { }

        public override string Description()
        {
            return $"{Name} ({Command})";
        }

        public override void ExecuteAction()
        {
            ChatHelper.SendChatMessage(Command);
        }

        public override string InvalidReason()
        {
            return "Command format invalid";
        }

        public override bool IsValid()
        {
            return Command.StartsWith("/");
        }

        public override void Draw(Vector2 position, Vector2 size, float scale, bool selected, uint color, float alpha, bool tooltip, ImDrawListPtr drawList)
        {
            base.Draw(position, size, scale, selected, color, alpha, tooltip, drawList);

            // name
            if (!DrawText) { return; }

            if (!DrawTextOnlyWhenSelected || (DrawTextOnlyWhenSelected && selected))
            {
                DrawHelper.DrawOutlinedText($"{Name}", position, true, scale, drawList);
            }
        }
    }
}
