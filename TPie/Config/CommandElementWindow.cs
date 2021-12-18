using Dalamud.Interface;
using ImGuiNET;
using ImGuiScene;
using System.Numerics;
using System.Text.RegularExpressions;
using TPie.Helpers;
using TPie.Models.Elements;

namespace TPie.Config
{
    public class CommandElementWindow : RingElementWindow
    {
        private CommandElement? _commandElement = null;
        public CommandElement? CommandElement
        {
            get => _commandElement;
            set
            {
                _commandElement = value;

                _inputText = value != null ? value.Name : "";
                _commandInputText = value != null ? value.Command : "";
                _iconInputText = value != null ? $"{value.IconID}" : "";
            }
        }

        protected override RingElement? Element
        {
            get => CommandElement;
            set => CommandElement = value is CommandElement o ? o : null;
        }

        protected string _commandInputText = "";
        protected string _iconInputText = "";

        public CommandElementWindow(string name) : base(name)
        {

        }

        public override void Draw()
        {
            if (CommandElement == null) return;

            ImGui.PushItemWidth(210 * _scale);

            // name
            FocusIfNeeded();
            if (ImGui.InputText("Name ##Command", ref _inputText, 100))
            {
                CommandElement.Name = _inputText;
            }

            // command
            if (ImGui.InputText("Command ##Command", ref _commandInputText, 100))
            {
                CommandElement.Command = _commandInputText;
            }

            ImGui.NewLine();
            ImGui.NewLine();

            // icon id
            ImGui.PushItemWidth(154 * _scale);
            string str = _iconInputText;
            if (ImGui.InputText("Icon ID ##Command", ref str, 100, ImGuiInputTextFlags.CharsDecimal))
            {
                _iconInputText = Regex.Replace(str, @"[^\d]", "");

                try
                {
                    CommandElement.IconID = uint.Parse(_iconInputText);
                }
                catch
                {
                    CommandElement.IconID = 0;
                }
            }

            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button("\uf2f9"))
            {
                CommandElement.IconID = 66001;
                _iconInputText = "66001";
            }
            ImGui.PopFont();
            DrawHelper.SetTooltip("Reset to default");

            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button(FontAwesomeIcon.Search.ToIconString()))
            {
                Plugin.ShowIconBrowserWindow(CommandElement.IconID, (iconId) =>
                {
                    CommandElement.IconID = iconId;
                    _iconInputText = $"{iconId}";
                });
            }
            ImGui.PopFont();
            DrawHelper.SetTooltip("Search ");

            ImGui.NewLine();

            // icon
            if (CommandElement.IconID > 0)
            {
                TextureWrap? texture = Plugin.TexturesCache.GetTextureFromIconId(CommandElement.IconID);
                if (texture != null)
                {
                    ImGui.SetCursorPosX(110 * _scale);
                    ImGui.Image(texture.ImGuiHandle, new Vector2(80 * _scale));
                }
            }

            // border
            ImGui.NewLine();
            CommandElement.Border.Draw();
        }
    }
}
