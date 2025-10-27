using System;
using System.Collections.Generic;

namespace DailyQuestMod.Framework
{
    /// <summary>
    /// Represents a pool of quests that can be selected from
    /// </summary>
    public interface IDailyQuestPool
    {
        /// <summary>
        /// Unique pool identifier
        /// Recommended: use a descriptive name like "MyModQuests"
        /// </summary>
        string PoolId { get; }

        /// <summary>
        /// Called once when pool is registered with the manager
        /// Use this to: load quest definitions, parse files, initialize data
        /// NOTE: May be called immediately on registration or delayed until first refresh
        /// </summary>
        void Initialize();

        /// <summary>
        /// Select which quests should be active today
        /// Called automatically when day refreshes (midnight UTC)
        ///
        /// Implementation should:
        /// 1. Apply selection logic (random, weighted, sequential, etc.)
        /// 2. Return list of quest instances to activate
        /// 3. Can return empty list if no quests available
        /// 4. Can return null (treated as empty list)
        ///
        /// Note: Quest instances should be fresh or properly reset for the new day
        /// </summary>
        /// <param name="today">Current date (UTC)</param>
        /// <returns>List of quests to activate for today, or null/empty if none</returns>
        List<IDailyQuest> SelectQuestsForToday(DateTime today);
    }
}
