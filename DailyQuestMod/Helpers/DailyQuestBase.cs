using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using Duckov;
using Duckov.Economy;

namespace DailyQuestMod.Helpers
{
    /// <summary>
    /// Abstract base class for daily quests with common functionality
    /// Provides default implementations and helper methods for rewards
    /// </summary>
    public abstract class DailyQuestBase : Framework.IDailyQuest
    {
        // ===== Abstract Properties (must implement) =====

        public abstract string QuestId { get; }
        public abstract string Title { get; }
        public abstract Framework.QuestCheckMode CheckMode { get; }
        public abstract bool IsCompleted();

        // ===== Virtual Properties (can override) =====

        /// <summary>
        /// Quest description with progress info
        /// Override to provide custom description
        /// </summary>
        public virtual string Description => Title;

        /// <summary>
        /// Scan interval in seconds (only for Scanning mode)
        /// Default: 0.5 seconds
        /// </summary>
        public virtual float ScanInterval => 0.5f;

        // ===== Lifecycle Methods (can override) =====

        /// <summary>
        /// Called when quest is activated
        /// Override to subscribe to events, initialize state, etc.
        /// </summary>
        public virtual void OnActivated()
        {
            // Default: do nothing
        }

        /// <summary>
        /// Called when quest is completed
        /// Override to give rewards and handle completion logic
        /// Use helper methods: GiveMoneyReward, GiveExpReward, GiveItemReward
        /// </summary>
        public virtual void OnCompleted()
        {
            // Default: do nothing (subclass should override)
        }

        /// <summary>
        /// Called when quest expires
        /// Override to clean up event subscriptions, show notifications, etc.
        /// </summary>
        public virtual void OnExpired()
        {
            // Default: do nothing
        }

        // ===== Save/Load (can override) =====

        /// <summary>
        /// Return quest state to save
        /// Override if your quest has state to persist
        /// </summary>
        public virtual object GetSaveData()
        {
            return null; // Default: no state to save
        }

        /// <summary>
        /// Restore quest state from save
        /// Override if your quest has state to persist
        /// </summary>
        public virtual void LoadSaveData(object data)
        {
            // Default: do nothing
        }

        // ===== Reward Helper Methods =====

        /// <summary>
        /// Give money reward to the player
        /// Shows notification on success
        /// </summary>
        /// <param name="amount">Amount of money to give</param>
        /// <returns>True if successful</returns>
        protected bool GiveMoneyReward(int amount)
        {
            try
            {
                if (amount <= 0)
                {
                    Debug.LogWarning($"[{QuestId}] Invalid money reward amount: {amount}");
                    return false;
                }

                bool success = EconomyManager.Add(amount);
                if (success)
                {
                    Debug.Log($"[{QuestId}] Gave money reward: {amount}");
                    ShowRewardNotification($"Received ${amount}");
                }
                else
                {
                    Debug.LogError($"[{QuestId}] Failed to give money reward: {amount}");
                }

                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"[{QuestId}] Error giving money reward: {e}");
                return false;
            }
        }

        /// <summary>
        /// Give experience reward to the player
        /// Shows notification on success
        /// </summary>
        /// <param name="amount">Amount of experience to give</param>
        /// <returns>True if successful</returns>
        protected bool GiveExpReward(int amount)
        {
            try
            {
                if (amount <= 0)
                {
                    Debug.LogWarning($"[{QuestId}] Invalid exp reward amount: {amount}");
                    return false;
                }

                bool success = EXPManager.AddExp(amount);
                if (success)
                {
                    Debug.Log($"[{QuestId}] Gave exp reward: {amount}");
                    ShowRewardNotification($"Gained {amount} EXP");
                }
                else
                {
                    Debug.LogError($"[{QuestId}] Failed to give exp reward: {amount}");
                }

                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"[{QuestId}] Error giving exp reward: {e}");
                return false;
            }
        }

        /// <summary>
        /// Give item reward to the player
        /// Items are sent to player storage
        /// Shows notification on success
        /// </summary>
        /// <param name="itemId">Item type ID</param>
        /// <param name="amount">Number of items to give</param>
        /// <returns>True if successful</returns>
        protected bool GiveItemReward(int itemId, int amount = 1)
        {
            try
            {
                if (itemId <= 0)
                {
                    Debug.LogWarning($"[{QuestId}] Invalid item ID: {itemId}");
                    return false;
                }

                if (amount <= 0)
                {
                    Debug.LogWarning($"[{QuestId}] Invalid item amount: {amount}");
                    return false;
                }

                GiveItemRewardAsync(itemId, amount).Forget();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[{QuestId}] Error giving item reward: {e}");
                return false;
            }
        }

        /// <summary>
        /// Give multiple item rewards at once
        /// </summary>
        /// <param name="rewards">List of item rewards to give</param>
        /// <returns>True if all successful</returns>
        protected bool GiveItemRewards(List<Framework.ItemReward> rewards)
        {
            if (rewards == null || rewards.Count == 0)
                return true;

            bool allSuccess = true;
            foreach (var reward in rewards)
            {
                if (!GiveItemReward(reward.itemId, reward.amount))
                {
                    allSuccess = false;
                }
            }

            return allSuccess;
        }

        private async UniTask GiveItemRewardAsync(int itemId, int amount)
        {
            try
            {
                ItemMetaData meta = ItemAssetsCollection.GetMetaData(itemId);
                if (meta.id <= 0)
                {
                    Debug.LogError($"[{QuestId}] Item not found: {itemId}");
                    return;
                }

                int remaining = amount;
                List<Item> generatedItems = new List<Item>();

                // Generate items respecting max stack count
                while (remaining > 0)
                {
                    int batchAmount = Mathf.Min(remaining, meta.maxStackCount);
                    if (batchAmount <= 0)
                        break;

                    remaining -= batchAmount;

                    Item item = await ItemAssetsCollection.InstantiateAsync(itemId);
                    if (item != null)
                    {
                        if (batchAmount > 1)
                        {
                            item.StackCount = batchAmount;
                        }
                        generatedItems.Add(item);
                    }
                }

                // Send items to player storage
                foreach (Item item in generatedItems)
                {
                    PlayerStorage.Push(item, toBufferDirectly: true);
                }

                Debug.Log($"[{QuestId}] Gave item reward: {meta.DisplayName} x{amount}");
                ShowRewardNotification($"Received {meta.DisplayName} x{amount}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[{QuestId}] Error in GiveItemRewardAsync: {e}");
            }
        }

        /// <summary>
        /// Show a reward notification to the player
        /// Override to customize notification display
        /// </summary>
        /// <param name="message">Notification message</param>
        protected virtual void ShowRewardNotification(string message)
        {
            // TODO: Integrate with game's notification system when available
            // For now, just log
            Debug.Log($"[{QuestId}] Reward: {message}");
        }

        /// <summary>
        /// Show a quest completion notification
        /// Override to customize notification display
        /// </summary>
        /// <param name="message">Notification message</param>
        protected virtual void ShowCompletionNotification(string message = null)
        {
            string notification = message ?? $"Quest Complete: {Title}";
            Debug.Log($"[{QuestId}] {notification}");
            // TODO: Integrate with game's notification system
        }

        /// <summary>
        /// Show a quest expiration notification
        /// Override to customize notification display
        /// </summary>
        /// <param name="message">Notification message</param>
        protected virtual void ShowExpirationNotification(string message = null)
        {
            string notification = message ?? $"Quest Expired: {Title}";
            Debug.Log($"[{QuestId}] {notification}");
            // TODO: Integrate with game's notification system
        }
    }
}
