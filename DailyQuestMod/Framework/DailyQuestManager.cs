using System;
using System.Collections.Generic;
using UnityEngine;

namespace DailyQuestMod.Framework
{
    /// <summary>
    /// Main daily quest system manager - Static API
    /// Thread-safe singleton access to the daily quest system
    /// </summary>
    public static class DailyQuestManager
    {
        private static DailyQuestManagerInternal _instance;

        /// <summary>
        /// Get or create the internal manager instance
        /// </summary>
        private static DailyQuestManagerInternal Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("DailyQuestManager");
                    _instance = go.AddComponent<DailyQuestManagerInternal>();
                    GameObject.DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // ===== Configuration =====

        /// <summary>
        /// Maximum number of daily quests allowed simultaneously
        /// -1 = unlimited (default)
        /// 0 = no quests
        /// >0 = max limit, excess quests are randomly sampled
        /// </summary>
        public static int MaxDailyQuests
        {
            get => Instance.MaxDailyQuests;
            set => Instance.MaxDailyQuests = value;
        }

        /// <summary>
        /// How often to check for day crossing (in seconds)
        /// Default: 60 seconds
        /// Lower values = more responsive but more CPU usage
        /// </summary>
        public static float DayCrossingCheckInterval
        {
            get => Instance.DayCrossingCheckInterval;
            set => Instance.DayCrossingCheckInterval = value;
        }

        // ===== Registration =====

        /// <summary>
        /// Register a quest pool
        /// Pool.Initialize() may be called immediately or deferred
        /// </summary>
        /// <param name="pool">Quest pool to register</param>
        public static void RegisterQuestPool(IDailyQuestPool pool)
        {
            if (pool == null)
            {
                Debug.LogError("[DailyQuestManager] Cannot register null quest pool");
                return;
            }

            Instance.RegisterQuestPool(pool);
        }

        /// <summary>
        /// Unregister a quest pool
        /// Does not affect currently active quests from this pool
        /// </summary>
        /// <param name="poolId">Pool ID to unregister</param>
        public static void UnregisterQuestPool(string poolId)
        {
            if (string.IsNullOrEmpty(poolId))
            {
                Debug.LogError("[DailyQuestManager] Cannot unregister pool with null or empty ID");
                return;
            }

            Instance.UnregisterQuestPool(poolId);
        }

        // ===== Quest Management =====

        /// <summary>
        /// Get all currently active daily quests (not completed)
        /// </summary>
        /// <returns>List of active quests (never null)</returns>
        public static List<IDailyQuest> GetActiveQuests()
        {
            return Instance.GetActiveQuests();
        }

        /// <summary>
        /// Get all completed quests for today
        /// </summary>
        /// <returns>List of completed quests (never null)</returns>
        public static List<IDailyQuest> GetCompletedQuests()
        {
            return Instance.GetCompletedQuests();
        }

        /// <summary>
        /// Get quest by ID from active or completed lists
        /// Returns null if not found
        /// </summary>
        /// <param name="questId">Quest ID to search for</param>
        /// <returns>Quest instance or null</returns>
        public static IDailyQuest GetQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
                return null;

            return Instance.GetQuest(questId);
        }

        /// <summary>
        /// Mark an event-driven quest as completed
        /// Framework will verify completion via IsCompleted() and call OnCompleted()
        /// For scanning quests, this is called automatically
        /// </summary>
        /// <param name="questId">Quest ID to complete</param>
        public static void CompleteQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogError("[DailyQuestManager] Cannot complete quest with null or empty ID");
                return;
            }

            Instance.CompleteQuest(questId);
        }

        /// <summary>
        /// Check if a quest is currently active (not completed)
        /// </summary>
        /// <param name="questId">Quest ID to check</param>
        /// <returns>True if quest is active</returns>
        public static bool IsQuestActive(string questId)
        {
            if (string.IsNullOrEmpty(questId))
                return false;

            return Instance.IsQuestActive(questId);
        }

        /// <summary>
        /// Check if a quest is completed today
        /// </summary>
        /// <param name="questId">Quest ID to check</param>
        /// <returns>True if quest is completed</returns>
        public static bool IsQuestCompleted(string questId)
        {
            if (string.IsNullOrEmpty(questId))
                return false;

            return Instance.IsQuestCompleted(questId);
        }

        // ===== Time & Refresh =====

        /// <summary>
        /// Last time quests were refreshed (date only, midnight UTC)
        /// </summary>
        public static DateTime LastRefreshTime => Instance.LastRefreshTime;

        /// <summary>
        /// Time remaining until next refresh (next midnight UTC)
        /// </summary>
        public static TimeSpan TimeUntilNextRefresh => Instance.TimeUntilNextRefresh;

        /// <summary>
        /// Manually trigger quest refresh
        /// WARNING: This will expire all current quests
        /// Mainly for testing/debugging
        /// </summary>
        public static void ForceRefresh()
        {
            Instance.ForceRefresh();
        }

        // ===== Events =====

        /// <summary>
        /// Fired when quests are refreshed for a new day
        /// </summary>
        public static event Action OnQuestsRefreshed
        {
            add => Instance.OnQuestsRefreshed += value;
            remove => Instance.OnQuestsRefreshed -= value;
        }

        /// <summary>
        /// Fired when a quest is completed
        /// </summary>
        public static event Action<IDailyQuest> OnQuestCompleted
        {
            add => Instance.OnQuestCompleted += value;
            remove => Instance.OnQuestCompleted -= value;
        }

        /// <summary>
        /// Fired when a quest expires without completion
        /// </summary>
        public static event Action<IDailyQuest> OnQuestExpired
        {
            add => Instance.OnQuestExpired += value;
            remove => Instance.OnQuestExpired -= value;
        }

        // ===== Internal Cleanup =====

        /// <summary>
        /// Internal method to set the instance (for testing or manual instantiation)
        /// </summary>
        internal static void SetInstance(DailyQuestManagerInternal instance)
        {
            _instance = instance;
        }
    }
}
