using UnityEngine;

namespace DailyQuestMod.Helpers
{
    /// <summary>
    /// Base class for simple scanning mode quests
    /// Automatically sets CheckMode to Scanning
    /// Subclasses only need to implement IsCompleted() and reward logic
    /// </summary>
    public abstract class SimpleScanningQuest : DailyQuestBase
    {
        /// <summary>
        /// Check mode is always Scanning
        /// </summary>
        public override Framework.QuestCheckMode CheckMode => Framework.QuestCheckMode.Scanning;

        /// <summary>
        /// Default scan interval: 1.0 second
        /// Override to customize scan frequency
        /// Recommended range: 0.5 - 2.0 seconds
        /// </summary>
        public override float ScanInterval => 1.0f;

        /// <summary>
        /// Called when quest is activated
        /// For scanning quests, usually minimal setup needed
        /// Override if you need to initialize state
        /// </summary>
        public override void OnActivated()
        {
            base.OnActivated();
            Debug.Log($"[{QuestId}] Scanning quest activated (interval: {ScanInterval}s)");
        }

        /// <summary>
        /// Called when quest is completed
        /// Override to give rewards using helper methods:
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
        /// For scanning quests, usually minimal cleanup needed
        /// Override if you have resources to clean up
        /// </summary>
        public override void OnExpired()
        {
            base.OnExpired();
            ShowExpirationNotification();
        }
    }
}
