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

            Item preferred = gunSetting.PreferdBulletsToLoad;
            if (preferred != null && preferred.InInventory != petInventory)
            {
                preferred = null;
            }

            Item chosenBullet = preferred ?? FindFirstCompatibleBullet(gunSetting, petInventory);
            if (chosenBullet == null)
            {
                return false;
            }

            int bulletTypeId = chosenBullet.TypeID;
            int availableCount = CountItemsOfType(petInventory, bulletTypeId);
            if (availableCount <= 0)
            {
                return false;
            }

            gunSetting.SetTargetBulletType(bulletTypeId);
            gunSetting.PreferdBulletsToLoad = chosenBullet;

            Contexts[gunSetting] = new FallbackContext
            {
                PlayerInventory = playerInventory,
                PetInventory = petInventory,
                BulletTypeId = bulletTypeId
            };

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

        private static Item FindFirstCompatibleBullet(ItemSetting_Gun gunSetting, Inventory source)
        {
            foreach (Item item in source)
            {
                if (item == null)
                {
                    continue;
                }

                if (gunSetting.IsValidBullet(item))
                {
                    return item;
                }
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
