using System;
using System.Collections.Generic;
using HarmonyLib;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;

namespace CarrySlotShortcutMod
{
    [HarmonyPatch]
    internal static class UseItemPatches
    {
        private const string LogPrefix = "[CarrySlotShortcutMod][Use]";

        private sealed class RefundContext
        {
            internal bool IsTransient;
            internal Item SourceItem;
            internal Inventory OriginInventory;
            internal int OriginIndex;
            internal Slot OriginSlot;
            internal WeakReference<CharacterMainControl> Controller;
        }

        private static readonly Dictionary<Item, RefundContext> Contexts = new Dictionary<Item, RefundContext>();
        private static readonly AccessTools.FieldRef<CA_UseItem, Item> ItemFieldRef =
            AccessTools.FieldRefAccess<CA_UseItem, Item>("item");
        private static readonly AccessTools.FieldRef<CharacterActionBase, CharacterMainControl> ControllerFieldRef =
            AccessTools.FieldRefAccess<CharacterActionBase, CharacterMainControl>("characterController");

        [HarmonyPatch(typeof(CharacterMainControl), nameof(CharacterMainControl.UseItem))]
        [HarmonyPrefix]
        private static void PrepareUse(CharacterMainControl __instance, ref Item item)
        {
            if (__instance == null || item == null)
            {
                return;
            }

            Inventory originInventory = item.InInventory;
            int originIndex = originInventory != null ? originInventory.GetIndex(item) : -1;
            Slot originSlot = item.PluggedIntoSlot;
            CharacterMainControl controller = __instance;
            Item characterItem = controller.CharacterItem;

            RemoveContext(item);

            if (characterItem == null)
            {
                Debug.Log($"{LogPrefix} Character item missing; falling back to vanilla behaviour.");
                return;
            }

            bool remote = item.GetRoot() != characterItem;
            if (!remote)
            {
                return;
            }

            if (item.Stackable && item.StackCount > 1)
            {
                item.StackCount -= 1;
                Item clone = item.CreateInstance();
                if (clone != null)
                {
                    clone.StackCount = 1;
                    clone.AgentUtilities.ReleaseActiveAgent();
                    clone.transform.SetParent(controller.transform);

                    RegisterContext(
                        clone,
                        new RefundContext
                        {
                            IsTransient = true,
                            SourceItem = item,
                            OriginInventory = originInventory,
                            OriginIndex = originIndex,
                            OriginSlot = originSlot,
                            Controller = new WeakReference<CharacterMainControl>(controller)
                        });

                    Debug.Log($"{LogPrefix} Created transient clone for {item.DisplayName}.");
                    item = clone;
                    return;
                }

                item.StackCount += 1;
                Debug.Log($"{LogPrefix} Clone creation failed for {item.DisplayName}; reverting to vanilla behaviour.");
            }

            RegisterContext(
                item,
                new RefundContext
                {
                    IsTransient = false,
                    SourceItem = item,
                    OriginInventory = originInventory,
                    OriginIndex = originIndex,
                    OriginSlot = originSlot,
                    Controller = new WeakReference<CharacterMainControl>(controller)
                });

            Debug.Log($"{LogPrefix} Tracking remote single item {item.DisplayName} for potential restore.");
        }

        [HarmonyPatch(typeof(CharacterMainControl), nameof(CharacterMainControl.UseItem))]
        [HarmonyPostfix]
        private static void HandleImmediateFailure(CharacterMainControl __instance, Item item)
        {
            if (item == null)
            {
                return;
            }

            if (!Contexts.TryGetValue(item, out RefundContext context))
            {
                return;
            }

            CA_UseItem action = __instance?.useItemAction;
            if (action != null && action.Running)
            {
                return;
            }

            Debug.Log($"{LogPrefix} UseItem did not start action; refunding immediately.");
            RefundItem(context, item);
            RemoveContext(item);
        }

        [HarmonyPatch(typeof(CA_UseItem), "OnStop")]
        [HarmonyPrefix]
        private static bool HandleOnStop(CA_UseItem __instance)
        {
            if (__instance == null)
            {
                return true;
            }

            Item currentItem = ItemFieldRef(__instance);
            if (currentItem == null)
            {
                return true;
            }

            if (!Contexts.TryGetValue(currentItem, out RefundContext context))
            {
                return true;
            }

            if (context.IsTransient && (currentItem.IsBeingDestroyed || currentItem.StackCount <= 0))
            {
                RemoveContext(currentItem);
                ItemFieldRef(__instance) = null;
                return true;
            }

            CharacterMainControl controller = ControllerFieldRef(__instance);
            bool refunded = RefundItem(context, currentItem);

            RemoveContext(currentItem);
            ItemFieldRef(__instance) = null;

            if (refunded)
            {
                Debug.Log($"{LogPrefix} OnStop handled for {currentItem.DisplayName}; skipping vanilla drop logic.");
                return false;
            }

            Debug.Log($"{LogPrefix} OnStop could not refund {currentItem.DisplayName}; letting vanilla logic proceed.");
            return true;
        }

        private static void RegisterContext(Item key, RefundContext context)
        {
            if (key == null || context == null)
            {
                return;
            }

            Contexts[key] = context;
        }

        private static void RemoveContext(Item key)
        {
            if (key == null)
            {
                return;
            }

            Contexts.Remove(key);
        }

        private static bool RefundItem(RefundContext context, Item item)
        {
            if (context == null || item == null)
            {
                return false;
            }

            if (context.IsTransient)
            {
                if (TryIncrementSourceStack(context))
                {
                    DestroyItemTree(item);
                    Debug.Log($"{LogPrefix} Refunded transient clone into source stack.");
                    return true;
                }

                if (TryPlaceIntoInventory(item, context.OriginInventory, context.OriginIndex))
                {
                    Debug.Log($"{LogPrefix} Refunded transient clone into origin inventory.");
                    return true;
                }

                if (ItemUtilities.SendToPlayerCharacterInventory(item))
                {
                    Debug.Log($"{LogPrefix} Refunded transient clone into player inventory.");
                    return true;
                }

                ItemUtilities.SendToPlayerStorage(item, directToBuffer: false);
                Debug.Log($"{LogPrefix} Refunded transient clone into storage.");
                return true;
            }

            if (TryRestoreToSlot(item, context.OriginSlot))
            {
                Debug.Log($"{LogPrefix} Restored single item to original slot.");
                return true;
            }

            if (TryPlaceIntoInventory(item, context.OriginInventory, context.OriginIndex))
            {
                Debug.Log($"{LogPrefix} Restored single item to original inventory.");
                return true;
            }

            CharacterMainControl controller = null;
            context.Controller?.TryGetTarget(out controller);
            if (controller != null && controller.PickupItem(item))
            {
                Debug.Log($"{LogPrefix} Restored single item by pickup.");
                return true;
            }

            if (ItemUtilities.SendToPlayerCharacterInventory(item))
            {
                Debug.Log($"{LogPrefix} Restored single item into player inventory.");
                return true;
            }

            ItemUtilities.SendToPlayerStorage(item, directToBuffer: false);
            Debug.Log($"{LogPrefix} Restored single item into storage.");
            return true;
        }

        private static bool TryIncrementSourceStack(RefundContext context)
        {
            Item source = context.SourceItem;
            if (source == null || source.IsBeingDestroyed)
            {
                return false;
            }

            if (context.OriginInventory != null && source.InInventory != context.OriginInventory)
            {
                return false;
            }

            source.StackCount += 1;
            return true;
        }

        private static bool TryRestoreToSlot(Item item, Slot slot)
        {
            if (item == null || slot == null)
            {
                return false;
            }

            if (slot.Content == item)
            {
                return true;
            }

            if (slot.Content == null && slot.Plug(item, out _))
            {
                return true;
            }

            return false;
        }

        private static bool TryPlaceIntoInventory(Item item, Inventory inventory, int preferredIndex)
        {
            if (item == null || inventory == null)
            {
                return false;
            }

            if (item.InInventory == inventory)
            {
                return true;
            }

            if (preferredIndex >= 0 && inventory.GetItemAt(preferredIndex) == null && inventory.AddAt(item, preferredIndex))
            {
                return true;
            }

            if (inventory.AddAndMerge(item))
            {
                return true;
            }

            return inventory.AddItem(item);
        }

        private static void DestroyItemTree(Item item)
        {
            if (item == null || item.IsBeingDestroyed)
            {
                return;
            }

            item.DestroyTree();
        }
    }
}
