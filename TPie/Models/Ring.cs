using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using ImGuiNET;
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
        public float Rotation;
        public float Radius;
        public Vector2 ItemSize;

        public KeyBind KeyBind;
        private KeyBind? _tmpKeyBind; // used for nested rings

        public bool DrawLine = true;
        public bool DrawSelectionBackground = true;
        public bool ShowTooltips = false;
        public bool PreventActionOnClose = false;

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
        public int QuickActionIndex = -1;

        private RingElement? QuickActionElement
        {
            get
            {
                if (QuickActionIndex < 0 || QuickActionIndex >= Items.Count) { return null; }

                RingElement quickAction = Items[QuickActionIndex];
                if (!quickAction.IsValid()) { return null; }

                return quickAction;
            }
        }

        public bool Previewing { get; private set; } = false;

        public bool IsActive { get; private set; } = false;
        public bool HasInventoryItems { get; private set; } = false;

        private List<RingElement> _validItems = null!;
        private int _previousCount = 0;

        private Vector2? _center = null;
        private int _selectedIndex = -1;
        private double _selectionStartTime = -1;
        private bool _quickActionSelected = false;
        private bool _canExecuteAction = true;

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

        public bool Update()
        {
            HasInventoryItems = Items.FirstOrDefault(item => item is ItemElement) != null;
            _validItems = Items.Where(o => o.IsValid() && o != QuickActionElement).ToList();

            if (_previousCount != _validItems.Count)
            {
                SetAnimState(_animState);
                _previousCount = _validItems.Count;
            }

            if (Previewing)
            {
                return true;
            }

            KeyBind currentKeyBind = CurrentKeybind();

            // click to select in toggle mode
            if (!Previewing &&
                currentKeyBind.Toggle &&
                ImGui.GetIO().MouseClicked[0] &&
                ((_selectedIndex >= 0 && _selectedIndex < _validItems.Count) || _quickActionSelected))
            {
                _canExecuteAction = true;
                currentKeyBind.Deactivate();
            }

            if (!currentKeyBind.IsActive())
            {
                IsActive = false;
                return false;
            }

            _canExecuteAction = !KeyBind.Toggle || !PreventActionOnClose;

            IsActive = _validItems.Count > 0;
            return IsActive;
        }

        public void Draw(string id)
        {
            if (!Previewing && CheckNestedRingSelection())
            {
                return;
            }

            if (!Previewing && !IsActive)
            {
                if (_canExecuteAction)
                {
                    if (_animState == AnimationState.Opened &&
                        _center != null &&
                        _selectedIndex >= 0 &&
                        _validItems != null &&
                        _selectedIndex < _validItems.Count)
                    {
                        _validItems[_selectedIndex].ExecuteAction();
                    }
                    else if ((_animState == AnimationState.Opened || _animState == AnimationState.Opening) &&
                        _center != null &&
                        _quickActionSelected)
                    {
                        QuickActionElement?.ExecuteAction();
                    }
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

                    // move cursor?
                    if (!Plugin.Settings.AppearAtCursor && Plugin.Settings.AutoCenterCursor)
                    {
                        CursorHelper.SetCursorPosition(_center.Value);
                    }
                }

                SetAnimState(AnimationState.Opening);
            }

            // animate
            UpdateAnimation();

            if (_animState == AnimationState.Closed) return;

            int count = _validItems?.Count ?? 0;
            Vector2 center = _center!.Value;
            Vector2 margin = new Vector2(400, 400);
            Vector2 radius = new Vector2(Radius);
            Vector2 pos = ValidatedPosition(center - radius - margin);
            Vector2 size = ValidatedSize(pos, radius * 2 + margin * 2);

            // create window
            ImGuiHelpers.ForceNextWindowMainViewport();

            ImGui.SetNextWindowPos(pos, Previewing ? ImGuiCond.Always : ImGuiCond.Appearing);
            ImGui.SetNextWindowSize(radius * 2 + margin * 2, ImGuiCond.Always);
            ImGui.SetNextWindowBgAlpha(0);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);

            ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;
            if (Previewing || (!Plugin.Settings.EnableQuickSettings && !CurrentKeybind().Toggle))
            {
                flags |= ImGuiWindowFlags.NoInputs;
            }

            if (!ImGui.Begin($"TPie_{id}", flags))
            {
                ImGui.End();
                ImGui.PopStyleVar();
                return;
            }

            // quick settings
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Right))
            {
                Plugin.ShowRingSettingsWindowInCursor(this);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            // bg
            if (Plugin.Settings.DrawRingBackground)
            {
                IDalamudTextureWrap? bg = Plugin.RingBackground?.GetWrapOrDefault();
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

            float rotation = (float)(Rotation * (Math.PI / 180f));
            float minDistance = float.MaxValue;

            int previousSelection = _selectedIndex;
            _selectedIndex = -1;

            int index = 0;

            for (double a = 0; a < Math.PI * 2; a += step)
            {
                if (index >= count) break;

                double angle = a + _angleOffset + rotation;
                float d = r * _itemsDistanceScales[index];
                double x = center.X + d * Math.Cos(angle);
                double y = center.Y + d * Math.Sin(angle);

                itemPositions[index] = new Vector2((float)x, (float)y);

                if (_animState == AnimationState.Opened)
                {
                    float distance = (itemPositions[index] - mousePos).Length();
                    if (distance < minDistance)
                    {
                        bool selected = distance <= ItemSize.Y * 2;
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

            // center and line
            uint color = !Previewing && _selectedIndex >= 0 ? _baseColor : _lineColor;
            if (DrawLine)
            {
                drawList.AddCircleFilled(center, 10, color);

                if (!Previewing && _animState == AnimationState.Opened)
                {
                    Vector2 endPos = _selectedIndex >= 0 ? itemPositions[_selectedIndex] : mousePos;
                    Vector2 direction = Vector2.Normalize(endPos - center);
                    Vector2 startPos = center + direction * 9.5f;
                    drawList.AddLine(startPos, endPos, color, 4);

                    Vector2 endCircleCenter = endPos + direction * 3;
                    drawList.AddCircleFilled(endCircleCenter, 4, color);
                }
            }

            // items
            if (_validItems != null)
            {
                for (int i = 0; i < count; i++)
                {
                    bool selected =
                        DrawSelectionBackground && !Previewing &&
                        _animState == AnimationState.Opened && i == _selectedIndex;

                    float scale = !Previewing && Plugin.Settings.AnimateIconSizes ? (selected ? 2f : itemScales[i]) : 1f;

                    _validItems[i].Draw(itemPositions[i], ItemSize, scale, selected, _baseColor, _itemsAlpha[i], ShowTooltips, drawList);
                }
            }

            // quick action
            if (QuickActionElement != null)
            {
                _quickActionSelected = DrawSelectionBackground && _selectedIndex == -1 && !Previewing && distanceToCenter <= ItemSize.Y * 2;
                float alpha = _itemsAlpha.Length > 0 ? _itemsAlpha[0] : 1f;
                uint selectionColor = alpha >= 1f ? _baseColor : 0;
                float scale = !Previewing && Plugin.Settings.AnimateIconSizes && itemScales.Length > 0 && _quickActionSelected ? 2f : 1f;

                QuickActionElement.Draw(center, ItemSize, scale, _quickActionSelected, selectionColor, alpha, ShowTooltips, drawList);
            }

            if (previousSelection != _selectedIndex && _selectedIndex >= 0)
            {
                _selectionStartTime = ImGui.GetTime();
            }

            ImGui.End();
            ImGui.PopStyleVar();
        }

        private Vector2 ValidatedPosition(Vector2 pos)
        {
            Vector2 screenSize = ImGui.GetMainViewport().Size;
            return new Vector2(Math.Max(0, pos.X), Math.Min(screenSize.Y, pos.Y));
        }

        private Vector2 ValidatedSize(Vector2 pos, Vector2 size)
        {
            Vector2 endPos = ValidatedPosition(pos + size);
            return endPos - pos;
        }

        private bool CheckNestedRingSelection()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _validItems.Count || _selectionStartTime == -1)
            {
                return false;
            }

            NestedRingElement? nestedRing = _validItems[_selectedIndex] as NestedRingElement;
            if (nestedRing == null || nestedRing.GetRing() == null)
            {
                return false;
            }

            double now = ImGui.GetTime();
            if (now - _selectionStartTime < nestedRing.ActivationTime)
            {
                return false;
            }

            Ring? ring = nestedRing.GetRing();
            if (ring != null)
            {
                ring.SetTemporalKeybind(CurrentKeybind());
                Plugin.RingsManager?.ForceRing(ring);
                _selectionStartTime = -1;

                if (nestedRing.KeepCenter && _center.HasValue)
                {
                    CursorHelper.SetCursorPosition(_center.Value);
                }
            }

            return true;
        }

        #region keybind
        public void SetTemporalKeybind(KeyBind? keybind)
        {
            _tmpKeyBind = keybind;
        }

        private KeyBind CurrentKeybind()
        {
            return _tmpKeyBind ?? KeyBind;
        }
        #endregion

        #region anim

        public bool IsClosed()
        {
            return _animState == AnimationState.Closed;
        }

        public void ForceClose()
        {
            if (!Previewing && _animState != AnimationState.Closed && _animState != AnimationState.Closing)
            {
                SetAnimState(AnimationState.Closed);
            }
        }

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
            int count = _validItems?.Count ?? 0;

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
                _angleOffset = -1.6f * (1 - _animProgress);

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
