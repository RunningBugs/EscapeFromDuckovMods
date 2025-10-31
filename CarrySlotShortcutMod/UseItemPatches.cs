using System;
using System.Collections.Generic;
using Duckov;
using HarmonyLib;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;

namespace CarrySlotShortcutMod
{
    [HarmonyPatch]
    internal static class UseItemPatches
    {
        private sealed class UseContext
        {
            internal bool IsTransient;
            internal bool Consumed;
            internal Item SourceItem;
            internal Inventory OriginInventory;
            internal int OriginIndex;
            internal Slot OriginSlot;
            internal WeakReference<CharacterMainControl> Controller;
            internal UsageSnapshot Snapshot;
        }

        private readonly struct UsageSnapshot
        {
            internal bool HasItem { get; }
            internal bool Stackable { get; }
            internal int StackCount { get; }
            internal bool UseDurability { get; }
            internal float Durability { get; }

            internal UsageSnapshot(Item item)
            {
                if (item == null)
                {
                    HasItem = false;
                    Stackable = false;
                    StackCount = 0;
                    UseDurability = false;
                    Durability = 0f;
                    return;
                }

                HasItem = true;
                Stackable = item.Stackable;
                StackCount = item.StackCount;
                UseDurability = item.UseDurability;
                Durability = item.Durability;
            }
        }

        private static readonly Dictionary<Item, UseContext> Contexts = new Dictionary<Item, UseContext>();
        private static readonly AccessTools.FieldRef<CA_UseItem, Item> ItemFieldRef = AccessTools.FieldRefAccess<CA_UseItem, Item>("item");
        private static readonly AccessTools.FieldRef<CharacterActionBase, CharacterMainControl> ControllerFieldRef = AccessTools.FieldRefAccess<CharacterActionBase, CharacterMainControl>("characterController");

        [HarmonyPatch(typeof(CharacterMainControl), nameof(CharacterMainControl.UseItem))]
        [HarmonyPrefix]
        private static void OnUseItem(CharacterMainControl __instance, ref Item item)
        {
            if (__instance == null || item == null)
            {
                return;
            }

            CharacterMainControl main = __instance;
            Item characterItem = main.CharacterItem;
            if (characterItem == null)
            {
                Contexts.Remove(item);
                return;
            }

            bool remote = item.GetRoot() != characterItem;
            if (!remote)
            {
                Contexts.Remove(item);
                return;
            }

            UsageSnapshot snapshot = new UsageSnapshot(item);
            Inventory originInventory = item.InInventory;
            int originIndex = originInventory != null ? originInventory.GetIndex(item) : -1;
            Slot originSlot = item.PluggedIntoSlot;

            if (item.Stackable && item.StackCount > 1)
            {
                Item clone = item.CreateInstance();
                if (clone != null)
                {
                    clone.StackCount = 1;
                    clone.AgentUtilities.ReleaseActiveAgent();
                    clone.transform.SetParent(main.transform);

                    Contexts[clone] = new UseContext
                    {
                        IsTransient = true,
                        Consumed = false,
                        SourceItem = item,
                        OriginInventory = originInventory,
                        OriginIndex = originIndex,
                        OriginSlot = originSlot,
                        Controller = new WeakReference<CharacterMainControl>(main),
                        Snapshot = snapshot
                    };

                    item = clone;
                    return;
                }
            }

            Contexts[item] = new UseContext
            {
                IsTransient = false,
                Consumed = false,
                SourceItem = item,
                OriginInventory = originInventory,
                OriginIndex = originIndex,
                OriginSlot = originSlot,
                Controller = new WeakReference<CharacterMainControl>(main),
                Snapshot = snapshot
            };
        }

        [HarmonyPatch(typeof(CharacterMainControl), nameof(CharacterMainControl.UseItem))]
        [HarmonyPostfix]
        private static void AfterUseItem(CharacterMainControl __instance)
        {
            CA_UseItem action = __instance?.useItemAction;
            if (action == null || action.Running)
            {
                return;
            }

            Item currentItem = ItemFieldRef(action);
            if (currentItem == null)
            {
                return;
            }

            if (!Contexts.TryGetValue(currentItem, out UseContext context))
            {
                return;
            }

            if (ControllerFieldRef(action) == null && context.Controller != null)
            {
                context.Controller.TryGetTarget(out CharacterMainControl controller);
                HandleReturn(currentItem, context, controller);
            }
            else
            {
                HandleReturn(currentItem, context, ControllerFieldRef(action));
            }

            Contexts.Remove(currentItem);
            ItemFieldRef(action) = null;
        }

        [HarmonyPatch(typeof(CA_UseItem), "OnFinish")]
        [HarmonyPostfix]
        private static void OnUseFinish(CA_UseItem __instance)
        {
            Item currentItem = ItemFieldRef(__instance);
            if (currentItem == null)
            {
                return;
            }

            if (!Contexts.TryGetValue(currentItem, out UseContext context))
            {
                return;
            }

            if (context.IsTransient)
            {
                bool decremented = TryConsumeSourceStack(context);
                bool consumed = ShouldMarkConsumedAfterTransientUse(context, decremented);
                context.Consumed = consumed;
                Debug.Log($"[CarrySlotShortcutMod][UseItem] OnUseFinish transient consumed={consumed} sourceItem={context.SourceItem?.name ?? "null"} stackable={context.SourceItem?.Stackable} stack={context.SourceItem?.StackCount} durability={context.SourceItem?.Durability}");
                DestroyItemTree(currentItem);
            }
            else
            {
                bool consumed = ShouldMarkConsumedAfterDirectUse(context, currentItem);
                context.Consumed = consumed;
                Debug.Log($"[CarrySlotShortcutMod][UseItem] OnUseFinish direct consumed={consumed} item={currentItem?.name ?? "null"} stackable={currentItem?.Stackable} stack={currentItem?.StackCount} durability={currentItem?.Durability}");
            }
        }

        [HarmonyPatch(typeof(CA_UseItem))]
        private static class OnStopPatch
        {
            private static System.Reflection.MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(CA_UseItem), "OnStop");
            }

            [HarmonyPrefix]
            private static void Prefix(CA_UseItem __instance)
            {
                if (__instance == null)
                {
                    return;
                }

                Item currentItem = ItemFieldRef(__instance);
                if (currentItem == null)
                {
                    return;
                }

                if (!Contexts.TryGetValue(currentItem, out UseContext context))
                {
                    return;
                }

                CharacterMainControl controller = ControllerFieldRef(__instance);
                HandleReturn(currentItem, context, controller);

                Contexts.Remove(currentItem);
                ItemFieldRef(__instance) = null;
            }
        }

        private static void HandleReturn(Item item, UseContext context, CharacterMainControl controller)
        {
            Debug.Log($"[CarrySlotShortcutMod][UseItem] HandleReturn item={item?.name ?? "null"} consumed={context?.Consumed} transient={context?.IsTransient} stackable={item?.Stackable} stack={item?.StackCount} durability={item?.Durability} useDurability={item?.UseDurability}");

            if (context.IsTransient)
            {
                if (!context.Consumed)
                {
                    Debug.Log("[CarrySlotShortcutMod][UseItem] Destroying transient clone due to non-consumed stop.");
                    DestroyItemTree(item);
                }

                return;
            }

            if (context.Consumed)
            {
                Debug.Log("[CarrySlotShortcutMod][UseItem] Context marked consumed; attempting orphan destroy path.");
                DestroyItemTreeIfOrphan(item);
                return;
            }

            if (TryRestoreSlot(item, context.OriginSlot))
            {
                Debug.Log("[CarrySlotShortcutMod][UseItem] Restored item back into original slot.");
                return;
            }

            if (TryPlaceIntoInventory(item, context.OriginInventory, context.OriginIndex))
            {
                Debug.Log("[CarrySlotShortcutMod][UseItem] Returned item to origin inventory.");
                return;
            }

            if (TryPickup(controller, item))
            {
                Debug.Log("[CarrySlotShortcutMod][UseItem] Character pickup succeeded.");
                return;
            }

            Debug.Log("[CarrySlotShortcutMod][UseItem] Fallback send to player storage.");
            ItemUtilities.SendToPlayerStorage(item, directToBuffer: false);
        }

        private static bool TryConsumeSourceStack(UseContext context)
        {
            Item source = context.SourceItem;
            if (source == null || source.IsBeingDestroyed)
            {
                Debug.Log("[CarrySlotShortcutMod][UseItem] Skip consuming source stack (null or being destroyed).");
                return false;
            }

            if (!source.Stackable || source.StackCount <= 0)
            {
                Debug.Log($"[CarrySlotShortcutMod][UseItem] Skip consuming source stack stackable={source.Stackable} stack={source.StackCount}.");
                return false;
            }

            int before = source.StackCount;
            source.StackCount -= 1;
            Debug.Log($"[CarrySlotShortcutMod][UseItem] Consumed source stack item={source.name} before={before} after={source.StackCount}.");
            return before > 0 && source.StackCount < before;
        }

        private static bool TryRestoreSlot(Item item, Slot slot)
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

            item.Detach();

            if (preferredIndex >= 0 && inventory.GetItemAt(preferredIndex) == null)
            {
                if (inventory.AddAt(item, preferredIndex))
                {
                    return true;
                }
            }

            return inventory.AddAndMerge(item, preferredIndex);
        }

        private static bool TryPickup(CharacterMainControl controller, Item item)
        {
            if (controller == null || item == null)
            {
                return false;
            }

            return controller.PickupItem(item);
        }

        private static bool ShouldMarkConsumedAfterDirectUse(UseContext context, Item currentItem)
        {
            UsageSnapshot snapshot = context.Snapshot;

            if (currentItem == null)
            {
                return true;
            }

            if (currentItem.IsBeingDestroyed)
            {
                return true;
            }

            if (snapshot.Stackable)
            {
                return currentItem.StackCount <= 0;
            }

            if (snapshot.UseDurability)
            {
                return currentItem.Durability <= 0f;
            }

            return false;
        }

        private static bool ShouldMarkConsumedAfterTransientUse(UseContext context, bool decrementedSource)
        {
            UsageSnapshot snapshot = context.Snapshot;
            Item source = context.SourceItem;

            if (!snapshot.HasItem)
            {
                return true;
            }

            if (source == null)
            {
                return true;
            }

            if (source.IsBeingDestroyed)
            {
                return true;
            }

            if (snapshot.Stackable)
            {
                if (!decrementedSource)
                {
                    return false;
                }

                return source.StackCount <= 0;
            }

            if (snapshot.UseDurability)
            {
                return source.Durability <= 0f;
            }

            return false;
        }

        private static void DestroyItemTree(Item item)
        {
            if (item == null || item.IsBeingDestroyed)
            {
                return;
            }

            item.DestroyTree();
        }

        private static void DestroyItemTreeIfOrphan(Item item)
        {
            if (item == null || item.IsBeingDestroyed)
            {
                return;
            }

            if (item.ParentObject != null || item.InInventory != null)
            {
                return;
            }

            item.DestroyTree();
        }
    }
}
