using System;
using System.Collections.Generic;

namespace DailyQuestMod.Framework
{
    /// <summary>
    /// Quest completion checking mode
    /// </summary>
    public enum QuestCheckMode
    {
        /// <summary>
        /// Framework polls IsCompleted() at ScanInterval
        /// Use for: location checks, possession checks, time checks
        /// </summary>
        Scanning,

        /// <summary>
        /// Modder manually calls DailyQuestManager.CompleteQuest()
        /// Use for: kill counts, event triggers, submissions
        /// More efficient than scanning
        /// </summary>
        EventDriven
    }

    /// <summary>
    /// Represents a single daily quest
    /// </summary>
    public interface IDailyQuest
    {
        // ===== Identity =====

        /// <summary>
        /// Unique quest identifier (must be unique across all pools)
        /// Recommended: use namespacing like "MyMod.MyQuest" or "mymod_quest_01"
        /// </summary>
        string QuestId { get; }

        /// <summary>
        /// Display name of the quest
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Quest description (can include progress info)
        /// Example: "Kill enemies (3/5)"
        /// </summary>
        string Description { get; }

        // ===== Checking Mode =====

        /// <summary>
        /// How the quest completion should be checked
        /// </summary>
        QuestCheckMode CheckMode { get; }

        /// <summary>
        /// Scan interval in seconds (only used if CheckMode == Scanning)
        /// Recommended: 0.5 - 2.0 seconds
        /// Default: 0.5 seconds
        /// </summary>
        float ScanInterval { get; }

        /// <summary>
        /// Returns true if quest objectives are completed
        /// For Scanning mode: called periodically by framework
        /// For EventDriven mode: called when CompleteQuest() is invoked
        /// </summary>
        /// <returns>True if quest is completed</returns>
        bool IsCompleted();

        // ===== Lifecycle Callbacks =====

        /// <summary>
        /// Called when quest is activated for the day
        /// Use this to: subscribe to events, initialize state, etc.
        /// </summary>
        void OnActivated();

        /// <summary>
        /// Called when quest is completed (IsCompleted() returns true)
        /// IMPORTANT: Handle rewards here (money, exp, items, etc.)
        /// Framework does NOT automatically give rewards
        /// </summary>
        void OnCompleted();

        /// <summary>
        /// Called when day ends and quest was not completed
        /// Use this to: clean up event subscriptions, show notifications, etc.
        /// </summary>
        void OnExpired();

        // ===== Persistence =====

        /// <summary>
        /// Return quest state data to be saved
        /// Return null if no state needs to be saved
        /// Supported types: primitives, strings, serializable objects
        /// </summary>
        /// <returns>Save data object or null</returns>
        object GetSaveData();

        /// <summary>
        /// Restore quest state from saved data
        /// Called after OnActivated() when loading from save
        /// </summary>
        /// <param name="data">Previously saved data from GetSaveData()</param>
        void LoadSaveData(object data);
    }

    /// <summary>
    /// Item reward information
    /// </summary>
    public struct ItemReward
    {
        public int itemId;
        public int amount;

        public ItemReward(int itemId, int amount)
        {
            this.itemId = itemId;
            this.amount = amount;
        }
    }
}
