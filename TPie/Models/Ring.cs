using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TPie.Helpers;
using TPie.Models.Items;

namespace TPie.Models
{
    public class Ring
    {
        public string Name;
        public Vector4 Color;
        public KeyBind KeyBind;
        public float Radius;
        public Vector2 ItemSize;

        public List<RingItem> Items;

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

            Items = new List<RingItem>();

            _color = ImGui.ColorConvertFloat4ToU32(Color);
            _lineColor = ImGui.ColorConvertFloat4ToU32(new Vector4(Color.X, Color.Y, Color.Z, 0.5f));
        }

        public void Draw()
        {
            //List<RingItem> validItems = Items.Where(o => o.IsValid()).ToList();
            var validItems = Items;
            int count = validItems.Count;

            if (!KeyBind.IsActive() || count == 0)
            {
                if (_center != null && _selectedIndex >= 0)
                {
                    validItems[_selectedIndex].ExecuteAction();
                    _selectedIndex = -1;
                }

                _center = null;
                return;
            }

            Vector2 margin = new Vector2(20, 20);
            Vector2 radius = new Vector2(Radius);

            Vector2 mousePos = ImGui.GetMousePos();
            Vector2 pos = mousePos - radius - margin;
            if (_center == null)
            {
                var asd = ItemsHelper.Instance.GetUsableItems();
                foreach (UsableItem item in asd)
                {
                    PluginLog.Log($"{item.Name} {item.ID} {item.HQ.ToString()} {item.Count}");
                }

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

            Vector2 center = _center!.Value;
            float r = Radius - ItemSize.Y;
            double step = (Math.PI * 2) / count;

            float distanceToCenter = (mousePos - center).Length();
            if (distanceToCenter > r)
            {
                mousePos = center + Vector2.Normalize(mousePos - center) * r;
            }

            Vector2[] itemPositions = new Vector2[count];
            Vector2[] itemSizes = new Vector2[count];

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

                float scale = distance > 200 ? 1f : Math.Clamp(2f - (distance * 2f / 200), 1f, 2f);
                itemSizes[index] = ItemSize * scale;

                index++;
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            uint color = _selectedIndex >= 0 ? _color : _lineColor;
            drawList.AddCircleFilled(center, 10, color);

            Vector2 endPos = _selectedIndex >= 0 ? itemPositions[_selectedIndex] : mousePos;
            Vector2 startPos = center + Vector2.Normalize(endPos - center) * 9.5f;
            drawList.AddLine(startPos, endPos, color, 4);

            for (int i = 0; i < Items.Count; i++)
            {
                Vector2 size = i == _selectedIndex ? ItemSize * 2f : itemSizes[i];
                Items[i].Draw(itemPositions[i], size, i == _selectedIndex, _color, drawList);
            }

            ImGui.End();
        }
    }
}
