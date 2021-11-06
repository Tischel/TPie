using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;
using TPie.Models;
using TPie.Models.Elements;

namespace TPie.Config
{
    public abstract class RingElementWindow : Window
    {
        public Ring? Ring = null;

        protected abstract RingElement? Element { get; set; }

        protected string _inputText = "";
        protected bool _needsFocus = false;
        protected float _scale => ImGuiHelpers.GlobalScale;

        public RingElementWindow(string name) : base(name)
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse;
            Size = new Vector2(300, 394);

            PositionCondition = ImGuiCond.Appearing;
        }

        public override void OnOpen()
        {
            _needsFocus = true;
        }

        public override void OnClose()
        {
            Element = null;
        }

        protected void FocusIfNeeded()
        {
            if (_needsFocus)
            {
                ImGui.SetKeyboardFocusHere(0);
                _needsFocus = false;
            }
        }

        public override void PreDraw()
        {
            if (Ring != null)
            {
                Settings settings = Plugin.Settings;
                if (!settings.Rings.Contains(Ring))
                {
                    IsOpen = false;
                    return;
                }

                if (Element != null && !Ring.Items.Contains(Element))
                {
                    IsOpen = false;
                    return;
                }
            }
        }
    }
}
