using System.Collections.Generic;
using Duckov;
using ItemStatsSystem;

namespace CarrySlotShortcutMod
{
    internal static class CarrySlotShortcutHelper
    {
        internal static bool BelongsToAllowedInventories(Item item)
        {
            if (item == null)
            {
                return false;
            }

            if (ItemUtilities.IsInPlayerCharacter(item))
            {
                return true;
            }

            return IsInPetInventory(item);
        }

        internal static Item FindAllowedItemByType(ItemShortcut shortcut, int currentIndex, int typeId)
        {
            if (shortcut == null || typeId < 0)
            {
                return null;
            }

            foreach (Item candidate in EnumerateAllowedItems())
            {
                if (candidate == null || candidate.TypeID != typeId)
                {
                    continue;
                }

                if (IsAlreadyAssigned(shortcut, currentIndex, candidate))
                {
                    continue;
                }

                return candidate;
            }

            return null;
        }

        private static bool IsAlreadyAssigned(ItemShortcut shortcut, int currentIndex, Item candidate)
        {
            List<Item> assignedItems = shortcut.items;
            if (assignedItems == null)
            {
                return false;
            }
            for (int i = 0; i < assignedItems.Count; i++)
            {
                if (i == currentIndex)
                {
                    continue;
                }
                if (assignedItems[i] == candidate)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsInPetInventory(Item item)
        {
            Inventory petInventory = PetProxy.PetInventory;
            if (petInventory == null)
            {
                return false;
            }

            if (item.InInventory == petInventory)
            {
                return true;
            }

            List<Item> parents = item.GetAllParents(excludeSelf: true);
            foreach (Item parent in parents)
            {
                if (parent != null && parent.InInventory == petInventory)
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<Item> EnumerateAllowedItems()
        {
            Item characterItem = CharacterMainControl.Main?.CharacterItem;
            if (characterItem != null)
            {
                foreach (Item child in characterItem.GetAllChildren(includingGrandChildren: true, excludeSelf: true))
                {
                    yield return child;
                }
            }

            Inventory petInventory = PetProxy.PetInventory;
            if (petInventory != null)
            {
                foreach (Item item in EnumerateInventory(petInventory))
                {
                    yield return item;
                }
            }
        }

        private static IEnumerable<Item> EnumerateInventory(Inventory inventory)
        {
            if (inventory == null)
            {
                yield break;
            }

            foreach (Item item in inventory)
            {
                if (item == null)
                {
                    continue;
                }

                yield return item;

                foreach (Item child in item.GetAllChildren(includingGrandChildren: true, excludeSelf: true))
                {
                    yield return child;
                }
            }
        }
    }
}
