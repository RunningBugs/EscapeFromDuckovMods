using System;
using UnityEngine;
using DailyQuestMod.Framework;
using Duckov.Economy;

namespace DailyQuestMod.Examples.Quests
{
    /// <summary>
    /// Daily quest: Kill a certain number of enemies
    /// Realistic quest that players can complete through normal gameplay
    /// </summary>
    public class KillEnemiesQuest : IDailyQuest
    {
        private int killCount = 0;
        private const int REQUIRED_KILLS = 10;

        public string QuestId => "daily_kill_enemies";
        public string Title => "Enemy Hunter";

        public string Description
        {
            get
            {
                if (killCount >= REQUIRED_KILLS)
                    return $"Kill enemies ({REQUIRED_KILLS}/{REQUIRED_KILLS}) - Complete!\nReward: $5000";
                return $"Kill enemies ({killCount}/{REQUIRED_KILLS})\nReward: $5000";
            }
        }

        public QuestCheckMode CheckMode => QuestCheckMode.EventDriven;
        public float ScanInterval => 0f; // Not used for event-driven

        public bool IsCompleted()
        {
            return killCount >= REQUIRED_KILLS;
        }

        public void OnActivated()
        {
            killCount = 0;
            Debug.Log($"[KillEnemiesQuest] Quest activated - Kill {REQUIRED_KILLS} enemies");

            // Subscribe to kill marker events
            try
            {
                // Unsubscribe first to prevent duplicate subscriptions
                HitMarker.OnKillMarker -= OnEnemyKilled;

                // Hook into the game's kill detection system
                HitMarker.OnKillMarker += OnEnemyKilled;
                Debug.Log("[KillEnemiesQuest] Subscribed to kill marker events");
            }
            catch (Exception e)
            {
                Debug.LogError($"[KillEnemiesQuest] Failed to subscribe to events: {e}");
            }
        }

        private void OnEnemyKilled()
        {
            // HitMarker.OnKillMarker is only called when the player kills an enemy
            // So we don't need to validate the attacker/victim

            // Increment kill count
            killCount++;
            Debug.Log($"[KillEnemiesQuest] Enemy killed! Progress: {killCount}/{REQUIRED_KILLS}");

            // Check if quest is complete
            if (IsCompleted())
            {
                Debug.Log($"[KillEnemiesQuest] Quest completed!");
                DailyQuestManager.CompleteQuest(QuestId);
            }
        }

        public void OnCompleted()
        {
            Debug.Log($"[KillEnemiesQuest] Quest completed with {killCount} kills!");

            // Unsubscribe from events
            try
            {
                HitMarker.OnKillMarker -= OnEnemyKilled;
                Debug.Log("[KillEnemiesQuest] Unsubscribed from kill marker events");
            }
            catch (Exception e)
            {
                Debug.LogError($"[KillEnemiesQuest] Error unsubscribing: {e}");
            }

            // Give rewards
            try
            {
                bool success = EconomyManager.Add(5000);
                if (success)
                {
                    Debug.Log("[KillEnemiesQuest] Gave $5000 reward to player");
                }
                else
                {
                    Debug.LogError("[KillEnemiesQuest] Failed to give money reward");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[KillEnemiesQuest] Error giving reward: {e}");
            }
        }

        public void OnExpired()
        {
            Debug.Log($"[KillEnemiesQuest] Quest expired with {killCount}/{REQUIRED_KILLS} kills");

            // Unsubscribe from events
            try
            {
                HitMarker.OnKillMarker -= OnEnemyKilled;
                Debug.Log("[KillEnemiesQuest] Unsubscribed from kill marker events");
            }
            catch (Exception e)
            {
                Debug.LogError($"[KillEnemiesQuest] Error unsubscribing: {e}");
            }
        }

        public object GetSaveData()
        {
            // Save current progress
            return killCount;
        }

        public void LoadSaveData(object data)
        {
            if (data is int count)
            {
                killCount = count;
                Debug.Log($"[KillEnemiesQuest] Restored progress: {killCount}/{REQUIRED_KILLS}");
            }
        }
    }
}
