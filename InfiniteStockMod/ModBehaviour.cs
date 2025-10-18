using System;
using System.Collections;
using System.Collections.Generic;
using Duckov.Economy;
using Duckov.Economy.UI;
using ItemStatsSystem;
using UnityEngine;

namespace InfiniteStockMod
{
    // Minimal, event-driven: refill stocks to Max after a purchase.
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        void Awake()
        {
            Debug.Log("InfiniteStockMod active: refilling shop stocks to Max after purchase.");
        }

        void OnEnable()
        {
            StockShop.OnItemPurchased += OnItemPurchased;
        }

        void OnDisable()
        {
            StockShop.OnItemPurchased -= OnItemPurchased;
        }

        private void OnItemPurchased(StockShop shop, Item item)
        {
            if (shop == null || item == null) return;
            StartCoroutine(RefillNextFrame(shop, item.TypeID));
        }

        private IEnumerator RefillNextFrame(StockShop shop, int itemTypeID)
        {
            yield return null; // let base game finish its UI updates for this frame
            try
            {
                RefillEntryToMax(shop, itemTypeID);
                RefreshActiveView(shop, itemTypeID);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[InfiniteStockMod] Refill failed: {ex.Message}");
            }
        }

        private static void RefillToMax(StockShop shop)
        {
            if (shop == null) return;
            var entries = shop.entries;
            if (entries == null) return;
            foreach (var e in entries)
            {
                if (e == null) continue;
                e.CurrentStock = e.MaxStock;
            }
        }

        private static void RefillEntryToMax(StockShop shop, int itemTypeID)
        {
            if (shop == null) return;
            var entries = shop.entries;
            if (entries == null) return;
            var found = entries.Find(e => e != null && e.ItemTypeID == itemTypeID);
            if (found != null) found.CurrentStock = found.MaxStock;
        }

        private static void RefreshActiveView(StockShop shop)
        {
            var view = StockShopView.Instance;
            if (view == null || !view.isActiveAndEnabled) return;
            if (!object.ReferenceEquals(view.Target, shop)) return;
            // Let the view recompute texts/buttons
            var t = typeof(StockShopView);
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
            try { t.GetMethod("RefreshStockText", flags)?.Invoke(view, null); } catch { }
            try { t.GetMethod("RefreshInteractionButton", flags)?.Invoke(view, null); } catch { }
        }

        private static void RefreshActiveView(StockShop shop, int itemTypeID)
        {
            var view = StockShopView.Instance;
            if (view == null || !view.isActiveAndEnabled) return;
            if (!object.ReferenceEquals(view.Target, shop)) return;
            // If selected entry matches the purchased type, refresh; otherwise skip
            try
            {
                var selected = view.GetSelection();
                if (selected != null && selected.Target != null && selected.Target.ItemTypeID == itemTypeID)
                {
                    var t = typeof(StockShopView);
                    var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
                    t.GetMethod("RefreshStockText", flags)?.Invoke(view, null);
                    t.GetMethod("RefreshInteractionButton", flags)?.Invoke(view, null);
                }
            }
            catch { }
        }
    }
}
