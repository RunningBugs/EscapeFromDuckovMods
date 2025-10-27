using System;
using UnityEngine;
using DailyQuestMod.Framework;
using DailyQuestMod.Examples.Pools;
using DailyQuestMod.Integration;
using DailyQuestMod.Helpers;

namespace DailyQuestMod
{
    /// <summary>
    /// Main mod entry point
    /// Initializes the Daily Quest Framework
    ///
    /// Debug Keybinds:
    /// - K: Open Daily Quest UI
    /// - L: Manually check and complete quests
    /// - R: Reset saved state and force refresh
    /// - F12: Toggle debug logging (reduces log spam when OFF)
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private bool trophyIntegrationAttempted = false;
        private float nextIntegrationAttempt = 0f;
        private float nextDebugCheck = 0f;
        private int updateCallCount = 0;

        protected override void OnAfterSetup()
        {
            try
            {
                Debug.Log("[DailyQuestMod] Initializing Daily Quest Framework...");

                // Force manager recreation by clearing any stale instance
                DailyQuestManager.SetInstance(null);
                Debug.Log("[DailyQuestMod] Cleared any stale manager instance");

                // Create UI instance
                Integration.UI.DailyQuestGiverView.CreateInstance();

                // Force manager creation by accessing it
                var activeQuests = DailyQuestManager.GetActiveQuests();
                Debug.Log($"[DailyQuestMod] Manager accessed, active quests: {activeQuests.Count}");

                // Register test pool for demo
                Debug.Log("[DailyQuestMod] Registering test quest pool for demo");
                DailyQuestManager.RegisterQuestPool(new TestQuestPool());

                // Subscribe to building events for trophy integration
                Duckov.Buildings.BuildingManager.OnBuildingBuilt += OnBuildingBuilt;

                Debug.Log("[DailyQuestMod] Daily Quest Framework initialized successfully");
                Debug.Log("[DailyQuestMod] Modders can now register quest pools via DailyQuestManager.RegisterQuestPool()");
                Debug.Log("[DailyQuestMod] Will integrate with Commemorative Trophy when available");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestMod] Failed to initialize: {e}");
            }
        }

        private void Update()
        {
            updateCallCount++;

            // Debug check every 3 seconds (throttled to reduce spam)
            if (Time.time >= nextDebugCheck)
            {
                nextDebugCheck = Time.time + 3f;
                var activeQuests = DailyQuestManager.GetActiveQuests();

                // Only log if in debug mode or if quest count changed
                LogHelper.LogThrottled($"ModBehaviour Update running. Calls: {updateCallCount}, Active quests: {activeQuests.Count}, Time: {Time.time:F1}", 10f, "DailyQuestMod");

                // Log individual quest states only in debug mode
                if (LogHelper.DebugMode)
                {
                    foreach (var quest in activeQuests)
                    {
                        Debug.Log($"[DailyQuestMod]   - {quest.QuestId}: {quest.Title}, IsCompleted: {quest.IsCompleted()}");
                    }
                }
            }

            // Attempt trophy integration periodically until successful
            if (!trophyIntegrationAttempted || !TrophyIntegration.IsIntegrated())
            {
                if (Time.time >= nextIntegrationAttempt)
                {
                    nextIntegrationAttempt = Time.time + 5f; // Try every 5 seconds
                    TrophyIntegration.IntegrateWithTrophy();

                    if (TrophyIntegration.IsIntegrated())
                    {
                        trophyIntegrationAttempted = true;
                        Debug.Log("[DailyQuestMod] Trophy integration successful, stopping retry attempts");
                    }
                }
            }

            // Test keybind: Press 'K' to open daily quest UI directly (for testing)
            if (Input.GetKeyDown(KeyCode.K))
            {
                TestOpenDailyQuestUI();
            }

            // Test keybind: Press 'L' to manually check quest completion
            if (Input.GetKeyDown(KeyCode.L))
            {
                Debug.Log("[DailyQuestMod] ===== MANUAL QUEST CHECK (L key) =====");
                var quests = DailyQuestManager.GetActiveQuests();
                foreach (var quest in quests)
                {
                    bool completed = quest.IsCompleted();
                    Debug.Log($"[DailyQuestMod] Quest {quest.QuestId}: IsCompleted={completed}");
                    if (completed)
                    {
                        Debug.Log($"[DailyQuestMod] Manually calling CompleteQuest for {quest.QuestId}");
                        DailyQuestManager.CompleteQuest(quest.QuestId);
                    }
                }
            }

            // Test keybind: Press 'R' to reset and force refresh
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("[DailyQuestMod] ===== RESET AND FORCE REFRESH (R key) =====");

                // Clear saved state
                PlayerPrefs.DeleteKey("DailyQuests_SaveData");
                PlayerPrefs.Save();
                Debug.Log("[DailyQuestMod] Cleared saved state");

                // Force manager refresh
                DailyQuestManager.ForceRefresh();
                Debug.Log("[DailyQuestMod] Forced quest refresh");

                ShowNotification("Daily Quests Reset!\nQuests refreshed for today");
            }

            // Debug key: Press 'F12' to toggle debug logging
            if (Input.GetKeyDown(KeyCode.F12))
            {
                LogHelper.DebugMode = !LogHelper.DebugMode;
                Debug.Log($"[DailyQuestMod] Debug logging {(LogHelper.DebugMode ? "ENABLED" : "DISABLED")}");
                ShowNotification($"Debug Logging: {(LogHelper.DebugMode ? "ON" : "OFF")}");
            }
        }

        private void OnBuildingBuilt(int buildingId)
        {
            // When a building is built, try integration again
            Debug.Log($"[DailyQuestMod] Building built: {buildingId}, attempting trophy integration");
            TrophyIntegration.IntegrateWithTrophy();

            if (TrophyIntegration.IsIntegrated())
            {
                ShowNotification("Daily Quest System Ready!\nPress 'K' to open Daily Quests\nOr interact with Commemorative Trophy");
            }
        }

        private void TestOpenDailyQuestUI()
        {
            try
            {
                Debug.Log("[DailyQuestMod] Test keybind pressed - attempting to open Daily Quest UI");

                var view = Integration.UI.DailyQuestGiverView.Instance;
                if (view == null)
                {
                    Debug.LogWarning("[DailyQuestMod] DailyQuestGiverView.Instance is null");
                    ShowNotification("Daily Quest UI not initialized!");
                    return;
                }

                var giver = TrophyIntegration.GetDailyQuestGiver();
                if (giver == null)
                {
                    Debug.LogWarning("[DailyQuestMod] DailyQuestGiver not found, opening UI anyway");
                }

                view.Setup(giver);
                view.Open();
                Debug.Log("[DailyQuestMod] Daily Quest UI opened successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestMod] Error opening Daily Quest UI: {e}");
                ShowNotification($"Error opening Daily Quest UI: {e.Message}");
            }
        }

        private void ShowNotification(string message)
        {
            Debug.Log($"[DailyQuestMod] NOTIFICATION: {message}");
            // TODO: Integrate with game's notification system if available
        }

        protected override void OnBeforeDeactivate()
        {
            try
            {
                Debug.Log("[DailyQuestMod] Shutting down Daily Quest Framework...");

                // Unsubscribe from events
                Duckov.Buildings.BuildingManager.OnBuildingBuilt -= OnBuildingBuilt;

                // Cleanup will happen automatically when GameObject is destroyed
                // Events will be unsubscribed by garbage collection

                Debug.Log("[DailyQuestMod] Daily Quest Framework shutdown complete");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestMod] Error during shutdown: {e}");
            }
        }
    }
}
