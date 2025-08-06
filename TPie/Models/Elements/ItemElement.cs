﻿using FFXIVClientStructs.FFXIV.Client.Game;
using Dalamud.Bindings.ImGui;
using ImGuiScene;
using System.Numerics;
using TPie.Helpers;

namespace TPie.Models.Elements
{
    public class ItemElement : RingElement
    {
        public uint ItemID;
        public string Name;
        public bool HQ;

        public ItemElement(uint itemId, bool hq, string name, uint iconId)
        {
            ItemID = itemId;
            Name = name;
            HQ = hq;
            IconID = iconId;
        }

        public ItemElement() : this(0, false, "", 0) { }

        public override unsafe void ExecuteAction()
        {
            uint id = HQ ? ItemID + 1000000 : ItemID;
            ItemsHelper.Instance?.Use(id);
        }

        public override bool IsValid()
        {
            UsableItem? item = ItemsHelper.Instance?.GetUsableItem(ItemID, HQ);
            return ItemID > 0 && item?.Count > 0;
        }

        public override string InvalidReason()
        {
            return "This item won't show if it's not in your inventory.";
        }

        public override string Description()
        {
            UsableItem? item = ItemsHelper.Instance?.GetUsableItem(ItemID, HQ);

            string hqString = HQ ? " (HQ)" : "";
            string countString = item?.Count > 0 && item?.IsKey == false ? $" x{item.Count}" : "";

            return $"{Name}{hqString}{countString}";
        }

        public override bool isHQ()
        {
            return HQ;
        }

        public override void Draw(Vector2 position, Vector2 size, float scale, bool selected, uint color, float alpha, bool tooltip, ImDrawListPtr drawList)
        {
            base.Draw(position, size, scale, selected, color, alpha, tooltip, drawList);

            size = size * scale;

            UsableItem? item = ItemsHelper.Instance?.GetUsableItem(ItemID, HQ);
            if (item == null) return;

            // count
            if (Plugin.Settings.ShowRemainingItemCount && item.Count > 1)
            {
                DrawHelper.DrawOutlinedText($"{item.Count}", position + size / 2 - new Vector2(2 * scale), true, scale, drawList);
            }

            // cooldown
            if (Plugin.Settings.ShowCooldowns)
            {
                DrawHelper.DrawCooldown(item.IsKey ? ActionType.KeyItem : ActionType.Item, ItemID, position, size, scale, drawList);
            }
        }
    }
}
