using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;
using TPie.Models;
using TPie.Models.Elements;

namespace TPie.Config
{
    public abstract class RingElementWindow : Window
    {
        public Action<RingElement?>? Callback = null;
        public Ring? Ring = null;

        protected bool _editing = false;
        protected string _inputText = "";
        protected bool _needsFocus = false;

        public RingElementWindow(string name) : base(name)
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse;
            Size = new Vector2(300, 300);

            PositionCondition = ImGuiCond.Appearing;
        }

        public override void OnOpen()
        {
            _needsFocus = true;
        }

        public override void OnClose()
        {
            Callback?.Invoke(null);
            Callback = null;
            DestroyElement();
        }

        protected void FocusIfNeeded()
        {
            if (_needsFocus)
            {
                ImGui.SetKeyboardFocusHere(0);
                _needsFocus = false;
            }
        }

        protected abstract RingElement? Element();
        protected abstract void CreateElement();
        protected abstract void DestroyElement();

        public override void PreDraw()
        {
            RingElement? element = Element();
            if (element == null)
            {
                CreateElement();
                _editing = false;
            }

            if (Ring != null)
            {
                Settings settings = Plugin.Settings;
                if (!settings.Rings.Contains(Ring))
                {
                    IsOpen = false;
                    return;
                }


                if (_editing && element != null && !Ring.Items.Contains(element))
                {
                    IsOpen = false;
                    return;
                }
            }
        }
    }
}
