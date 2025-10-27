using UnityEngine;
using DailyQuestMod.Framework;

namespace DailyQuestMod.Helpers
{
    /// <summary>
    /// Base class for simple event-driven quests
    /// Automatically sets CheckMode to EventDriven
    /// Provides NotifyCompleted() helper for easy completion
    /// Subclasses implement event subscription logic and rewards
    /// </summary>
    public abstract class SimpleEventQuest : DailyQuestBase
    {
        /// <summary>
        /// Check mode is always EventDriven
        /// </summary>
        public override QuestCheckMode CheckMode => QuestCheckMode.EventDriven;

        /// <summary>
        /// Scan interval not used for event-driven quests
        /// </summary>
        public override float ScanInterval => 0f;

        /// <summary>
        /// Called when quest is activated
        /// Override to subscribe to game events
        /// Example: Health.OnDead += OnEnemyDead;
        /// </summary>
        public override void OnActivated()
        {
            base.OnActivated();
            Debug.Log($"[{QuestId}] Event-driven quest activated");
        }

        /// <summary>
        /// Called when quest is completed
        /// Override to give rewards and clean up event subscriptions
        /// Use helper methods:
        /// - GiveMoneyReward(amount)
        /// - GiveExpReward(amount)
        /// - GiveItemReward(itemId, amount)
        /// </summary>
        public override void OnCompleted()
        {
            base.OnCompleted();
            ShowCompletionNotification();
        }

        /// <summary>
        /// Called when quest expires
        /// IMPORTANT: Unsubscribe from events here to prevent memory leaks
        /// Example: Health.OnDead -= OnEnemyDead;
        /// </summary>
        public override void OnExpired()
        {
            base.OnExpired();
            ShowExpirationNotification();
        }

        /// <summary>
        /// Helper method to check completion and notify manager
        /// Call this from your event handlers when progress is made
        /// Framework will verify IsCompleted() before completing
        /// </summary>
        protected void NotifyCompleted()
        {
            if (IsCompleted())
            {
                DailyQuestManager.CompleteQuest(QuestId);
            }
        }

        /// <summary>
        /// Helper method to log progress (optional)
        /// Override to provide custom progress logging
        /// </summary>
        /// <param name="message">Progress message</param>
        protected virtual void LogProgress(string message)
        {
            Debug.Log($"[{QuestId}] Progress: {message}");
        }
    }
}
