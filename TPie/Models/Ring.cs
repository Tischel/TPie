using Dalamud.Logging;
using ImGuiNET;
using ImGuiScene;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TPie.Config;
using TPie.Helpers;
using TPie.Models.Elements;

namespace TPie.Models
{
    public class Ring
    {
        public string Name;
        public KeyBind KeyBind;
        public float Radius;
        public Vector2 ItemSize;

        private Vector4 _color = Vector4.One;
        public Vector4 Color
        {
            get => _color;
            set
            {
                _color = value;
                _baseColor = ImGui.ColorConvertFloat4ToU32(value);
                _lineColor = ImGui.ColorConvertFloat4ToU32(new Vector4(value.X, value.Y, value.Z, 0.5f));
            }
        }

        private uint _baseColor;
        private uint _lineColor;

        public List<RingElement> Items;

        public bool Previewing { get; private set; } = false;

        public bool IsActive { get; private set; } = false;
        public bool HasInventoryItems { get; private set; } = false;

        private List<RingElement> _validItems = null!;
        private Vector2? _center = null;
        private int _selectedIndex = -1;

        private AnimationState _animState = AnimationState.Closed;
        private bool _animating = false;
        private double _animEndTime = -1;
        private double _animProgress = 0;
        private double _angleOffset = 0;
        private float[] _itemsDistanceScales = null!;
        private float[] _itemsAlpha = null!;

        public Ring(string name, Vector4 color, KeyBind keyBind, float radius, Vector2 itemSize)
        {
            Name = name;
            Color = color;
            KeyBind = keyBind;
            Radius = radius;
            ItemSize = itemSize;

            Items = new List<RingElement>();
        }

        public void Preview(Vector2 position)
        {
            _center = position;
            SetAnimState(AnimationState.Opened);
            Previewing = true;
        }

        public void EndPreview()
        {
            SetAnimState(AnimationState.Closed);
            Previewing = false;
        }

        public void Update()
        {
            HasInventoryItems = Items.FirstOrDefault(item => item is ItemElement) != null;
            _validItems = Items.Where(o => o.IsValid()).ToList();

            if (!KeyBind.IsActive())
            {
                IsActive = false;
                return;
            }

            IsActive = _validItems.Count > 0;
        }

        public void Draw()
        {
            if (!Previewing && !IsActive)
            {
                if (_animState == AnimationState.Opened && _center != null && _selectedIndex >= 0 && _selectedIndex < _validItems.Count)
                {
                    _validItems[_selectedIndex].ExecuteAction();
                }

                if (_animState != AnimationState.Closing && _animState != AnimationState.Closed)
                {
                    SetAnimState(AnimationState.Closing);
                }
            }

            Vector2 mousePos = ImGui.GetMousePos();

            // detect start
            if (!Previewing && IsActive && (_animState == AnimationState.Closed || _animState == AnimationState.Closing))
            {
                if (_animState == AnimationState.Closed)
                {
                    _center = Plugin.Settings.AppearAtCursor ?
                        mousePos :
                        ImGui.GetMainViewport().Size / 2f + Plugin.Settings.CenterPositionOffset;
                }

                SetAnimState(AnimationState.Opening);
            }

            // animate
            UpdateAnimation();

            if (_animState == AnimationState.Closed) return;

            int count = _validItems.Count;
            Vector2 center = _center!.Value;
            Vector2 margin = new Vector2(40, 40);
            Vector2 radius = new Vector2(Radius);
            Vector2 pos = center - radius - margin;

            // create window
            ImGui.SetNextWindowPos(pos, Previewing ? ImGuiCond.Always : ImGuiCond.Appearing);
            ImGui.SetNextWindowSize(radius * 2 + margin * 2, ImGuiCond.Always);
            ImGui.SetNextWindowBgAlpha(0);

            if (!ImGui.Begin($"TPie_{Name}", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoInputs))
            {
                ImGui.End();
                return;
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            // bg
            if (Plugin.Settings.DrawRingBackground)
            {
                TextureWrap? bg = TexturesCache.Instance?.RingBackground;
                if (bg != null)
                {
                    Vector2 bgSize = new Vector2(Radius * 1.3f);
                    uint c = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, (float)_animProgress));
                    drawList.AddImage(bg.ImGuiHandle, center - bgSize, center + bgSize, Vector2.Zero, Vector2.One, c);
                }
            }

            // elements
            float r = Radius - ItemSize.Y;
            double step = (Math.PI * 2) / count;

            float distanceToCenter = (mousePos - center).Length();
            if (distanceToCenter > r)
            {
                mousePos = center + Vector2.Normalize(mousePos - center) * r;
            }

            Vector2[] itemPositions = new Vector2[count];
            float[] itemScales = new float[count];

            float minDistance = float.MaxValue;
            _selectedIndex = -1;

            int index = 0;

            for (double a = 0; a < Math.PI * 2; a += step)
            {
                if (index >= count) break;

                double angle = a + _angleOffset;
                float d = r * _itemsDistanceScales[index];
                double x = center.X + d * Math.Cos(angle);
                double y = center.Y + d * Math.Sin(angle);

                itemPositions[index] = new Vector2((float)x, (float)y);

                if (_animState == AnimationState.Opened)
                {
                    float distance = (itemPositions[index] - mousePos).Length();
                    if (distance < minDistance)
                    {
                        bool selected = distance <= ItemSize.Y * 0.75f;
                        _selectedIndex = selected ? index : _selectedIndex;
                        minDistance = selected ? distance : minDistance;
                    }

                    itemScales[index] = distance > 200 ? 1f : Math.Clamp(2f - (distance * 2f / 200), 1f, 2f);
                }
                else
                {
                    itemScales[index] = 1f;
                }

                index++;
            }

            uint color = !Previewing && _selectedIndex >= 0 ? _baseColor : _lineColor;
            drawList.AddCircleFilled(center, 10, color);

            if (!Previewing && _animState == AnimationState.Opened)
            {
                Vector2 endPos = _selectedIndex >= 0 ? itemPositions[_selectedIndex] : mousePos;
                Vector2 direction = Vector2.Normalize(endPos - center);
                Vector2 startPos = center + direction * 9.5f;
                drawList.AddLine(startPos, endPos, color, 4);

                if (_selectedIndex == -1)
                {
                    Vector2 endCircleCenter = endPos + direction * 3;
                    drawList.AddCircleFilled(endCircleCenter, 4, color);
                }
            }

            for (int i = 0; i < count; i++)
            {
                bool selected = !Previewing && _animState == AnimationState.Opened && i == _selectedIndex;
                float scale = !Previewing && Plugin.Settings.AnimateIconSizes ? (selected ? 2f : itemScales[i]) : 1f;
                _validItems[i].Draw(itemPositions[i], ItemSize, scale, selected, _baseColor, _itemsAlpha[i], drawList);
            }

            ImGui.End();
        }

        #region anim
        private void SetAnimState(AnimationState state)
        {
            float animDuration = Plugin.Settings.AnimationDuration;
            if (Plugin.Settings.AnimationType == RingAnimationType.None || animDuration == 0)
            {
                if (state == AnimationState.Opening) { state = AnimationState.Opened; }
                else if (state == AnimationState.Closing) { state = AnimationState.Closed; }
            }

            _animState = state;

            int count = _validItems?.Count ?? 0;
            _itemsDistanceScales = new float[count];
            _itemsAlpha = new float[count];

            // opened
            if (state == AnimationState.Opened || state == AnimationState.Closed)
            {
                _animEndTime = -1;
                _animProgress = state == AnimationState.Opened ? 1 : 0;
                _animating = false;
                _angleOffset = 0;

                for (int i = 0; i < count; i++)
                {
                    _itemsDistanceScales[i] = state == AnimationState.Opened ? 1f : 0f;
                    _itemsAlpha[i] = state == AnimationState.Opened ? 1f : 0f;
                }

                if (state == AnimationState.Closed)
                {
                    _center = null;
                }
                return;
            }


            double p = state == AnimationState.Opening ? 1 - _animProgress : _animProgress;
            _animEndTime = ImGui.GetTime() + (animDuration * p);

            _animating = true;
        }

        private void UpdateAnimation()
        {
            if (!_animating) { return; }

            double now = ImGui.GetTime();
            int count = _validItems.Count;

            float animDuration = Plugin.Settings.AnimationDuration;
            if (now > _animEndTime)
            {
                _animProgress = _animState == AnimationState.Opening ? 1 : 0;
            }
            else
            {
                _animProgress = Math.Min(1, (_animEndTime - now) / animDuration);
                if (_animState == AnimationState.Opening)
                {
                    _animProgress = 1 - _animProgress;
                }
            }

            RingAnimationType type = Plugin.Settings.AnimationType;

            // spiral
            if (type == RingAnimationType.Spiral)
            {
                _angleOffset = -0.8f * (1 - _animProgress);

                for (int i = 0; i < count; i++)
                {
                    _itemsDistanceScales[i] = (float)_animProgress;
                    _itemsAlpha[i] = (float)_animProgress;
                }
            }

            // sequential
            else if (type == RingAnimationType.Sequential)
            {
                _angleOffset = 0;

                for (int i = 0; i < count; i++)
                {
                    float start = i * (1f / count);
                    float end = (i + 1) * (1f / count);
                    float duration = end - start;

                    float p = 0;
                    if (_animProgress > start && _animProgress <= end)
                    {
                        p = ((float)_animProgress - start) / duration;
                    }
                    else
                    {
                        p = _animProgress < start ? 0f : 1f;
                    }

                    _itemsDistanceScales[i] = p;
                    _itemsAlpha[i] = p;
                }
            }

            // fade
            else if (type == RingAnimationType.Fade)
            {
                _angleOffset = 0;

                for (int i = 0; i < count; i++)
                {
                    _itemsDistanceScales[i] = 1f;
                    _itemsAlpha[i] = (float)_animProgress;
                }
            }

            if (now > _animEndTime)
            {
                AnimationState state = _animState == AnimationState.Opening ? AnimationState.Opened : AnimationState.Closed;
                SetAnimState(state);
            }
        }

        private enum AnimationState
        {
            Opening = 0,
            Opened = 1,
            Closing = 2,
            Closed = 3
        }
        #endregion
    }
}
