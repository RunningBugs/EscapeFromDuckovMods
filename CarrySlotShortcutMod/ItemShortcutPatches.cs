using System.Collections.Generic;
using Duckov;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
using UnityEngine.EventSystems;

namespace CarrySlotShortcutMod
{
    [HarmonyPatch]
    internal static class ItemShortcutPatches
    {
        // NOTE: Use-item handling (clone/refund logic) is implemented in UseItemPatches.cs.
        [HarmonyPatch(typeof(ItemShortcut), nameof(ItemShortcut.IsItemValid))]
        [HarmonyPostfix]
        private static void AllowExtendedInventories(Item item, ref bool __result)
        {
            if (__result)
            {
                return;
            }

            if (item == null)
            {
                return;
            }

            if (item.Tags.Contains("Weapon"))
            {
                return;
            }

            if (CarrySlotShortcutHelper.BelongsToAllowedInventories(item))
            {
                __result = true;
            }
        }

        [HarmonyPatch(typeof(ItemShortcut), "Get_Local")]
        [HarmonyPostfix]
        private static void ResolveFromExtendedInventories(ItemShortcut __instance, int index, ref Item __result)
        {
            if (__result != null || __instance == null || index < 0)
            {
                return;
            }

            List<int> typeList = __instance.itemTypes;
            if (typeList == null || index >= typeList.Count)
            {
                return;
            }

            int typeId = typeList[index];
            if (typeId < 0)
            {
                return;
            }

            Item candidate = CarrySlotShortcutHelper.FindAllowedItemByType(__instance, index, typeId);
            if (candidate == null)
            {
                return;
            }

            List<Item> shortcutItems = __instance.items;
            while (shortcutItems.Count <= index)
            {
                shortcutItems.Add(null);
            }

            shortcutItems[index] = candidate;
            __result = candidate;
            ShortcutPersistence.Remember(index, candidate);
        }

        [HarmonyPatch(typeof(ItemShortcutEditorEntry), nameof(ItemShortcutEditorEntry.OnDrop))]
        [HarmonyPrefix]
        private static bool RelaxDropRequirements(ItemShortcutEditorEntry __instance, PointerEventData eventData)
        {
            if (__instance == null || eventData == null)
            {
                return false;
            }

            eventData.Use();

            IItemDragSource dragSource = eventData.pointerDrag == null ? null : eventData.pointerDrag.GetComponent<IItemDragSource>();
            if (dragSource == null || !dragSource.IsEditable())
            {
                return false;
            }

            Item item = dragSource.GetItem();
            if (item == null)
            {
                return false;
            }

            bool allowedRemotely = CarrySlotShortcutHelper.BelongsToAllowedInventories(item);

            if (!allowedRemotely && !ItemUtilities.IsInPlayerCharacter(item))
            {
                ItemUtilities.SendToPlayer(item, dontMerge: false, sendToStorage: false);
            }

            if (ItemShortcut.Set(__instance.index, item))
            {
                ShortcutPersistence.Remember(__instance.index, item);
                __instance.Refresh();
                AudioManager.Post("UI/click");
            }
            else
            {
                ShortcutPersistence.Forget(__instance.index);
            }

            return false;
        }
    }
}
