using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Saves;
using DailyQuestMod.Helpers;

namespace DailyQuestMod.Framework
{
    /// <summary>
    /// Internal implementation of the daily quest manager
    /// MonoBehaviour that handles the actual quest lifecycle
    /// </summary>
    public class DailyQuestManagerInternal : MonoBehaviour
    {
        // ===== Configuration =====

        public int MaxDailyQuests = -1;
        public float DayCrossingCheckInterval = 60f;

        // ===== State =====

        private List<IDailyQuestPool> registeredPools = new List<IDailyQuestPool>();
        private List<QuestData> activeQuests = new List<QuestData>();
        private List<QuestData> completedQuests = new List<QuestData>();
        private DateTime lastRefreshDate;
        private float nextDayCrossingCheck;
        private bool initialized = false;
        private bool questsNeedActivation = false;
        private List<IDailyQuest> questsPendingActivation = new List<IDailyQuest>();
        private bool hasRunFirstUpdate = false;
        private bool questsArePaused = true; // Start paused until player enters a level

        // ===== Events =====

        public event Action OnQuestsRefreshed;
        public event Action<IDailyQuest> OnQuestCompleted;
        public event Action<IDailyQuest> OnQuestExpired;

        // ===== Properties =====

        public DateTime LastRefreshTime => lastRefreshDate;

        public TimeSpan TimeUntilNextRefresh
        {
            get
            {
                DateTime now = DateTime.UtcNow;
                DateTime nextMidnight = now.Date.AddDays(1);
                return nextMidnight - now;
            }
        }

        // ===== Quest Data Container =====

        private class QuestData
        {
            public IDailyQuest quest;
            public string poolId;
            public float nextScanTime;
            public bool completed;
        }

        // ===== Unity Lifecycle =====

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[DailyQuestManager] Awake called, GameObject: {gameObject.name}, enabled: {enabled}, activeInHierarchy: {gameObject.activeInHierarchy}");

            // Subscribe to level events
            try
            {
                LevelManager.OnAfterLevelInitialized += OnLevelEntered;
                LevelManager.OnEvacuated += OnLevelExited;
                LevelManager.OnMainCharacterDead += OnPlayerDied;
                Debug.Log("[DailyQuestManager] Subscribed to LevelManager events");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestManager] Failed to subscribe to LevelManager events: {e}");
            }

            // Subscribe to game's save system
            try
            {
                SavesSystem.OnCollectSaveData += SaveState;
                SavesSystem.OnSetFile += OnSaveFileChanged;
                Debug.Log("[DailyQuestManager] Subscribed to SavesSystem events");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestManager] Failed to subscribe to SavesSystem events: {e}");
            }

            Initialize();
        }

        private void Initialize()
        {
            if (initialized)
                return;

            initialized = true;
            lastRefreshDate = DateTime.UtcNow.Date;
            nextDayCrossingCheck = Time.time + DayCrossingCheckInterval;

            Debug.Log("[DailyQuestManager] Initialized");

            // Load saved state
            LoadState();
        }

        private void Update()
        {
            // Mark that we've entered Update loop (Unity is fully initialized)
            if (!hasRunFirstUpdate)
            {
                hasRunFirstUpdate = true;
                Debug.Log($"[DailyQuestManager] First Update frame reached at Time.time={Time.time:F3}");
            }

            // Activate any pending quests (deferred from initialization when Unity logging wasn't ready)
            if (questsNeedActivation && questsPendingActivation.Count > 0)
            {
                Debug.Log($"[DailyQuestManager] ===== Activating {questsPendingActivation.Count} pending quests (Unity is now ready) =====");
                questsNeedActivation = false;

                foreach (var quest in questsPendingActivation.ToList())
                {
                    try
                    {
                        Debug.Log($"[DailyQuestManager] >>> BEFORE OnActivated() for: {quest.QuestId}, Quest type: {quest.GetType().Name}");
                        Debug.Log($"[DailyQuestManager] >>> Quest instance: {quest.GetHashCode()}, Title: {quest.Title}");
                        quest.OnActivated();
                        Debug.Log($"[DailyQuestManager] <<< AFTER OnActivated() for: {quest.QuestId} - SUCCESS");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[DailyQuestManager] <<< AFTER OnActivated() for: {quest.QuestId} - EXCEPTION: {e}");
                    }
                }

                questsPendingActivation.Clear();
                Debug.Log("[DailyQuestManager] All pending quests activated");

                // Trigger refresh event since quests are now fully active
                OnQuestsRefreshed?.Invoke();
            }

            // Check for day crossing periodically
            if (Time.time >= nextDayCrossingCheck)
            {
                nextDayCrossingCheck = Time.time + DayCrossingCheckInterval;

                if (HasDayCrossed())
                {
                    Debug.Log("[DailyQuestManager] Day crossed, refreshing quests");
                    RefreshDailyQuests(null); // No saved state to restore, it's a new day
                }
            }

            // Poll scanning quests (only if not paused)
            if (!questsArePaused)
            {
                foreach (var questData in activeQuests.ToList()) // ToList to avoid modification during iteration
                {
                    if (questData.completed)
                        continue;

                    if (questData.quest.CheckMode != QuestCheckMode.Scanning)
                        continue;

                    if (Time.time >= questData.nextScanTime)
                    {
                        questData.nextScanTime = Time.time + questData.quest.ScanInterval;

                        try
                        {
                            if (questData.quest.IsCompleted())
                            {
                                CompleteQuestInternal(questData);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[DailyQuestManager] Error checking quest {questData.quest.QuestId}: {e}");
                        }
                    }
                }
            }
        }

        // ===== Level Event Handlers =====

        private void OnLevelEntered()
        {
            questsArePaused = false;
            Debug.Log("[DailyQuestManager] Player entered level - Quests RESUMED");
        }

        private void OnLevelExited(EvacuationInfo info)
        {
            questsArePaused = true;
            Debug.Log("[DailyQuestManager] Player evacuated - Quests PAUSED");
        }

        private void OnPlayerDied(DamageInfo damageInfo)
        {
            questsArePaused = true;
            Debug.Log("[DailyQuestManager] Player died - Quests PAUSED");
        }

        private void OnSaveFileChanged()
        {
            Debug.Log("[DailyQuestManager] Save file changed - reloading quest state");

            // Clear current in-memory state
            activeQuests.Clear();
            completedQuests.Clear();

            // Reload from saved state
            LoadState();

            // If we have saved state, restore it
            if (savedStateToRestore != null)
            {
                RestoreQuestState(savedStateToRestore);
                savedStateToRestore = null;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            try
            {
                LevelManager.OnAfterLevelInitialized -= OnLevelEntered;
                LevelManager.OnEvacuated -= OnLevelExited;
                LevelManager.OnMainCharacterDead -= OnPlayerDied;
                Debug.Log("[DailyQuestManager] Unsubscribed from LevelManager events");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestManager] Error unsubscribing from events: {e}");
            }

            // Unsubscribe from save system
            try
            {
                SavesSystem.OnCollectSaveData -= SaveState;
                SavesSystem.OnSetFile -= OnSaveFileChanged;
                Debug.Log("[DailyQuestManager] Unsubscribed from SavesSystem events");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestManager] Error unsubscribing from SavesSystem events: {e}");
            }
        }

        // ===== Day Crossing Detection =====

        private bool HasDayCrossed()
        {
            DateTime now = DateTime.UtcNow.Date;

            // Handle time going backwards
            if (now < lastRefreshDate)
            {
                Debug.LogWarning("[DailyQuestManager] System time went backwards, resetting to current time");
                lastRefreshDate = now;
                return false;
            }

            return now > lastRefreshDate;
        }

        // ===== Quest Refresh =====

        public void ForceRefresh()
        {
            Debug.Log("[DailyQuestManager] Force refresh requested");
            RefreshDailyQuests(null); // Don't restore state on force refresh
        }

        private void RefreshDailyQuests(SaveData previousState = null)
        {
            Debug.Log($"[DailyQuestManager] ===== REFRESHING DAILY QUESTS ===== Date: {DateTime.UtcNow.Date}, Time.time: {Time.time:F1}");

            // Expire old quests (only if they're from a previous day)
            bool isDifferentDay = previousState != null && HasDayCrossed();
            if (isDifferentDay)
            {
                foreach (var questData in activeQuests.ToList())
                {
                    if (!questData.completed)
                    {
                        try
                        {
                            questData.quest.OnExpired();
                            OnQuestExpired?.Invoke(questData.quest);
                            Debug.Log($"[DailyQuestManager] Quest expired: {questData.quest.QuestId}");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[DailyQuestManager] Error expiring quest {questData.quest.QuestId}: {e}");
                        }
                    }
                }
            }

            // Clear old quests
            activeQuests.Clear();
            completedQuests.Clear();

            // Collect new quests from all pools
            List<IDailyQuest> allSelectedQuests = new List<IDailyQuest>();
            DateTime today = DateTime.UtcNow;

            foreach (var pool in registeredPools.ToList())
            {
                try
                {
                    var selectedQuests = pool.SelectQuestsForToday(today);
                    if (selectedQuests != null && selectedQuests.Count > 0)
                    {
                        foreach (var quest in selectedQuests)
                        {
                            if (quest != null)
                            {
                                // Check for duplicate quest IDs
                                if (allSelectedQuests.Any(q => q.QuestId == quest.QuestId))
                                {
                                    Debug.LogError($"[DailyQuestManager] Duplicate quest ID '{quest.QuestId}' from pool '{pool.PoolId}', skipping");
                                    continue;
                                }

                                allSelectedQuests.Add(quest);
                            }
                        }

                        Debug.Log($"[DailyQuestManager] Pool '{pool.PoolId}' provided {selectedQuests.Count} quests");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DailyQuestManager] Error selecting quests from pool {pool.PoolId}: {e}");
                }
            }

            // Apply max quest limit if needed
            if (MaxDailyQuests > 0 && allSelectedQuests.Count > MaxDailyQuests)
            {
                Debug.Log($"[DailyQuestManager] Limiting quests from {allSelectedQuests.Count} to {MaxDailyQuests}");
                allSelectedQuests = SampleRandom(allSelectedQuests, MaxDailyQuests);
            }

            // Always defer activation to Update to avoid Unity logging initialization issues
            bool deferActivation = !hasRunFirstUpdate;

            if (deferActivation)
            {
                Debug.Log($"[DailyQuestManager] Deferring quest activation to first Update frame (Time.time={Time.time:F3})");
            }

            // Activate selected quests (or defer if Update hasn't run yet)
            foreach (var quest in allSelectedQuests)
            {
                try
                {
                    if (deferActivation)
                    {
                        // Defer activation until first Update frame
                        Debug.Log($"[DailyQuestManager] Quest {quest.QuestId} added to pending activation list");
                        questsPendingActivation.Add(quest);
                    }
                    else
                    {
                        // Activate immediately
                        Debug.Log($"[DailyQuestManager] >>> IMMEDIATE OnActivated() for: {quest.QuestId}, Quest type: {quest.GetType().Name}");
                        Debug.Log($"[DailyQuestManager] >>> Quest instance: {quest.GetHashCode()}, Title: {quest.Title}");
                        quest.OnActivated();
                        Debug.Log($"[DailyQuestManager] <<< IMMEDIATE OnActivated() completed for: {quest.QuestId} - SUCCESS");
                    }

                    float nextScan = Time.time + quest.ScanInterval;
                    var questData = new QuestData
                    {
                        quest = quest,
                        poolId = FindPoolIdForQuest(quest),
                        nextScanTime = nextScan,
                        completed = false
                    };

                    activeQuests.Add(questData);
                    Debug.Log($"[DailyQuestManager] Quest activated: {quest.QuestId} ({quest.Title}), Mode: {quest.CheckMode}, ScanInterval: {quest.ScanInterval}, NextScanTime: {nextScan:F1}, Current Time.time: {Time.time:F1}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DailyQuestManager] Error activating quest {quest.QuestId}: {e}");
                }
            }

            if (deferActivation && questsPendingActivation.Count > 0)
            {
                questsNeedActivation = true;
                Debug.Log($"[DailyQuestManager] {questsPendingActivation.Count} quests will be activated in first Update frame");
            }

            // Restore quest state if loading from same day
            if (previousState != null && !isDifferentDay)
            {
                Debug.Log($"[DailyQuestManager] Restoring quest state from same day");
                RestoreQuestState(previousState);
            }

            lastRefreshDate = DateTime.UtcNow.Date;

            Debug.Log($"[DailyQuestManager] Refresh complete, {activeQuests.Count} active, {completedQuests.Count} completed");

            OnQuestsRefreshed?.Invoke();
            SaveState();
        }

        private void RestoreQuestState(SaveData saveData)
        {
            if (saveData == null)
                return;

            int restoredCount = 0;

            // Restore from activeQuests list (contains quest-specific data)
            if (saveData.activeQuests != null && saveData.activeQuests.Count > 0)
            {
                Debug.Log($"[DailyQuestManager] Attempting to restore {saveData.activeQuests.Count} quest states from activeQuests");

                foreach (var savedQuest in saveData.activeQuests)
                {
                    var questData = activeQuests.FirstOrDefault(q => q.quest.QuestId == savedQuest.questId);
                    if (questData == null)
                    {
                        Debug.LogWarning($"[DailyQuestManager] Saved quest '{savedQuest.questId}' not found in current active quests");
                        continue;
                    }

                    try
                    {
                        // Restore quest-specific data
                        if (!string.IsNullOrEmpty(savedQuest.questDataJson))
                        {
                            try
                            {
                                // Try to parse as primitive types first (most common case)
                                if (float.TryParse(savedQuest.questDataJson, out float floatVal))
                                {
                                    questData.quest.LoadSaveData(floatVal);
                                    Debug.Log($"[DailyQuestManager] Restored float data for quest '{savedQuest.questId}': {floatVal}");
                                }
                                else if (int.TryParse(savedQuest.questDataJson, out int intVal))
                                {
                                    questData.quest.LoadSaveData(intVal);
                                    Debug.Log($"[DailyQuestManager] Restored int data for quest '{savedQuest.questId}': {intVal}");
                                }
                                else
                                {
                                    // Try as JSON object - create new instance of the type
                                    var sampleData = questData.quest.GetSaveData();
                                    if (sampleData != null)
                                    {
                                        var restoredData = JsonUtility.FromJson(savedQuest.questDataJson, sampleData.GetType());
                                        questData.quest.LoadSaveData(restoredData);
                                        Debug.Log($"[DailyQuestManager] Restored object data for quest '{savedQuest.questId}'");
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning($"[DailyQuestManager] Could not restore quest data for '{savedQuest.questId}': {e.Message}");
                            }
                        }

                        // If quest was marked as completed in the save data, mark it as completed
                        if (savedQuest.completed)
                        {
                            questData.completed = true;
                            completedQuests.Add(questData);
                            restoredCount++;
                            Debug.Log($"[DailyQuestManager] Restored quest '{savedQuest.questId}' as completed (from activeQuests)");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[DailyQuestManager] Error restoring quest state for '{savedQuest.questId}': {e}");
                    }
                }
            }

            // Also restore from completedQuestIds list (backup/legacy support)
            if (saveData.completedQuestIds != null && saveData.completedQuestIds.Count > 0)
            {
                Debug.Log($"[DailyQuestManager] Attempting to restore {saveData.completedQuestIds.Count} completed quests from completedQuestIds");

                foreach (var completedQuestId in saveData.completedQuestIds)
                {
                    var questData = activeQuests.FirstOrDefault(q => q.quest.QuestId == completedQuestId);
                    if (questData == null)
                    {
                        Debug.LogWarning($"[DailyQuestManager] Completed quest '{completedQuestId}' not found in current active quests");
                        continue;
                    }

                    // Only mark as completed if not already marked
                    if (!questData.completed)
                    {
                        questData.completed = true;
                        completedQuests.Add(questData);
                        restoredCount++;
                        Debug.Log($"[DailyQuestManager] Restored quest '{completedQuestId}' as completed (from completedQuestIds)");
                    }
                }
            }

            // Remove completed quests from active list
            activeQuests.RemoveAll(q => q.completed);

            Debug.Log($"[DailyQuestManager] Quest state restoration complete: {activeQuests.Count} active, {completedQuests.Count} completed ({restoredCount} restored)");
        }

        private string FindPoolIdForQuest(IDailyQuest quest)
        {
            // Try to find which pool this quest came from
            // This is a best-effort attempt for save/load purposes
            foreach (var pool in registeredPools)
            {
                try
                {
                    var allQuests = pool.SelectQuestsForToday(DateTime.UtcNow);
                    if (allQuests != null && allQuests.Any(q => q?.QuestId == quest.QuestId))
                    {
                        return pool.PoolId;
                    }
                }
                catch
                {
                    // Ignore errors when trying to find pool
                }
            }

            return "unknown";
        }

        // ===== Quest Completion =====

        public void CompleteQuest(string questId)
        {
            var questData = activeQuests.FirstOrDefault(q => q.quest.QuestId == questId);
            if (questData == null)
            {
                Debug.LogWarning($"[DailyQuestManager] Cannot complete quest '{questId}': not found in active quests");
                return;
            }

            if (questData.completed)
            {
                Debug.LogWarning($"[DailyQuestManager] Quest '{questId}' is already completed");
                return;
            }

            CompleteQuestInternal(questData);
        }

        private void CompleteQuestInternal(QuestData questData)
        {
            // Verify quest is actually completed
            try
            {
                if (!questData.quest.IsCompleted())
                {
                    Debug.LogWarning($"[DailyQuestManager] Quest '{questData.quest.QuestId}' CompleteQuest called but IsCompleted() returns false");
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestManager] Error checking if quest {questData.quest.QuestId} is completed: {e}");
                return;
            }

            questData.completed = true;
            completedQuests.Add(questData);

            Debug.Log($"[DailyQuestManager] Quest completed: {questData.quest.QuestId} ({questData.quest.Title})");

            try
            {
                questData.quest.OnCompleted(); // Quest handles rewards itself
                OnQuestCompleted?.Invoke(questData.quest);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestManager] Error in quest OnCompleted callback {questData.quest.QuestId}: {e}");
            }

            // Don't save immediately - wait for game's save event
            // SaveState() is now called via SavesSystem.OnCollectSaveData
        }

        // ===== Pool Management =====

        public void RegisterQuestPool(IDailyQuestPool pool)
        {
            LogHelper.LogOnce($"RegisterQuestPool called for: {pool.PoolId}", "DailyQuestManager");
            LogHelper.LogDebug($"Current state: registeredPools={registeredPools.Count}, activeQuests={activeQuests.Count}, initialized={initialized}", "DailyQuestManager");

            if (registeredPools.Any(p => p.PoolId == pool.PoolId))
            {
                Debug.LogWarning($"[DailyQuestManager] Pool '{pool.PoolId}' is already registered");
                return;
            }

            registeredPools.Add(pool);
            Debug.Log($"[DailyQuestManager] Pool registered: {pool.PoolId}");

            // Initialize the pool
            try
            {
                pool.Initialize();
                Debug.Log($"[DailyQuestManager] Pool initialized: {pool.PoolId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestManager] Error initializing pool {pool.PoolId}: {e}");
            }

            // If this is the first pool and we haven't refreshed yet, do a refresh
            // Always refresh if no active quests, even if saved state exists
            LogHelper.LogDebug($"Checking refresh condition: registeredPools.Count={registeredPools.Count}, activeQuests.Count={activeQuests.Count}", "DailyQuestManager");
            if (registeredPools.Count == 1 && activeQuests.Count == 0)
            {
                Debug.Log($"[DailyQuestManager] First pool registered, triggering initial refresh (has saved state: {savedStateToRestore != null})");
                if (savedStateToRestore != null)
                {
                    Debug.Log($"[DailyQuestManager] Saved state exists: lastRefreshDate={savedStateToRestore.lastRefreshDate}, activeQuests count={savedStateToRestore.activeQuests?.Count ?? 0}");
                }
                RefreshDailyQuests(savedStateToRestore);
                savedStateToRestore = null; // Clear after use
            }
            else if (registeredPools.Count == 1 && activeQuests.Count > 0)
            {
                Debug.LogWarning($"[DailyQuestManager] First pool registered but activeQuests.Count = {activeQuests.Count}, skipping refresh - THIS IS A BUG!");
            }
            else
            {
                Debug.Log($"[DailyQuestManager] Not triggering refresh: multiple pools or already refreshed");
            }
        }

        public void UnregisterQuestPool(string poolId)
        {
            var pool = registeredPools.FirstOrDefault(p => p.PoolId == poolId);
            if (pool == null)
            {
                Debug.LogWarning($"[DailyQuestManager] Cannot unregister pool '{poolId}': not found");
                return;
            }

            registeredPools.Remove(pool);
            Debug.Log($"[DailyQuestManager] Pool unregistered: {poolId}");
        }

        // ===== Query Methods =====

        public List<IDailyQuest> GetActiveQuests()
        {
            return activeQuests
                .Where(q => !q.completed)
                .Select(q => q.quest)
                .ToList();
        }

        public List<IDailyQuest> GetCompletedQuests()
        {
            return completedQuests
                .Select(q => q.quest)
                .ToList();
        }

        public IDailyQuest GetQuest(string questId)
        {
            var questData = activeQuests.FirstOrDefault(q => q.quest.QuestId == questId);
            if (questData != null)
                return questData.quest;

            questData = completedQuests.FirstOrDefault(q => q.quest.QuestId == questId);
            return questData?.quest;
        }

        public bool IsQuestActive(string questId)
        {
            return activeQuests.Any(q => q.quest.QuestId == questId && !q.completed);
        }

        public bool IsQuestCompleted(string questId)
        {
            return completedQuests.Any(q => q.quest.QuestId == questId);
        }

        // ===== Utility Methods =====

        private List<T> SampleRandom<T>(List<T> list, int count)
        {
            if (list.Count <= count)
                return new List<T>(list);

            var random = new System.Random();
            return list.OrderBy(x => random.Next()).Take(count).ToList();
        }

        // ===== Save/Load =====

        [Serializable]
        private class SaveData
        {
            public string lastRefreshDate;
            public List<QuestSaveEntry> activeQuests;
            public List<string> completedQuestIds;
        }

        [Serializable]
        private class QuestSaveEntry
        {
            public string questId;
            public string poolId;
            public bool completed;
            public string questDataJson; // Serialized quest data
        }

        private void SaveState()
        {
            try
            {
                var saveData = new SaveData
                {
                    lastRefreshDate = lastRefreshDate.ToString("yyyy-MM-dd"),
                    activeQuests = new List<QuestSaveEntry>(),
                    completedQuestIds = new List<string>()
                };

                // Save active quests
                foreach (var questData in activeQuests)
                {
                    try
                    {
                        var questSaveData = questData.quest.GetSaveData();
                        var entry = new QuestSaveEntry
                        {
                            questId = questData.quest.QuestId,
                            poolId = questData.poolId,
                            completed = questData.completed,
                            questDataJson = questSaveData != null ? JsonUtility.ToJson(questSaveData) : null
                        };
                        saveData.activeQuests.Add(entry);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[DailyQuestManager] Error saving quest {questData.quest.QuestId}: {e}");
                    }
                }

                // Save completed quest IDs
                foreach (var questData in completedQuests)
                {
                    saveData.completedQuestIds.Add(questData.quest.QuestId);
                }

                string json = JsonUtility.ToJson(saveData, true);

                // Use SavesSystem to get current slot for slot-specific save data
                int saveSlot = SavesSystem.CurrentSlot;
                string saveKey = $"DailyQuests_SaveData_Slot{saveSlot}";
                PlayerPrefs.SetString(saveKey, json);
                PlayerPrefs.Save();

                Debug.Log($"[DailyQuestManager] State saved to slot {saveSlot}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestManager] Error saving state: {e}");
            }
        }

        private SaveData savedStateToRestore = null;

        private void LoadState()
        {
            try
            {
                LogHelper.LogOnce("LoadState called", "DailyQuestManager");

                // Use SavesSystem to get current slot for slot-specific save data
                int saveSlot = SavesSystem.CurrentSlot;
                string saveKey = $"DailyQuests_SaveData_Slot{saveSlot}";
                string json = PlayerPrefs.GetString(saveKey, null);

                if (string.IsNullOrEmpty(json))
                {
                    Debug.Log($"[DailyQuestManager] No saved state found for slot {saveSlot}, starting fresh");
                    savedStateToRestore = null;
                    return;
                }

                LogHelper.LogDebug($"Found saved state JSON for slot {saveSlot}: {json.Substring(0, Math.Min(100, json.Length))}...", "DailyQuestManager");

                savedStateToRestore = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"[DailyQuestManager] Deserialized saved state from slot {saveSlot}: lastRefreshDate={savedStateToRestore.lastRefreshDate}, activeQuests={savedStateToRestore.activeQuests?.Count ?? 0}, completedQuestIds={savedStateToRestore.completedQuestIds?.Count ?? 0}");

                // Parse last refresh date
                if (!string.IsNullOrEmpty(savedStateToRestore.lastRefreshDate))
                {
                    if (DateTime.TryParse(savedStateToRestore.lastRefreshDate, out DateTime savedDate))
                    {
                        lastRefreshDate = savedDate.Date;
                        DateTime now = DateTime.UtcNow.Date;
                        Debug.Log($"[DailyQuestManager] Loaded last refresh date: {lastRefreshDate}, current date: {now}");

                        // Check if we need to refresh
                        if (HasDayCrossed())
                        {
                            Debug.Log("[DailyQuestManager] Saved state is from previous day, quests will be expired and refreshed");
                        }
                        else
                        {
                            Debug.Log($"[DailyQuestManager] Same day - quest state will be restored ({savedStateToRestore.activeQuests?.Count ?? 0} quests)");
                        }
                        return;
                    }
                }

                Debug.Log("[DailyQuestManager] State loaded, will apply when pools register");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestManager] Error loading state: {e}");
                savedStateToRestore = null;
            }
        }
    }
}
