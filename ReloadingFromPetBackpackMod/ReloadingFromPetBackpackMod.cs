using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.Utilities;
using HarmonyLib;
using ItemStatsSystem;
using UnityEngine;

namespace ReloadingFromPetBackpackMod
{
    internal static class InventoryBulletUtility
    {
        internal static Inventory MainInventory => LevelManager.Instance?.MainCharacter?.CharacterItem?.Inventory;
        internal static Inventory PetInventory => PetProxy.PetInventory;

        internal static bool ShouldAugment(Inventory inventory, int itemTypeId)
        {
            if (itemTypeId < 0 || inventory == null || MainInventory == null)
            {
                return false;
            }

            if (!ReferenceEquals(inventory, MainInventory))
            {
                return false;
            }

            return IsBullet(itemTypeId);
        }

        internal static int CountNested(Inventory inventory, int itemTypeId)
        {
            if (inventory == null)
            {
                return 0;
            }

            int total = 0;
            int foundItems = 0;
            foreach (Item item in EnumerateNested(inventory))
            {
                if (item != null && item.TypeID == itemTypeId)
                {
                    int stack = SafeStack(item);
                    foundItems++;
                    total += stack;
                }
            }

            return total;
        }

        internal static int CountAll(Inventory inventory, int itemTypeId)
        {
            if (inventory == null)
            {
                return 0;
            }

            int total = 0;
            int directCount = 0;
            int nestedCount = 0;

            foreach (Item item in EnumerateDirect(inventory))
            {
                if (item != null && item.TypeID == itemTypeId)
                {
                    int stack = SafeStack(item);
                    directCount++;
                    total += stack;
                }
            }

            foreach (Item item in EnumerateNested(inventory))
            {
                if (item != null && item.TypeID == itemTypeId)
                {
                    int stack = SafeStack(item);
                    nestedCount++;
                    total += stack;
                }
            }

            return total;
        }

        internal static bool TryFindBullet(ItemSetting_Gun gunSetting, out Item bullet)
        {
            bullet = null;
            if (gunSetting == null)
            {
                return false;
            }

            // Match vanilla AutoSetTypeInInventory logic: check caliber only, not fullness
            string gunCaliber = gunSetting.Item?.Constants?.GetString("Caliber".GetHashCode());
            if (string.IsNullOrEmpty(gunCaliber))
            {
                return false;
            }

            foreach (Item candidate in EnumerateBulletCandidates())
            {
                if (candidate == null)
                    continue;

                // Check if it's a bullet
                if (!IsBullet(candidate.TypeID))
                    continue;

                // Check caliber match (same as vanilla)
                string bulletCaliber = candidate.Constants?.GetString("Caliber".GetHashCode());
                if (bulletCaliber == gunCaliber)
                {
                    bullet = candidate;
                    return true;
                }
            }

            return false;
        }

        internal static UniTask<List<Item>> CollectAugmentedAsync(Inventory inventory, int itemTypeId, int amount)
        {
            return CollectAsync(inventory, itemTypeId, amount);
        }

        private static async UniTask<List<Item>> CollectAsync(Inventory inventory, int itemTypeId, int amount)
        {
            List<Item> result = new List<Item>();
            if (amount <= 0)
            {
                return result;
            }

            int collected = 0;

            collected = await ConsumeAsync(EnumerateDirect(inventory), itemTypeId, amount, collected, result, "PlayerDirect");

            if (collected < amount)
            {
                collected = await ConsumeAsync(EnumerateNested(inventory), itemTypeId, amount, collected, result, "PlayerNested");
            }
            if (collected < amount)
            {
                collected = await ConsumeAsync(EnumerateDirect(PetInventory), itemTypeId, amount, collected, result, "PetDirect");
            }
            if (collected < amount)
            {
                collected = await ConsumeAsync(EnumerateNested(PetInventory), itemTypeId, amount, collected, result, "PetNested");
            }

            return result;
        }

        private static async UniTask<int> ConsumeAsync(IEnumerable<Item> source, int itemTypeId, int targetAmount, int collected, List<Item> buffer, string sourceName = "Unknown")
        {
            if (source == null)
            {
                return collected;
            }

            int foundCount = 0;

            // Materialize the enumerable to avoid issues with items in slots being modified during iteration
            List<Item> itemList = new List<Item>();
            try
            {
                foreach (Item item in source)
                {
                    if (item != null)
                    {
                        itemList.Add(item);
                    }
                }
            }
            catch (System.Exception)
            {
                return collected;
            }

            foreach (Item item in itemList)
            {
                if (item == null || item.TypeID != itemTypeId)
                {
                    continue;
                }

                foundCount++;
                int remaining = targetAmount - collected;
                if (remaining <= 0)
                {
                    break;
                }

                int stack = SafeStack(item);
                if (stack <= 0)
                {
                    continue;
                }


                try
                {
                    if (stack > remaining)
                    {
                        Item split = await item.Split(remaining);
                        if (split != null)
                        {
                            buffer.Add(split);
                            collected += remaining;
                        }
                        else
                        {
                            item.Detach();
                            buffer.Add(item);
                            collected += stack;
                        }
                    }
                    else
                    {
                        item.Detach();
                        buffer.Add(item);
                        collected += stack;
                    }
                }
                catch (System.Exception)
                {
                }
            }

            return collected;
        }

        private static IEnumerable<Item> EnumerateBulletCandidates()
        {

            // FIXED: Include direct player inventory items first
            int count = 0;
            foreach (Item item in EnumerateDirect(MainInventory))
            {
                count++;
                yield return item;
            }

            count = 0;
            foreach (Item item in EnumerateNested(MainInventory))
            {
                count++;
                yield return item;
            }

            count = 0;
            foreach (Item item in EnumerateDirect(PetInventory))
            {
                count++;
                yield return item;
            }

            count = 0;
            foreach (Item item in EnumerateNested(PetInventory))
            {
                count++;
                yield return item;
            }
        }

        internal static int SafeStack(Item item)
        {
            return item?.StackCount > 0 ? item.StackCount : 0;
        }

        internal static bool IsBullet(int itemTypeId)
        {
            try
            {
                ItemMetaData meta = ItemAssetsCollection.GetMetaData(itemTypeId);
                if (meta.tags == null)
                {
                    return false;
                }

                foreach (Tag tag in meta.tags)
                {
                    if (tag == GameplayDataSettings.Tags.Bullet)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        internal static IEnumerable<Item> EnumerateDirect(Inventory inventory)
        {
            if (inventory == null)
            {
                yield break;
            }

            foreach (Item item in inventory)
            {
                if (item != null)
                {
                    yield return item;
                }
            }
        }

        internal static IEnumerable<Item> EnumerateNested(Inventory inventory)
        {
            if (inventory == null)
            {
                yield break;
            }


            HashSet<Item> visitedItems = new HashSet<Item>();
            Queue<Item> queue = new Queue<Item>();

            // Find all items with slots (containers like backpacks/bags)
            int containerCount = 0;
            int itemCount = 0;
            foreach (Item item in inventory)
            {
                itemCount++;
                if (item == null)
                {
                    continue;
                }

                // Check if item has Slots (this is how containers store items!)
                bool hasSlots = item.Slots != null && item.Slots.Count > 0;


                if (hasSlots && visitedItems.Add(item))
                {
                    containerCount++;
                    queue.Enqueue(item);
                }
            }

            // BFS traversal of items in slots
            int totalItems = 0;
            while (queue.Count > 0)
            {
                Item containerItem = queue.Dequeue();

                if (containerItem.Slots == null)
                    continue;

                foreach (var slot in containerItem.Slots)
                {
                    Item slotContent = slot.Content;
                    if (slotContent != null)
                    {
                        totalItems++;
                        yield return slotContent;

                        // Check if this item also has slots (nested containers)
                        if (slotContent.Slots != null && slotContent.Slots.Count > 0 && visitedItems.Add(slotContent))
                        {
                            queue.Enqueue(slotContent);
                        }
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(ItemSetting_Gun), nameof(ItemSetting_Gun.GetBulletTypesInInventory))]
    internal static class ItemSettingGunGetBulletTypesPatch
    {
        private static void Postfix(ItemSetting_Gun __instance, Inventory inventory, ref Dictionary<int, BulletTypeInfo> __result)
        {
            if (__instance == null || inventory == null || __result == null)
            {
                return;
            }

            if (!ReferenceEquals(inventory, InventoryBulletUtility.MainInventory))
            {
                return;
            }


            string gunCaliber = __instance.Item?.Constants?.GetString("Caliber".GetHashCode());
            if (string.IsNullOrEmpty(gunCaliber))
            {
                return;
            }

            // Add bullets from nested containers
            foreach (Item item in InventoryBulletUtility.EnumerateNested(inventory))
            {
                if (item == null || !InventoryBulletUtility.IsBullet(item.TypeID))
                    continue;

                string bulletCaliber = item.Constants?.GetString("Caliber".GetHashCode());
                if (bulletCaliber != gunCaliber)
                    continue;

                if (!__result.ContainsKey(item.TypeID))
                {
                    BulletTypeInfo info = new BulletTypeInfo();
                    info.bulletTypeID = item.TypeID;
                    info.count = InventoryBulletUtility.SafeStack(item);
                    __result.Add(item.TypeID, info);
                }
                else
                {
                    __result[item.TypeID].count += InventoryBulletUtility.SafeStack(item);
                }
            }

            // Add bullets from pet inventory
            if (InventoryBulletUtility.PetInventory != null)
            {
                foreach (Item item in InventoryBulletUtility.EnumerateDirect(InventoryBulletUtility.PetInventory))
                {
                    if (item == null || !InventoryBulletUtility.IsBullet(item.TypeID))
                        continue;

                    string bulletCaliber = item.Constants?.GetString("Caliber".GetHashCode());
                    if (bulletCaliber != gunCaliber)
                        continue;

                    if (!__result.ContainsKey(item.TypeID))
                    {
                        BulletTypeInfo info = new BulletTypeInfo();
                        info.bulletTypeID = item.TypeID;
                        info.count = InventoryBulletUtility.SafeStack(item);
                        __result.Add(item.TypeID, info);
                    }
                    else
                    {
                        __result[item.TypeID].count += InventoryBulletUtility.SafeStack(item);
                    }
                }

                foreach (Item item in InventoryBulletUtility.EnumerateNested(InventoryBulletUtility.PetInventory))
                {
                    if (item == null || !InventoryBulletUtility.IsBullet(item.TypeID))
                        continue;

                    string bulletCaliber = item.Constants?.GetString("Caliber".GetHashCode());
                    if (bulletCaliber != gunCaliber)
                        continue;

                    if (!__result.ContainsKey(item.TypeID))
                    {
                        BulletTypeInfo info = new BulletTypeInfo();
                        info.bulletTypeID = item.TypeID;
                        info.count = InventoryBulletUtility.SafeStack(item);
                        __result.Add(item.TypeID, info);
                    }
                    else
                    {
                        __result[item.TypeID].count += InventoryBulletUtility.SafeStack(item);
                    }
                }
            }

        }
    }

    [HarmonyPatch(typeof(ItemSetting_Gun), nameof(ItemSetting_Gun.GetBulletCountofTypeInInventory))]
    internal static class ItemSettingGunGetBulletCountPatch
    {
        private static void Postfix(ItemSetting_Gun __instance, int bulletItemTypeID, Inventory inventory, ref int __result)
        {

            if (!InventoryBulletUtility.ShouldAugment(inventory, bulletItemTypeID))
            {
                return;
            }

            int originalCount = __result;
            int nestedCount = InventoryBulletUtility.CountNested(inventory, bulletItemTypeID);
            int petCount = InventoryBulletUtility.CountAll(InventoryBulletUtility.PetInventory, bulletItemTypeID);

            __result += nestedCount + petCount;

        }
    }

    [HarmonyPatch(typeof(ItemSetting_Gun), nameof(ItemSetting_Gun.AutoSetTypeInInventory))]
    internal static class ItemSettingGunAutoSetTypePatch
    {
        private static void Postfix(ItemSetting_Gun __instance, Inventory inventory, ref bool __result)
        {

            if (__result || __instance == null || inventory == null)
            {
                return;
            }

            if (!ReferenceEquals(inventory, InventoryBulletUtility.MainInventory))
            {
                return;
            }

            if (InventoryBulletUtility.TryFindBullet(__instance, out Item bullet))
            {
                __instance.SetTargetBulletType(bullet);
                __result = true;
            }
            else
            {
            }
        }
    }

    [HarmonyPatch(typeof(ItemExtensions), nameof(ItemExtensions.GetItemsOfAmount))]
    internal static class ItemExtensionsGetItemsOfAmountPatch
    {
        private static bool Prefix(Inventory inventory, int itemTypeID, int amount, ref UniTask<List<Item>> __result)
        {
            if (!InventoryBulletUtility.ShouldAugment(inventory, itemTypeID))
            {
                return true;
            }

            __result = InventoryBulletUtility.CollectAugmentedAsync(inventory, itemTypeID, amount);
            return false;
        }
    }

    // Patch TransToReady to refresh HUD after reload completes
    [HarmonyPatch(typeof(ItemAgent_Gun), "TransToReady")]
    internal static class ItemAgentGunTransToReadyPatch
    {
        private static void Postfix(ItemAgent_Gun __instance)
        {

            // Find the BulletCountHUD and force refresh
            var hud = UnityEngine.Object.FindObjectOfType<BulletCountHUD>();
            if (hud != null)
            {
                try
                {
                    var traverse = Traverse.Create(hud);
                    traverse.Method("ChangeTotalCount").GetValue();
                }
                catch (System.Exception)
                {
                }
            }
        }
    }
}
