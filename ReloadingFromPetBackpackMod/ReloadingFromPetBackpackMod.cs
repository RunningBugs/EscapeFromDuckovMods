using System.Collections.Generic;
using HarmonyLib;
using ItemStatsSystem;

namespace ReloadingFromPetBackpackMod
{
    internal static class PetAmmoBridge
    {
        internal sealed class FallbackContext
        {
            public Inventory PlayerInventory;
            public Inventory PetInventory;
            public int BulletTypeId;
            public bool Applied;
        }

        private static readonly Dictionary<ItemSetting_Gun, FallbackContext> Contexts = new Dictionary<ItemSetting_Gun, FallbackContext>();

        public static bool TryPrepare(ItemAgent_Gun gun)
        {
            if (gun == null)
            {
                return false;
            }

            CharacterMainControl holder = gun.Holder;
            if (holder == null)
            {
                return false;
            }

            Inventory playerInventory = holder.CharacterItem != null ? holder.CharacterItem.Inventory : null;
            if (playerInventory == null)
            {
                return false;
            }

            Inventory petInventory = PetProxy.PetInventory;
            if (petInventory == null || petInventory == playerInventory)
            {
                return false;
            }

            ItemSetting_Gun gunSetting = gun.GunItemSetting;
            if (gunSetting == null)
            {
                return false;
            }

            int currentTargetId = gunSetting.TargetBulletID;
            if (currentTargetId >= 0 && CountItemsOfType(playerInventory, currentTargetId) > 0)
            {
                Clear(gunSetting);
                return false;
            }

            Clear(gunSetting);

            int targetTypeId = currentTargetId;
            Item currentLoaded = gunSetting.GetCurrentLoadedBullet();
            if (currentLoaded != null)
            {
                if (targetTypeId < 0 || currentLoaded.TypeID != targetTypeId)
                {
                    targetTypeId = currentLoaded.TypeID;
                }
            }

            if (gunSetting.IsFull() && currentLoaded != null && currentLoaded.TypeID == targetTypeId)
            {
                return false;
            }

            if (targetTypeId < 0 && gunSetting.PreferdBulletsToLoad != null)
            {
                targetTypeId = gunSetting.PreferdBulletsToLoad.TypeID;
            }

            Item chosenBullet = null;
            Inventory sourceInventory = null;

            if (targetTypeId >= 0)
            {
                chosenBullet = FindFirstCompatibleBullet(gunSetting, playerInventory, targetTypeId, true);
                if (chosenBullet != null)
                {
                    sourceInventory = playerInventory;
                }
            }

            if (chosenBullet == null && targetTypeId >= 0)
            {
                chosenBullet = FindFirstCompatibleBullet(gunSetting, petInventory, targetTypeId, true);
                if (chosenBullet != null)
                {
                    sourceInventory = petInventory;
                }
            }

            if (chosenBullet == null)
            {
                chosenBullet = FindFirstCompatibleBullet(gunSetting, playerInventory, targetTypeId, false);
                if (chosenBullet != null)
                {
                    sourceInventory = playerInventory;
                    targetTypeId = chosenBullet.TypeID;
                }
            }

            if (chosenBullet == null)
            {
                chosenBullet = FindFirstCompatibleBullet(gunSetting, petInventory, targetTypeId, false);
                if (chosenBullet != null)
                {
                    sourceInventory = petInventory;
                    targetTypeId = chosenBullet.TypeID;
                }
            }

            if (chosenBullet == null || sourceInventory == null)
            {
                return false;
            }

            int availableCount = CountItemsOfType(sourceInventory, chosenBullet.TypeID);
            if (availableCount <= 0)
            {
                return false;
            }

            gunSetting.SetTargetBulletType(chosenBullet.TypeID);
            gunSetting.PreferdBulletsToLoad = chosenBullet;

            if (sourceInventory == petInventory)
            {
                Contexts[gunSetting] = new FallbackContext
                {
                    PlayerInventory = playerInventory,
                    PetInventory = petInventory,
                    BulletTypeId = chosenBullet.TypeID
                };
            }

            return true;
        }

        public static bool TryGet(ItemSetting_Gun setting, out FallbackContext context)
        {
            return Contexts.TryGetValue(setting, out context);
        }

        public static void Clear(ItemSetting_Gun setting)
        {
            Contexts.Remove(setting);
        }

        public static void ClearAll()
        {
            Contexts.Clear();
        }

        private static Item FindFirstCompatibleBullet(ItemSetting_Gun gunSetting, Inventory source, int targetTypeId, bool requireSameType)
        {
            if (source == null)
            {
                return null;
            }

            foreach (Item item in source)
            {
                if (item == null)
                {
                    continue;
                }

                if (!gunSetting.IsValidBullet(item))
                {
                    continue;
                }

                if (requireSameType)
                {
                    if (targetTypeId >= 0 && item.TypeID == targetTypeId)
                    {
                        return item;
                    }

                    continue;
                }

                if (targetTypeId >= 0 && item.TypeID == targetTypeId)
                {
                    continue;
                }

                return item;
            }

            return null;
        }

        private static int CountItemsOfType(Inventory inventory, int bulletTypeId)
        {
            int total = 0;
            foreach (Item item in inventory)
            {
                if (item != null && item.TypeID == bulletTypeId)
                {
                    total += item.StackCount;
                }
            }

            return total;
        }
    }

    [HarmonyPatch(typeof(ItemAgent_Gun), nameof(ItemAgent_Gun.BeginReload))]
    internal static class ItemAgentGunBeginReloadPatch
    {
        private static bool reentryGuard;

        [HarmonyPostfix]
        private static void Postfix(ItemAgent_Gun __instance, ref bool __result)
        {
            if (__result || reentryGuard)
            {
                return;
            }

            if (!PetAmmoBridge.TryPrepare(__instance))
            {
                return;
            }

            reentryGuard = true;
            try
            {
                __result = __instance.BeginReload();
                if (!__result && __instance != null && __instance.GunItemSetting != null)
                {
                    PetAmmoBridge.Clear(__instance.GunItemSetting);
                }
            }
            finally
            {
                reentryGuard = false;
            }
        }
    }

    [HarmonyPatch(typeof(ItemSetting_Gun), nameof(ItemSetting_Gun.GetBulletCountofTypeInInventory))]
    internal static class ItemSettingGunGetCountPatch
    {
        private static void Postfix(ItemSetting_Gun __instance, Inventory inventory, int bulletItemTypeID, ref int __result)
        {
            if (!PetAmmoBridge.TryGet(__instance, out PetAmmoBridge.FallbackContext context))
            {
                return;
            }

            if (context.PlayerInventory != inventory)
            {
                return;
            }

            if (bulletItemTypeID != context.BulletTypeId)
            {
                return;
            }

            Inventory petInventory = context.PetInventory;
            if (petInventory == null)
            {
                return;
            }

            __result += CountItems(petInventory, bulletItemTypeID);
        }

        private static int CountItems(Inventory inventory, int bulletTypeId)
        {
            int total = 0;
            foreach (Item item in inventory)
            {
                if (item != null && item.TypeID == bulletTypeId)
                {
                    total += item.StackCount;
                }
            }

            return total;
        }
    }

    [HarmonyPatch(typeof(ItemSetting_Gun), nameof(ItemSetting_Gun.LoadBulletsFromInventory))]
    internal static class ItemSettingGunLoadPatch
    {
        private static void Prefix(ItemSetting_Gun __instance, ref Inventory inventory)
        {
            if (!PetAmmoBridge.TryGet(__instance, out PetAmmoBridge.FallbackContext context))
            {
                return;
            }

            if (inventory != context.PlayerInventory)
            {
                return;
            }

            if (context.PetInventory != null)
            {
                context.Applied = true;
                inventory = context.PetInventory;
            }
        }

        private static void Postfix(ItemSetting_Gun __instance)
        {
            if (!PetAmmoBridge.TryGet(__instance, out PetAmmoBridge.FallbackContext context))
            {
                return;
            }

            if (context.Applied)
            {
                PetAmmoBridge.Clear(__instance);
            }
        }
    }
}
