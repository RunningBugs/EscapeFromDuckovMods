using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Duckov.UI;
using DailyQuestMod.Framework;

namespace DailyQuestMod.Integration.UI
{
    /// <summary>
    /// UI View for displaying daily quests
    /// Uses IMGUI for rendering
    /// Shows available and completed daily quests
    /// </summary>
    public class DailyQuestGiverView : MonoBehaviour
    {
        // ===== Singleton =====

        private static DailyQuestGiverView _instance;
        public static DailyQuestGiverView Instance => _instance;

        // ===== UI State =====

        public bool open = false;
        private Rect windowRect = new Rect(Screen.width / 2 - 400, Screen.height / 2 - 300, 800, 600);
        private Vector2 scrollPosition = Vector2.zero;
        private GUIStyle windowStyle;
        private GUIStyle buttonStyle;
        private GUIStyle labelStyle;
        private GUIStyle boxStyle;
        private bool stylesInitialized = false;

        // ===== References =====

        private DailyQuestGiver target;
        private List<DailyQuestEntry> activeEntries = new List<DailyQuestEntry>();
        private DailyQuestEntry selectedEntry;
        private bool showingCompleted = false;

        // ===== Unity Lifecycle =====

        /// <summary>
        /// Create the DailyQuestGiverView instance if it doesn't exist
        /// Call this during mod initialization
        /// </summary>
        public static void CreateInstance()
        {
            if (_instance != null)
            {
                Debug.Log("[DailyQuestGiverView] Instance already exists");
                return;
            }

            try
            {
                var go = new GameObject("DailyQuestGiverView");
                _instance = go.AddComponent<DailyQuestGiverView>();
                DontDestroyOnLoad(go);
                Debug.Log("[DailyQuestGiverView] Instance created successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestGiverView] Failed to create instance: {e}");
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[DailyQuestGiverView] Multiple instances detected, destroying duplicate");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            Debug.Log("[DailyQuestGiverView] Awake called, instance set");

            // Subscribe to level events to close UI when exiting level
            try
            {
                LevelManager.OnEvacuated += OnLevelExited;
                LevelManager.OnMainCharacterDead += OnPlayerDied;
                Debug.Log("[DailyQuestGiverView] Subscribed to LevelManager events");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestGiverView] Failed to subscribe to LevelManager events: {e}");
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            // Unsubscribe from quest events
            DailyQuestManager.OnQuestsRefreshed -= OnQuestsRefreshed;
            DailyQuestManager.OnQuestCompleted -= OnQuestCompleted;

            // Unsubscribe from level events
            try
            {
                LevelManager.OnEvacuated -= OnLevelExited;
                LevelManager.OnMainCharacterDead -= OnPlayerDied;
                Debug.Log("[DailyQuestGiverView] Unsubscribed from LevelManager events");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestGiverView] Error unsubscribing from LevelManager events: {e}");
            }
        }

        // ===== Level Event Handlers =====

        private void OnLevelExited(EvacuationInfo info)
        {
            if (open)
            {
                Debug.Log("[DailyQuestGiverView] Level exited - closing UI");
                Close();
            }
        }

        private void OnPlayerDied(DamageInfo damageInfo)
        {
            if (open)
            {
                Debug.Log("[DailyQuestGiverView] Player died - closing UI");
                Close();
            }
        }

        // ===== IMGUI Rendering =====

        private void OnGUI()
        {
            if (!open)
                return;

            InitializeStyles();

            // Draw window
            windowRect = GUILayout.Window(12345, windowRect, DrawWindow, "Daily Quests", windowStyle);
        }

        private void InitializeStyles()
        {
            if (stylesInitialized)
                return;

            windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.fontSize = 16;
            windowStyle.fontStyle = FontStyle.Bold;

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 14;
            buttonStyle.padding = new RectOffset(10, 10, 5, 5);

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 14;
            labelStyle.wordWrap = true;

            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.padding = new RectOffset(10, 10, 10, 10);

            stylesInitialized = true;
        }

        private void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            // Tabs
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(showingCompleted ? "Active Quests" : "Active Quests (Current)", buttonStyle, GUILayout.Height(30)))
            {
                ShowActiveQuests();
            }
            if (GUILayout.Button(showingCompleted ? "Completed Quests (Current)" : "Completed Quests", buttonStyle, GUILayout.Height(30)))
            {
                ShowCompletedQuests();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Quest list
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));

            var quests = showingCompleted ? DailyQuestManager.GetCompletedQuests() : DailyQuestManager.GetActiveQuests();

            if (quests.Count == 0)
            {
                GUILayout.Label(showingCompleted ? "No completed quests today." : "No active quests available.", labelStyle);
            }
            else
            {
                foreach (var quest in quests)
                {
                    GUILayout.BeginVertical(boxStyle);

                    GUILayout.Label($"<b>{quest.Title}</b>", labelStyle);
                    GUILayout.Space(5);
                    GUILayout.Label(quest.Description, labelStyle);

                    GUILayout.Space(5);

                    if (showingCompleted)
                    {
                        GUILayout.Label("✓ Completed", labelStyle);
                    }
                    else
                    {
                        string status = quest.IsCompleted() ? "Ready to complete!" : "In progress...";
                        GUILayout.Label($"Status: {status}", labelStyle);
                    }

                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                }
            }

            GUILayout.EndScrollView();

            GUILayout.Space(10);

            // Info
            GUILayout.Label($"Time until refresh: {DailyQuestManager.TimeUntilNextRefresh:hh\\:mm\\:ss}", labelStyle);

            GUILayout.Space(10);

            // Close button
            if (GUILayout.Button("Close", buttonStyle, GUILayout.Height(40)))
            {
                Close();
            }

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        // ===== View Lifecycle =====

        public void Open()
        {
            open = true;

            Debug.Log("[DailyQuestGiverView] Opening view");

            // Subscribe to events
            DailyQuestManager.OnQuestsRefreshed += OnQuestsRefreshed;
            DailyQuestManager.OnQuestCompleted += OnQuestCompleted;

            RefreshQuestList();
        }

        public void Close()
        {
            open = false;

            Debug.Log("[DailyQuestGiverView] Closing view");

            // Unsubscribe from events
            DailyQuestManager.OnQuestsRefreshed -= OnQuestsRefreshed;
            DailyQuestManager.OnQuestCompleted -= OnQuestCompleted;

            // Clear selection
            selectedEntry = null;
        }

        // ===== Setup =====

        /// <summary>
        /// Setup the view with a daily quest giver
        /// </summary>
        public void Setup(DailyQuestGiver giver)
        {
            target = giver;
            Debug.Log("[DailyQuestGiverView] Setup complete");
        }

        // ===== Event Handlers =====

        private void OnQuestsRefreshed()
        {
            if (!open)
                return;

            Debug.Log("[DailyQuestGiverView] Quests refreshed, updating list");
            RefreshQuestList();
        }

        private void OnQuestCompleted(IDailyQuest quest)
        {
            if (!open)
                return;

            Debug.Log($"[DailyQuestGiverView] Quest completed: {quest.QuestId}");
            RefreshQuestList();
        }

        // ===== Quest List Management =====

        private void RefreshQuestList()
        {
            try
            {
                // Clear existing entries
                activeEntries.Clear();
                selectedEntry = null;

                List<IDailyQuest> questsToShow = GetQuestsToShow();

                Debug.Log($"[DailyQuestGiverView] Displaying {questsToShow.Count} quests (showingCompleted: {showingCompleted})");

                // Create entries for each quest
                foreach (var quest in questsToShow)
                {
                    var entry = new DailyQuestEntry(quest);
                    activeEntries.Add(entry);
                }

                // Auto-select first quest if available
                if (activeEntries.Count > 0)
                {
                    SetSelection(activeEntries[0]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestGiverView] Error refreshing quest list: {e}");
            }
        }

        private List<IDailyQuest> GetQuestsToShow()
        {
            if (showingCompleted)
            {
                return DailyQuestManager.GetCompletedQuests();
            }
            else
            {
                return DailyQuestManager.GetActiveQuests();
            }
        }

        // ===== Selection Management =====

        private void SetSelection(DailyQuestEntry entry)
        {
            selectedEntry = entry;
            Debug.Log($"[DailyQuestGiverView] Selected quest: {entry?.Quest?.QuestId}");
        }

        // ===== Tab Management =====

        /// <summary>
        /// Switch to showing active quests
        /// </summary>
        public void ShowActiveQuests()
        {
            if (!showingCompleted)
                return;

            showingCompleted = false;
            Debug.Log("[DailyQuestGiverView] Switched to active quests");
            RefreshQuestList();
        }

        /// <summary>
        /// Switch to showing completed quests
        /// </summary>
        public void ShowCompletedQuests()
        {
            if (showingCompleted)
                return;

            showingCompleted = true;
            Debug.Log("[DailyQuestGiverView] Switched to completed quests");
            RefreshQuestList();
        }

        // ===== Public Query Methods =====

        /// <summary>
        /// Get currently selected quest
        /// </summary>
        public IDailyQuest GetSelectedQuest()
        {
            return selectedEntry?.Quest;
        }

        /// <summary>
        /// Get all displayed quest entries
        /// </summary>
        public List<DailyQuestEntry> GetQuestEntries()
        {
            return new List<DailyQuestEntry>(activeEntries);
        }

        /// <summary>
        /// Select a quest by ID
        /// </summary>
        public bool SelectQuest(string questId)
        {
            var entry = activeEntries.FirstOrDefault(e => e.Quest.QuestId == questId);
            if (entry != null)
            {
                SetSelection(entry);
                return true;
            }

            return false;
        }

        // ===== Helper Classes =====

        /// <summary>
        /// Represents a quest entry in the list
        /// Simplified version without actual UI components
        /// </summary>
        public class DailyQuestEntry
        {
            public IDailyQuest Quest { get; private set; }
            public bool IsSelected { get; set; }

            public DailyQuestEntry(IDailyQuest quest)
            {
                Quest = quest;
                IsSelected = false;
            }

            public string GetTitle()
            {
                return Quest?.Title ?? "Unknown Quest";
            }

            public string GetDescription()
            {
                return Quest?.Description ?? "";
            }

            public bool IsCompleted()
            {
                return DailyQuestManager.IsQuestCompleted(Quest?.QuestId);
            }
        }
    }
}
