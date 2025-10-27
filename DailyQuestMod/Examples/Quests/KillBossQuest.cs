using System;
using UnityEngine;
using DailyQuestMod.Helpers;

namespace DailyQuestMod.Examples.Quests
{
    /// <summary>
    /// Daily quest to kill boss enemies
    /// Uses event-driven mode, subscribes to Health.OnDead event
    /// Checks for boss type using characterIconType
    /// </summary>
    public class KillBossQuest : SimpleEventQuest
    {
        private int killCount = 0;
        private int requiredKills = 1;

        // Configurable parameters
        private int moneyReward = 500;
        private int expReward = 100;

        // ===== Quest Identity =====

        public override string QuestId => "daily_kill_boss";
        public override string Title => "Boss Hunter";
        public override string Description => $"Defeat any boss creature ({killCount}/{requiredKills})\nReward: ${moneyReward} + {expReward} EXP";

        // ===== Quest Logic =====

        public override bool IsCompleted()
        {
            return killCount >= requiredKills;
        }

        public override void OnActivated()
        {
            base.OnActivated();

            // Reset progress
            killCount = 0;

            // Subscribe to death event
            Health.OnDead += OnEnemyDead;

            Debug.Log($"[KillBossQuest] Activated - kill {requiredKills} boss(es)");
        }

        public override void OnCompleted()
        {
            // Unsubscribe from event
            Health.OnDead -= OnEnemyDead;

            // Give rewards
            GiveMoneyReward(moneyReward);
            GiveExpReward(expReward);

            ShowCompletionNotification($"Boss Hunter Complete! Defeated {killCount} boss(es)");

            base.OnCompleted();
        }

        public override void OnExpired()
        {
            // Clean up event subscription
            Health.OnDead -= OnEnemyDead;

            ShowExpirationNotification($"Boss Hunter Expired (Progress: {killCount}/{requiredKills})");

            base.OnExpired();
        }

        // ===== Event Handlers =====

        private void OnEnemyDead(Health health, DamageInfo info)
        {
            try
            {
                // Check if killed by player
                if (info.fromCharacter == null || !info.fromCharacter.IsMainCharacter())
                    return;

                // Check if it's a boss
                if (!IsBoss(health))
                    return;

                // Increment kill count
                killCount++;
                LogProgress($"Boss killed! Progress: {killCount}/{requiredKills}");

                // Check if quest is complete
                NotifyCompleted();
            }
            catch (Exception e)
            {
                Debug.LogError($"[KillBossQuest] Error in OnEnemyDead: {e}");
            }
        }

        // ===== Helper Methods =====

        private bool IsBoss(Health health)
        {
            try
            {
                var character = health.TryGetCharacter();
                if (character == null)
                    return false;

                var preset = character.characterPreset;
                if (preset == null)
                    return false;

                // Check if character icon type is boss
                return preset.characterIconType == CharacterIconTypes.boss;
            }
            catch (Exception e)
            {
                Debug.LogError($"[KillBossQuest] Error checking if boss: {e}");
                return false;
            }
        }

        // ===== Save/Load =====

        public override object GetSaveData()
        {
            return killCount;
        }

        public override void LoadSaveData(object data)
        {
            if (data is int count)
            {
                killCount = count;
                Debug.Log($"[KillBossQuest] Restored kill count: {killCount}");
            }
        }

        // ===== Configuration =====

        /// <summary>
        /// Create a boss kill quest with custom parameters
        /// </summary>
        public KillBossQuest() : this(1, 500, 100)
        {
        }

        /// <summary>
        /// Create a boss kill quest with custom parameters
        /// </summary>
        /// <param name="required">Number of bosses to kill</param>
        /// <param name="money">Money reward</param>
        /// <param name="exp">Experience reward</param>
        public KillBossQuest(int required, int money, int exp)
        {
            requiredKills = required;
            moneyReward = money;
            expReward = exp;
        }
    }
}
