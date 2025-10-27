using System;
using System.Collections.Generic;
using UnityEngine;
using DailyQuestMod.Framework;
using DailyQuestMod.Examples.Quests;

namespace DailyQuestMod.Examples.Pools
{
    /// <summary>
    /// Example quest pool for framework demonstration
    /// Provides test quests and a realistic example quest
    /// </summary>
    public class TestQuestPool : IDailyQuestPool
    {
        public string PoolId => "TestQuestPool";

        private List<IDailyQuest> allQuests;

        public void Initialize()
        {
            Debug.Log("[TestQuestPool] Initializing...");

            allQuests = new List<IDailyQuest>
            {
                new TestQuest(),
                new TestEventQuest(),
                new KillEnemiesQuest()  // Realistic example quest
            };

            Debug.Log($"[TestQuestPool] Initialized with {allQuests.Count} quests (2 test + 1 example)");
        }

        public List<IDailyQuest> SelectQuestsForToday(DateTime today)
        {
            Debug.Log($"[TestQuestPool] Selecting quests for {today:yyyy-MM-dd}");

            // For demo purposes, return all quests
            // In production, you would implement logic to select a subset
            var selectedQuests = new List<IDailyQuest>(allQuests);

            Debug.Log($"[TestQuestPool] Selected {selectedQuests.Count} quest(s) for today");
            foreach (var quest in selectedQuests)
            {
                Debug.Log($"[TestQuestPool]   - {quest.QuestId}: {quest.Title}");
            }

            return selectedQuests;
        }
    }
}
