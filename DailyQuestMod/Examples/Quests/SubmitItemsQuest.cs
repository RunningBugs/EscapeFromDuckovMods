using System;
using System.Collections.Generic;
using UnityEngine;
using ItemStatsSystem;
using DailyQuestMod.Helpers;

namespace DailyQuestMod.Examples.Quests
{
    /// <summary>
    /// Daily quest to submit specific items to complete
    /// Uses scanning mode to check item possession
    /// Items are consumed when quest is completed
    /// </summary>
    public class SubmitItemsQuest : SimpleScanningQuest
    {
        private int itemId;
        private int requiredAmount;
        private string itemName;
        private bool itemsSubmitted = false;

        // Configurable parameters
        private int moneyReward = 200;
        private int expReward = 30;

        // ===== Quest Identity =====

        public override string QuestId => $"daily_submit_items_{itemId}";
        public override string Title => $"Item Collector: {itemName}";

        public override string Description
        {
            get
            {
                int currentAmount = GetPlayerItemCount();
                if (itemsSubmitted)
                {
                    return $"Submit {itemName} x{requiredAmount} (Submitted!)\nReward: ${moneyReward} + {expReward} EXP";
                }
                return $"Submit {itemName} ({currentAmount}/{requiredAmount} available)\nReward: ${moneyReward} + {expReward} EXP";
            }
        }

        // Scan less frequently for item checks
        public override float ScanInterval => 2.0f;

        // ===== Quest Logic =====

        public override bool IsCompleted()
        {
            return itemsSubmitted;
        }

        public override void OnActivated()
        {
            base.OnActivated();

            // Reset state
            itemsSubmitted = false;

            Debug.Log($"[SubmitItemsQuest] Activated - collect and submit {requiredAmount} {itemName}");
        }

        public override void OnCompleted()
        {
            // Consume the items
            if (!itemsSubmitted)
            {
                ConsumeItems();
            }

            // Give rewards
            GiveMoneyReward(moneyReward);
            GiveExpReward(expReward);

            ShowCompletionNotification($"Item Collector Complete! Submitted {requiredAmount} {itemName}");

            base.OnCompleted();
        }

        public override void OnExpired()
        {
            ShowExpirationNotification($"Item Collector Expired (Items not submitted)");
            base.OnExpired();
        }

        // ===== Helper Methods =====

        /// <summary>
        /// Get the number of target items the player currently has
        /// </summary>
        private int GetPlayerItemCount()
        {
            try
            {
                return ItemUtilities.GetItemCount(itemId);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SubmitItemsQuest] Error getting item count: {e}");
                return 0;
            }
        }

        /// <summary>
        /// Check if player has enough items to submit
        /// </summary>
        private bool HasEnoughItems()
        {
            return GetPlayerItemCount() >= requiredAmount;
        }

        /// <summary>
        /// Consume the required items from player inventory
        /// </summary>
        private void ConsumeItems()
        {
            try
            {
                List<Item> playerItems = ItemUtilities.FindAllBelongsToPlayer(
                    item => item != null && item.TypeID == itemId
                );

                int remaining = requiredAmount;

                foreach (Item item in playerItems)
                {
                    if (remaining <= 0)
                        break;

                    if (item.StackCount <= remaining)
                    {
                        // Consume entire stack
                        remaining -= item.StackCount;
                        item.Detach();
                        item.DestroyTree();
                    }
                    else
                    {
                        // Consume partial stack
                        item.StackCount -= remaining;
                        remaining = 0;
                    }
                }

                if (remaining == 0)
                {
                    itemsSubmitted = true;
                    Debug.Log($"[SubmitItemsQuest] Successfully consumed {requiredAmount} {itemName}");
                }
                else
                {
                    Debug.LogWarning($"[SubmitItemsQuest] Could not consume all items, {remaining} remaining");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SubmitItemsQuest] Error consuming items: {e}");
            }
        }

        /// <summary>
        /// Manual submit method (can be called from UI or interaction)
        /// </summary>
        public void SubmitItems()
        {
            if (itemsSubmitted)
            {
                Debug.LogWarning($"[SubmitItemsQuest] Items already submitted");
                return;
            }

            if (!HasEnoughItems())
            {
                Debug.LogWarning($"[SubmitItemsQuest] Not enough items to submit ({GetPlayerItemCount()}/{requiredAmount})");
                return;
            }

            ConsumeItems();

            if (itemsSubmitted)
            {
                // Manually trigger completion check
                Framework.DailyQuestManager.CompleteQuest(QuestId);
            }
        }

        // ===== Save/Load =====

        public override object GetSaveData()
        {
            return itemsSubmitted;
        }

        public override void LoadSaveData(object data)
        {
            if (data is bool submitted)
            {
                itemsSubmitted = submitted;
                Debug.Log($"[SubmitItemsQuest] Restored submission state: {itemsSubmitted}");
            }
        }

        // ===== Configuration =====

        /// <summary>
        /// Create an item submission quest
        /// </summary>
        /// <param name="itemId">Item type ID to submit</param>
        /// <param name="itemName">Display name for the item</param>
        /// <param name="amount">Number of items to submit</param>
        /// <param name="money">Money reward</param>
        /// <param name="exp">Experience reward</param>
        public SubmitItemsQuest(int itemId, string itemName, int amount, int money, int exp)
        {
            this.itemId = itemId;
            this.itemName = itemName;
            this.requiredAmount = amount;
            this.moneyReward = money;
            this.expReward = exp;
        }

        /// <summary>
        /// Create an item submission quest with item metadata lookup
        /// </summary>
        /// <param name="itemId">Item type ID to submit</param>
        /// <param name="amount">Number of items to submit</param>
        /// <param name="money">Money reward</param>
        /// <param name="exp">Experience reward</param>
        public SubmitItemsQuest(int itemId, int amount, int money, int exp)
        {
            this.itemId = itemId;
            this.requiredAmount = amount;
            this.moneyReward = money;
            this.expReward = exp;

            // Try to get item name from metadata
            try
            {
                ItemMetaData meta = ItemAssetsCollection.GetMetaData(itemId);
                this.itemName = meta.DisplayName ?? $"Item #{itemId}";
            }
            catch
            {
                this.itemName = $"Item #{itemId}";
            }
        }
    }
}
