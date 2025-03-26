using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace TPie.Helpers
{
    internal class ItemsHelper
    {
        #region Singleton
        private ItemsHelper()
        {
            ExcelSheet<Item>? itemsSheet = Plugin.DataManager.GetExcelSheet<Item>();
            List<Item> validItems = itemsSheet?.Where(item => item.ItemAction.RowId > 0).ToList() ?? new List<Item>();
            _usableItems = validItems.ToDictionary(item => item.RowId);

            ExcelSheet<EventItem>? eventItemsSheet = Plugin.DataManager.GetExcelSheet<EventItem>();
            List<EventItem> validEventItems = eventItemsSheet?.Where(item => item.Action.RowId > 0).ToList() ?? new List<EventItem>();
            _usableEventItems = validEventItems.ToDictionary(item => item.RowId);
        }

        public static void Initialize() { Instance = new ItemsHelper(); }

        public static ItemsHelper Instance { get; private set; } = null!;

        ~ItemsHelper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Instance = null!;
        }
        #endregion

        private Dictionary<uint, Item> _usableItems;
        private Dictionary<uint, EventItem> _usableEventItems;

        private Dictionary<string, UsableItem> UsableItems = new Dictionary<string, UsableItem>();

        public unsafe void CalculateUsableItems()
        {
            InventoryManager* manager = InventoryManager.Instance();
            InventoryType[] inventoryTypes = new InventoryType[]
            {
                InventoryType.Inventory1,
                InventoryType.Inventory2,
                InventoryType.Inventory3,
                InventoryType.Inventory4,
                InventoryType.KeyItems
            };

            UsableItems.Clear();

            try
            {
                foreach (InventoryType inventoryType in inventoryTypes)
                {
                    InventoryContainer* container = manager->GetInventoryContainer(inventoryType);
                    if (container == null) continue;

                    for (int i = 0; i < container->Size; i++)
                    {
                        try
                        {
                            InventoryItem* item = container->GetInventorySlot(i);
                            if (item == null) continue;

                            if (item->Quantity == 0) continue;

                            bool hq = (item->Flags & InventoryItem.ItemFlags.HighQuality) != 0;
                            string hqString = hq ? "_1" : "_0";
                            string key = $"{item->ItemId}{hqString}";

                            if (UsableItems.TryGetValue(key, out UsableItem? usableItem) && usableItem != null)
                            {
                                usableItem.Count += (uint)item->Quantity;
                            }
                            else
                            {
                                if (_usableItems.TryGetValue(item->ItemId, out Item itemData))
                                {
                                    UsableItems.Add(key, new UsableItem(itemData, hq, (uint)item->Quantity));
                                }
                                else if (_usableEventItems.TryGetValue(item->ItemId, out EventItem eventItemData) )
                                {
                                    UsableItems.Add(key, new UsableItem(eventItemData, hq, (uint)item->Quantity));
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        public UsableItem? GetUsableItem(uint itemId, bool hq)
        {
            string hqString = hq ? "_1" : "_0";
            string key = $"{itemId}{hqString}";

            if (UsableItems.TryGetValue(key, out UsableItem? value))
            {
                return value;
            }

            return null;
        }

        public List<UsableItem> GetUsableItems()
        {
            return UsableItems.Values.ToList();
        }

        public unsafe void Use(uint itemId)
        {
            AgentInventoryContext.Instance()->UseItem(itemId, (InventoryType)4);
        }
    }

    public class UsableItem
    {
        public readonly string Name;
        public readonly uint ID;
        public readonly bool IsHQ;
        public readonly uint IconID;
        public uint Count;
        public readonly bool IsKey;

        public UsableItem(Item item, bool hq, uint count)
        {
            Name = item.Name.ToString();
            ID = item.RowId;
            IsHQ = hq;
            IconID = item.Icon;
            Count = count;
            IsKey = false;
        }

        public UsableItem(EventItem item, bool hq, uint count)
        {
            Name = item.Name.ToString();
            ID = item.RowId;
            IsHQ = hq;
            IconID = item.Icon;
            Count = count;
            IsKey = true;
        }

        public override string ToString()
        {
            return $"UsableItem: {ID}, {Name}, {IsHQ}, {IconID}, {Count}";
        }
    }
}
