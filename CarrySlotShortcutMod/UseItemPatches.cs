using System;
using System.Collections.Generic;
using Duckov;
using HarmonyLib;
using ItemStatsSystem;
using ItemStatsSystem.Items;

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
                        Controller = new WeakReference<CharacterMainControl>(main)
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
                Controller = new WeakReference<CharacterMainControl>(main)
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

            context.Consumed = true;

            if (context.IsTransient)
            {
                TryConsumeSourceStack(context);
                DestroyItemTree(currentItem);
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
            if (context.IsTransient)
            {
                if (!context.Consumed)
                {
                    DestroyItemTree(item);
                }

                return;
            }

            if (context.Consumed)
            {
                DestroyItemTreeIfOrphan(item);
                return;
            }

            if (TryRestoreSlot(item, context.OriginSlot))
            {
                return;
            }

            if (TryPlaceIntoInventory(item, context.OriginInventory, context.OriginIndex))
            {
                return;
            }

            if (TryPickup(controller, item))
            {
                return;
            }

            ItemUtilities.SendToPlayerStorage(item, directToBuffer: false);
        }

        private static void TryConsumeSourceStack(UseContext context)
        {
            Item source = context.SourceItem;
            if (source == null || source.IsBeingDestroyed)
            {
                return;
            }

            if (!source.Stackable || source.StackCount <= 0)
            {
                return;
            }

            source.StackCount -= 1;
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
