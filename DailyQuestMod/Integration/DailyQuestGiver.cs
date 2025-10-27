using System;
using UnityEngine;
using DailyQuestMod.Framework;
using DailyQuestMod.Integration.UI;

namespace DailyQuestMod.Integration
{
    /// <summary>
    /// InteractableBase component for NPCs to provide daily quests
    /// Use with MultiInteraction for multiple interaction options
    /// Opens DailyQuestGiverView UI when interacted
    /// </summary>
    public class DailyQuestGiver : InteractableBase
    {
        protected override void Awake()
        {
            try
            {
                base.Awake();
            }
            catch (System.Exception e)
            {
                // Handle Harmony patch conflicts gracefully
                Debug.LogWarning($"[DailyQuestGiver] Base.Awake() threw exception (likely Harmony patch conflict): {e.Message}");
            }

            try
            {
                // Set interaction name
                // TODO: Add localization support
                InteractName = "Daily Quests";

                // Subscribe to daily quest events for inspection indicator
                DailyQuestManager.OnQuestsRefreshed += OnQuestsRefreshed;
                DailyQuestManager.OnQuestCompleted += OnQuestCompleted;

                Debug.Log("[DailyQuestGiver] Awake completed successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DailyQuestGiver] Error in Awake: {e}");
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from events
            DailyQuestManager.OnQuestsRefreshed -= OnQuestsRefreshed;
            DailyQuestManager.OnQuestCompleted -= OnQuestCompleted;
        }

        private void OnQuestsRefreshed()
        {
            // Quest list changed, might affect inspection indicator
            // TODO: Implement inspection indicator logic
        }

        private void OnQuestCompleted(IDailyQuest quest)
        {
            // Quest completed, might affect inspection indicator
            // TODO: Implement inspection indicator logic
        }

        /// <summary>
        /// Called when player starts interacting
        /// Opens the daily quest UI
        /// </summary>
        protected override void OnInteractStart(CharacterMainControl interactCharacter)
        {
            try
            {
                base.OnInteractStart(interactCharacter);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DailyQuestGiver] Base.OnInteractStart() threw exception: {e.Message}");
            }

            var view = DailyQuestGiverView.Instance;
            if (view == null)
            {
                Debug.LogError("[DailyQuestGiver] DailyQuestGiverView.Instance is null, cannot open UI");
                return;
            }

            try
            {
                view.Setup(this);
                view.Open();
                Debug.Log("[DailyQuestGiver] Opened daily quest UI");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyQuestGiver] Error opening UI: {e}");
            }
        }

        /// <summary>
        /// Called when interaction stops
        /// Closes the daily quest UI
        /// </summary>
        protected override void OnInteractStop()
        {
            try
            {
                base.OnInteractStop();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DailyQuestGiver] Base.OnInteractStop() threw exception: {e.Message}");
            }

            var view = DailyQuestGiverView.Instance;
            if (view != null && view.open)
            {
                try
                {
                    view.Close();
                    Debug.Log("[DailyQuestGiver] Closed daily quest UI");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DailyQuestGiver] Error closing UI: {e}");
                }
            }
        }

        /// <summary>
        /// Called during Update while interacting
        /// Stops interaction if UI is closed
        /// </summary>
        protected override void OnUpdate(CharacterMainControl _interactCharacter, float deltaTime)
        {
            try
            {
                base.OnUpdate(_interactCharacter, deltaTime);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DailyQuestGiver] Base.OnUpdate() threw exception: {e.Message}");
            }

            var view = DailyQuestGiverView.Instance;
            if (view == null || !view.open)
            {
                StopInteract();
            }
        }

        /// <summary>
        /// Check if any daily quests are available
        /// Used for inspection indicator logic
        /// </summary>
        public bool HasAvailableQuests()
        {
            var activeQuests = DailyQuestManager.GetActiveQuests();
            return activeQuests != null && activeQuests.Count > 0;
        }

        /// <summary>
        /// Check if any daily quests are completed but not yet viewed
        /// Used for inspection indicator logic
        /// </summary>
        public bool HasCompletedQuests()
        {
            var completedQuests = DailyQuestManager.GetCompletedQuests();
            return completedQuests != null && completedQuests.Count > 0;
        }
    }
}
