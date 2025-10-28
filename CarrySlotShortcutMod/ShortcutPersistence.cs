using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Duckov;
using ItemStatsSystem;
using ItemStatsSystem.Items;

namespace CarrySlotShortcutMod
{
    internal static class ShortcutPersistence
    {
        private enum BindingLocation
        {
            Unknown,
            MainInventory,
            CharacterTree,
            PetInventory
        }

        private sealed class BindingRecord
        {
            internal int SlotIndex;
            internal int TypeId;
            internal BindingLocation Location;
            internal int InventoryIndex;
            internal readonly List<int> ParentTypePath = new List<int>();
        }

        private static readonly Dictionary<int, BindingRecord> Records = new Dictionary<int, BindingRecord>();
        private static readonly FieldInfo OnSetItemField = typeof(ItemShortcut).GetField("OnSetItem", BindingFlags.Static | BindingFlags.NonPublic);

        internal static void ClearAll()
        {
            Records.Clear();
        }

        internal static void Forget(int slotIndex)
        {
            Records.Remove(slotIndex);
        }

        internal static void Remember(int slotIndex, Item item)
        {
            if (item == null)
            {
                Forget(slotIndex);
                return;
            }

            BindingRecord record = BuildRecord(slotIndex, item);
            if (record == null || record.Location == BindingLocation.MainInventory || record.Location == BindingLocation.Unknown)
            {
                Forget(slotIndex);
                return;
            }

            Records[slotIndex] = record;
        }

        internal static Item TryResolve(ItemShortcut shortcut, int slotIndex, int typeId)
        {
            if (shortcut == null || slotIndex < 0 || typeId < 0)
            {
                return null;
            }

            if (!Records.TryGetValue(slotIndex, out BindingRecord record) || record.TypeId != typeId)
            {
                return null;
            }

            Item resolved = ResolveRecord(record);
            if (resolved == null)
            {
                Records.Remove(slotIndex);
            }

            return resolved;
        }

        internal static void RefreshAll(ItemShortcut shortcut)
        {
            if (shortcut == null || Records.Count == 0)
            {
                return;
            }

            Action<int> onSetItem = OnSetItemField?.GetValue(null) as Action<int>;

            foreach (KeyValuePair<int, BindingRecord> kvp in Records.ToArray())
            {
                BindingRecord record = kvp.Value;
                Item resolved = ResolveRecord(record);
                if (resolved == null)
                {
                    Records.Remove(record.SlotIndex);
                    continue;
                }

                EnsureListSize(shortcut.items, record.SlotIndex);
                shortcut.items[record.SlotIndex] = resolved;

                EnsureListSize(shortcut.itemTypes, record.SlotIndex);
                shortcut.itemTypes[record.SlotIndex] = resolved.TypeID;

                onSetItem?.Invoke(record.SlotIndex);
            }
        }

        private static BindingRecord BuildRecord(int slotIndex, Item item)
        {
            CharacterMainControl main = CharacterMainControl.Main;
            if (main == null)
            {
                return null;
            }

            Item characterItem = main.CharacterItem;
            Inventory mainInventory = characterItem?.Inventory;
            Inventory petInventory = PetProxy.PetInventory;

            BindingLocation location = DetermineLocation(item, characterItem, mainInventory, petInventory);
            if (location == BindingLocation.Unknown || location == BindingLocation.MainInventory)
            {
                return null;
            }

            BindingRecord record = new BindingRecord
            {
                SlotIndex = slotIndex,
                TypeId = item.TypeID,
                Location = location,
                InventoryIndex = item.InInventory != null ? item.InInventory.GetIndex(item) : -1
            };
            BuildParentTypePath(item, characterItem, record.ParentTypePath);
            return record;
        }

        private static BindingLocation DetermineLocation(Item item, Item characterItem, Inventory mainInventory, Inventory petInventory)
        {
            if (item == null)
            {
                return BindingLocation.Unknown;
            }

            if (IsWithinInventory(item, mainInventory))
            {
                return BindingLocation.MainInventory;
            }

            if (IsWithinInventory(item, petInventory))
            {
                return BindingLocation.PetInventory;
            }

            Item root = item.GetRoot();
            if (root != null && root == characterItem)
            {
                return BindingLocation.CharacterTree;
            }

            return BindingLocation.Unknown;
        }

        private static void BuildParentTypePath(Item item, Item characterItem, List<int> path)
        {
            path.Clear();
            Item current = item?.ParentItem;
            while (current != null)
            {
                path.Insert(0, current.TypeID);
                if (current == characterItem)
                {
                    break;
                }
                current = current.ParentItem;
            }
        }

        private static bool IsWithinInventory(Item item, Inventory inventory)
        {
            if (item == null || inventory == null)
            {
                return false;
            }

            Item current = item;
            while (current != null)
            {
                if (current.InInventory == inventory)
                {
                    return true;
                }
                current = current.ParentItem;
            }

            return false;
        }

        private static Item ResolveRecord(BindingRecord record)
        {
            CharacterMainControl main = CharacterMainControl.Main;
            if (main == null)
            {
                return null;
            }

            Item characterItem = main.CharacterItem;
            Inventory mainInventory = characterItem?.Inventory;
            Inventory petInventory = PetProxy.PetInventory;

            return record.Location switch
            {
                BindingLocation.MainInventory => ResolveFromInventory(mainInventory, record),
                BindingLocation.PetInventory => ResolveFromInventory(petInventory, record),
                BindingLocation.CharacterTree => ResolveFromCharacterTree(characterItem, record),
                _ => null
            };
        }

        private static Item ResolveFromInventory(Inventory inventory, BindingRecord record)
        {
            if (inventory == null)
            {
                return null;
            }

            if (record.InventoryIndex >= 0)
            {
                Item indexed = inventory.GetItemAt(record.InventoryIndex);
                if (indexed != null && indexed.TypeID == record.TypeId)
                {
                    return indexed;
                }
            }

            if (record.ParentTypePath.Count > 0)
            {
                Item parent = FindItemByPath(inventory, record.ParentTypePath);
                if (parent != null)
                {
                    Item nested = FindInItemTree(parent, record.TypeId);
                    if (nested != null)
                    {
                        return nested;
                    }
                }
            }

            return FindInInventoryRecursive(inventory, record.TypeId);
        }

        private static Item ResolveFromCharacterTree(Item characterItem, BindingRecord record)
        {
            if (characterItem == null)
            {
                return null;
            }

            Item node = TraverseCharacterPath(characterItem, record.ParentTypePath);
            if (node == null)
            {
                return null;
            }

            return FindInItemTree(node, record.TypeId);
        }

        private static Item FindItemByPath(Inventory inventory, List<int> path)
        {
            if (inventory == null || path == null || path.Count == 0)
            {
                return null;
            }

            Item current = null;
            Inventory currentInventory = inventory;

            foreach (int parentType in path)
            {
                current = FindInInventoryRecursive(currentInventory, parentType);
                if (current == null)
                {
                    return null;
                }
                currentInventory = current.Inventory;
            }

            return current;
        }

        private static Item TraverseCharacterPath(Item start, List<int> path)
        {
            Item current = start;
            if (path == null)
            {
                return current;
            }

            foreach (int typeId in path)
            {
                current = FindInItemTree(current, typeId);
                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }

        private static Item FindInInventoryRecursive(Inventory inventory, int typeId)
        {
            if (inventory == null)
            {
                return null;
            }

            foreach (Item item in inventory)
            {
                Item match = FindInItemTree(item, typeId);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static Item FindInItemTree(Item item, int typeId)
        {
            if (item == null)
            {
                return null;
            }

            if (item.TypeID == typeId)
            {
                return item;
            }

            Item match = FindInInventoryRecursive(item.Inventory, typeId);
            if (match != null)
            {
                return match;
            }

            SlotCollection slots = item.Slots;
            if (slots != null)
            {
                foreach (Slot slot in slots)
                {
                    Item content = slot?.Content;
                    match = FindInItemTree(content, typeId);
                    if (match != null)
                    {
                        return match;
                    }
                }
            }

            return null;
        }

        private static void EnsureListSize<T>(List<T> list, int index)
        {
            while (list.Count <= index)
            {
                list.Add(default);
            }
        }
    }
}
