using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Bindings.ImGui;
using System.Numerics;
using System.Runtime.CompilerServices;
using TPie.Helpers;
using Macro = FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureMacroModule.Macro;

namespace TPie.Models.Elements
{
    public class GameMacroElement : RingElement
    {
        public string Name;
        public int Identifier;
        public bool IsShared;
        public bool DrawText;
        public bool DrawTextOnlyWhenSelected;

        public GameMacroElement(string name, int id, bool shared, bool drawText, bool drawTextOnlyWhenSelected, uint iconId)
        {
            Name = name;
            Identifier = id;
            IsShared = shared;
            DrawText = drawText;
            DrawTextOnlyWhenSelected = drawTextOnlyWhenSelected;
            IconID = iconId;
        }

        public GameMacroElement() : this("New Macro", 0, false, true, false, 66001) { }

        public override string Description()
        {
            string sharedString = IsShared ? "Shared " : "";

            return $"{Name} ({sharedString}Macro #{Identifier})";
        }

        private unsafe Macro* GetGameMacro()
        {
            if (Identifier < 0 || Identifier > 99) return null;

            uint set = (uint)(IsShared ? 1 : 0);
            return RaptureMacroModule.Instance()->GetMacro(set, (uint)Identifier);
        }

        public override unsafe void ExecuteAction()
        {
            // already executing macro?
            if (RaptureShellModule.Instance()->MacroLocked || RaptureShellModule.Instance()->MacroCurrentLine >= 0) return;

            Macro* macro = GetGameMacro();
            if (macro != null)
            {
                RaptureShellModule.Instance()->ExecuteMacro(macro);
            }
        }

        public override string InvalidReason()
        {
            return "Invalid macro";
        }

        public override unsafe bool IsValid()
        {
            return GetGameMacro() != null;
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
