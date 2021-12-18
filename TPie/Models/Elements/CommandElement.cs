using ImGuiNET;
using System.Numerics;
using TPie.Helpers;

namespace TPie.Models.Elements
{
    public class CommandElement : RingElement
    {
        public string Name;
        public string Command;

        public CommandElement(string name, string command, uint iconId)
        {
            Name = name;
            Command = command;
            IconID = iconId;
        }

        public CommandElement() : this("New Command", "", 66001) { }

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

        public override void Draw(Vector2 position, Vector2 size, float scale, bool selected, uint color, float alpha, ImDrawListPtr drawList)
        {
            base.Draw(position, size, scale, selected, color, alpha, drawList);

            // name
            DrawHelper.DrawOutlinedText($"{Name}", position, true, scale, drawList);
        }
    }
}
