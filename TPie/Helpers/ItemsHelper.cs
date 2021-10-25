using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TPie.Helpers
{
    public class ItemsHelper
    {
        private delegate void UseItem(IntPtr agent, uint itemId, uint unk1, uint unk2, short unk3);
        private delegate uint GetActionID(uint unk, uint itemId);

        #region Singleton
        private ItemsHelper()
        {
            _useItemPtr = Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 41 B0 01 BA 13 00 00 00");

            ExcelSheet<Item>? itemsSheet = Plugin.DataManager.GetExcelSheet<Item>();
            List<Item> validItems = itemsSheet?.Where(item => item.ItemAction.Row > 0).ToList() ?? new List<Item>();
            _usableItems = validItems.ToDictionary(item => item.RowId);

            ExcelSheet<EventItem>? eventItemsSheet = Plugin.DataManager.GetExcelSheet<EventItem>();
            List<EventItem> validEventItems = eventItemsSheet?.Where(item => item.Action.Row > 0).ToList() ?? new List<EventItem>();
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

        private IntPtr _useItemPtr = IntPtr.Zero;
        private Dictionary<uint, Item> _usableItems;
        private Dictionary<uint, EventItem> _usableEventItems;

        public unsafe List<UsableItem> GetUsableItems()
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

            List<UsableItem> usableItems = new List<UsableItem>();

            foreach (InventoryType inventoryType in inventoryTypes)
            {
                InventoryContainer* container = manager->GetInventoryContainer(inventoryType);
                if (container == null) continue;

                for (int i = 0; i < container->Size; i++)
                {
                    InventoryItem* item = container->GetInventorySlot(i);
                    if (item == null) continue;

                    if (item->Quantity == 0) continue;

                    bool hq = (item->Flags & InventoryItem.ItemFlags.HQ) != 0;

                    if (_usableItems.TryGetValue(item->ItemID, out Item? itemData) && itemData != null)
                    {
                        usableItems.Add(new UsableItem(itemData, hq, item->Quantity));
                    }
                    else if (_usableEventItems.TryGetValue(item->ItemID, out EventItem? eventItemData) && eventItemData != null)
                    {
                        usableItems.Add(new UsableItem(eventItemData, hq, item->Quantity));
                    }
                }
            }

            return usableItems;
        }

        private unsafe void Use(uint itemId)
        {
            if (_useItemPtr == IntPtr.Zero) return;

            AgentModule* agentModule = (AgentModule*)Plugin.GameGui.GetUIModule();
            IntPtr agent = (IntPtr)agentModule->GetAgentByInternalID(10);

            UseItem usetItemDelegate = Marshal.GetDelegateForFunctionPointer<UseItem>(_useItemPtr);
            usetItemDelegate(agent, itemId, 999, 0, 0);
        }
    }

    public class UsableItem
    {
        public readonly string Name;
        public readonly uint ID;
        public readonly bool HQ;
        public readonly uint IconID;
        public readonly uint Count;

        public UsableItem(Item item, bool hq, uint count)
        {
            Name = item.Name;
            ID = item.RowId;
            HQ = hq;
            IconID = item.Icon;
            Count = count;
        }

        public UsableItem(EventItem item, bool hq, uint count)
        {
            Name = item.Name;
            ID = item.RowId;
            HQ = hq;
            IconID = item.Icon;
            Count = count;
        }
    }
}
