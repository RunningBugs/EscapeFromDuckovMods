using System;
using UnityEngine;
using DailyQuestMod.Framework;
using Duckov.Economy;

namespace DailyQuestMod.Examples.Quests
{
    /// <summary>
    /// Simple test quest for framework verification
    /// Completes after 10 seconds of being active (scanning mode)
    /// </summary>
    public class TestQuest : IDailyQuest
    {
        private float activationTime;
        private const float COMPLETION_TIME = 10f;
        private static int activationCounter = 0;

        public string QuestId => "test_simple_quest";
        public string Title => "Test Quest";
        public string Description
        {
            get
            {
                if (activationTime == 0)
                    return "Wait 10 seconds (not started)";

                float elapsed = Time.time - activationTime;
                float remaining = Mathf.Max(0, COMPLETION_TIME - elapsed);
                return $"Wait 10 seconds (remaining: {remaining:F1}s)";
            }
        }

        public QuestCheckMode CheckMode => QuestCheckMode.Scanning;
        public float ScanInterval => 0.5f;

        public bool IsCompleted()
        {
            if (activationTime == 0)
                return false;

            return (Time.time - activationTime) >= COMPLETION_TIME;
        }

        public void OnActivated()
        {
            activationTime = Time.time;
            activationCounter++;
            Debug.Log($"[TestQuest] Activated #{activationCounter} at Time.time={activationTime}");
        }

        public void OnCompleted()
        {
            Debug.Log($"[TestQuest] Completed! Elapsed time: {Time.time - activationTime:F1}s");

            // Give test reward
            try
            {
                bool success = EconomyManager.Add(100);
                if (success)
                {
                    Debug.Log("[TestQuest] Gave $100 test reward to player");
                }
                else
                {
                    Debug.LogError("[TestQuest] Failed to give money reward");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[TestQuest] Error giving reward: {e}");
            }
        }

        public void OnExpired()
        {
            Debug.Log($"[TestQuest] Expired without completion");
        }

        public object GetSaveData()
        {
            return activationTime;
        }

        public void LoadSaveData(object data)
        {
            if (data is float time)
            {
                activationTime = time;
                Debug.Log($"[TestQuest] Restored activation time: {activationTime}");
            }
        }
    }
}
