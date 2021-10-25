using Dalamud.Logging;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TPie.Helpers;
using TPie.Models.Elements;

namespace TPie.Models
{
    public class Ring
    {
        public string Name;
        public Vector4 Color;
        public KeyBind KeyBind;
        public float Radius;
        public Vector2 ItemSize;

        public List<RingElement> Items;

        public bool IsActive { get; private set; } = false;
        public bool HasInventoryItems { get; private set; } = false;

        private List<RingElement> _validItems = null!;
        private Vector2? _center = null;
        private int _selectedIndex = -1;
        public uint _color;
        public uint _lineColor;

        public Ring(string name, Vector4 color, KeyBind keyBind, float radius, Vector2 itemSize)
        {
            Name = name;
            Color = color;
            KeyBind = keyBind;
            Radius = radius;
            ItemSize = itemSize;

            Items = new List<RingElement>();

            _color = ImGui.ColorConvertFloat4ToU32(Color);
            _lineColor = ImGui.ColorConvertFloat4ToU32(new Vector4(Color.X, Color.Y, Color.Z, 0.5f));
        }

        public void Update()
        {
            if (!KeyBind.IsActive())
            {
                IsActive = false;
                return;
            }

            _validItems = Items.Where(o => o.IsValid()).ToList();
            IsActive = _validItems.Count > 0;

            HasInventoryItems = _validItems.FirstOrDefault(item => item is ItemElement) != null;
        }

        public void Draw()
        {
            if (!IsActive)
            {
                if (_center != null && _selectedIndex >= 0 && _selectedIndex < _validItems.Count)
                {
                    _validItems[_selectedIndex].ExecuteAction();

                }

                _selectedIndex = -1;
                _center = null;
                return;
            }

            int count = _validItems.Count;

            Vector2 margin = new Vector2(20, 20);
            Vector2 radius = new Vector2(Radius);

            Vector2 mousePos = ImGui.GetMousePos();
            Vector2 pos = mousePos - radius - margin;
            if (_center == null)
            {
                _center = mousePos;
            }

            ImGui.SetNextWindowPos(pos, ImGuiCond.Appearing);
            ImGui.SetNextWindowSize(radius * 2 + margin * 2, ImGuiCond.Always);
            ImGui.SetNextWindowBgAlpha(0);

            if (!ImGui.Begin($"TPie_{Name}", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoInputs))
            {
                ImGui.End();
                return;
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            Vector2 center = _center!.Value;

            // bg
            if (Plugin.Settings.DrawRingBackground)
            {
                TextureWrap? bg = TexturesCache.Instance?.RingBackground;
                if (bg != null)
                {
                    Vector2 bgSize = new Vector2(Radius * 1.2f);
                    drawList.AddImage(bg.ImGuiHandle, center - bgSize, center + bgSize, Vector2.Zero, Vector2.One);
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

                double x = center.X + r * Math.Cos(a);
                double y = center.Y + r * Math.Sin(a);

                itemPositions[index] = new Vector2((float)x, (float)y);

                float distance = (itemPositions[index] - mousePos).Length();
                if (distance < minDistance)
                {
                    bool selected = distance <= ItemSize.Y * 0.75f;
                    _selectedIndex = selected ? index : _selectedIndex;
                    minDistance = selected ? distance : minDistance;
                }

                itemScales[index] = distance > 200 ? 1f : Math.Clamp(2f - (distance * 2f / 200), 1f, 2f);

                index++;
            }

            uint color = _selectedIndex >= 0 ? _color : _lineColor;
            drawList.AddCircleFilled(center, 10, color);

            Vector2 endPos = _selectedIndex >= 0 ? itemPositions[_selectedIndex] : mousePos;
            Vector2 startPos = center + Vector2.Normalize(endPos - center) * 9.5f;
            drawList.AddLine(startPos, endPos, color, 4);

            for (int i = 0; i < count; i++)
            {
                float scale = i == _selectedIndex ? 2f : itemScales[i];
                Items[i].Draw(itemPositions[i], ItemSize, scale, i == _selectedIndex, _color, drawList);
            }

            ImGui.End();
        }
    }
}
