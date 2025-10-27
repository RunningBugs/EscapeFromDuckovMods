using System;
using UnityEngine;
using DailyQuestMod.Helpers;

namespace DailyQuestMod.Examples.Quests
{
    /// <summary>
    /// Daily quest to kill specific mob types
    /// Uses event-driven mode, subscribes to Health.OnDead event
    /// Targets specific character preset, excludes bosses and friendly NPCs
    /// </summary>
    public class KillMobsQuest : SimpleEventQuest
    {
        private int killCount = 0;
        private int requiredKills = 5;
        private string targetMobName = "Any Enemy";
        private CharacterRandomPreset targetPreset = null;

        // Configurable parameters
        private int moneyReward = 300;
        private int expReward = 50;

        // ===== Quest Identity =====

        public override string QuestId => $"daily_kill_mobs_{targetMobName.Replace(" ", "_").ToLower()}";
        public override string Title => $"Mob Slayer: {targetMobName}";
        public override string Description => $"Eliminate {targetMobName} enemies ({killCount}/{requiredKills})\nReward: ${moneyReward} + {expReward} EXP";

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

            Debug.Log($"[KillMobsQuest] Activated - kill {requiredKills} {targetMobName}");
        }

        public override void OnCompleted()
        {
            // Unsubscribe from event
            Health.OnDead -= OnEnemyDead;

            // Give rewards
            GiveMoneyReward(moneyReward);
            GiveExpReward(expReward);

            ShowCompletionNotification($"Mob Slayer Complete! Eliminated {killCount} {targetMobName}");

            base.OnCompleted();
        }

        public override void OnExpired()
        {
            // Clean up event subscription
            Health.OnDead -= OnEnemyDead;

            ShowExpirationNotification($"Mob Slayer Expired (Progress: {killCount}/{requiredKills})");

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

                // Check if it's a valid target
                if (!IsValidTarget(health))
                    return;

                // Increment kill count
                killCount++;
                LogProgress($"{targetMobName} killed! Progress: {killCount}/{requiredKills}");

                // Check if quest is complete
                NotifyCompleted();
            }
            catch (Exception e)
            {
                Debug.LogError($"[KillMobsQuest] Error in OnEnemyDead: {e}");
            }
        }

        // ===== Helper Methods =====

        private bool IsValidTarget(Health health)
        {
            try
            {
                var character = health.TryGetCharacter();
                if (character == null)
                    return false;

                var preset = character.characterPreset;
                if (preset == null)
                    return false;

                // Exclude bosses
                if (preset.characterIconType == CharacterIconTypes.boss)
                    return false;

                // Exclude friendly NPCs (player team)
                if (health.team == Teams.player)
                    return false;

                // If specific preset required, check for match
                if (targetPreset != null)
                {
                    return preset.nameKey == targetPreset.nameKey;
                }

                // If no specific preset, any non-boss, non-friendly enemy is valid
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[KillMobsQuest] Error checking if valid target: {e}");
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
                Debug.Log($"[KillMobsQuest] Restored kill count: {killCount}");
            }
        }

        // ===== Configuration =====

        /// <summary>
        /// Create a kill mobs quest with default parameters (any enemy)
        /// </summary>
        public KillMobsQuest() : this(null, "Any Enemy", 5, 300, 50)
        {
        }

        /// <summary>
        /// Create a kill mobs quest targeting a specific preset
        /// </summary>
        /// <param name="preset">Target character preset (null = any non-boss enemy)</param>
        /// <param name="mobName">Display name for the mob type</param>
        /// <param name="required">Number of mobs to kill</param>
        /// <param name="money">Money reward</param>
        /// <param name="exp">Experience reward</param>
        public KillMobsQuest(CharacterRandomPreset preset, string mobName, int required, int money, int exp)
        {
            targetPreset = preset;
            targetMobName = mobName;
            requiredKills = required;
            moneyReward = money;
            expReward = exp;
        }
    }
}
