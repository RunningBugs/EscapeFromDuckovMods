using System;
using System.Linq;
using UnityEngine;
using Duckov.Buildings;

namespace DailyQuestMod.Integration
{
    /// <summary>
    /// Integrates daily quest system with Commemorative Trophy building
    /// Adds DailyQuestGiver component to trophy at runtime
    /// Sets up MultiInteraction if needed
    /// </summary>
    public static class TrophyIntegration
    {
        private static bool integrated = false;
        private static DailyQuestGiver dailyQuestGiver = null;

        /// <summary>
        /// Attempt to integrate with Commemorative Trophy
        /// Call this after buildings are loaded
        /// </summary>
        public static void IntegrateWithTrophy()
        {
            if (integrated)
            {
                Debug.Log("[TrophyIntegration] Already integrated");
                return;
            }

            try
            {
                Debug.Log("[TrophyIntegration] Searching for Commemorative Trophy...");

                // Find the trophy building
                var trophy = FindCommemoratativeTrophy();
                if (trophy == null)
                {
                    Debug.LogWarning("[TrophyIntegration] Commemorative Trophy not found, will retry later");
                    return;
                }

                Debug.Log($"[TrophyIntegration] Found trophy: {trophy.name}");

                // Add DailyQuestGiver component
                AddDailyQuestGiver(trophy);

                integrated = true;
                Debug.Log("[TrophyIntegration] Successfully integrated with Commemorative Trophy!");
            }
            catch (Exception e)
            {
                Debug.LogError($"[TrophyIntegration] Error during integration: {e}");
            }
        }

        /// <summary>
        /// Find the Commemorative Trophy GameObject
        /// Tries multiple methods to locate it
        /// </summary>
        private static GameObject FindCommemoratativeTrophy()
        {
            // Method 1: Find by Building component
            var buildings = GameObject.FindObjectsOfType<Building>();
            foreach (var building in buildings)
            {
                // Check building name or ID
                // Trophy building ID might be specific, check BuildingDataSettings
                if (building.name.Contains("Trophy") ||
                    building.name.Contains("Souvenir") ||
                    building.name.Contains("纪念"))
                {
                    Debug.Log($"[TrophyIntegration] Found potential trophy: {building.name}");
                    return building.gameObject;
                }
            }

            // Method 2: Search by name pattern
            var allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name.Contains("Trophy") ||
                    obj.name.Contains("Souvenir") ||
                    obj.name.Contains("纪念奖杯"))
                {
                    // Verify it has InteractableBase component
                    if (obj.GetComponent<InteractableBase>() != null)
                    {
                        Debug.Log($"[TrophyIntegration] Found trophy by name: {obj.name}");
                        return obj;
                    }
                }
            }

            // Method 3: Find by Building with specific building ID
            // TODO: Replace "0" with actual trophy building ID when known
            var specificBuilding = buildings.FirstOrDefault(b => b.ID == "0");
            if (specificBuilding != null)
            {
                return specificBuilding.gameObject;
            }

            return null;
        }

        /// <summary>
        /// Add DailyQuestGiver component to trophy
        /// Sets up MultiInteraction if needed
        /// </summary>
        private static void AddDailyQuestGiver(GameObject trophy)
        {
            // Check if already has DailyQuestGiver
            var existingGiver = trophy.GetComponent<DailyQuestGiver>();
            if (existingGiver != null)
            {
                Debug.Log("[TrophyIntegration] DailyQuestGiver already exists");
                dailyQuestGiver = existingGiver;
                return;
            }

            // Check for existing InteractableBase components
            var existingInteractables = trophy.GetComponents<InteractableBase>();
            var multiInteraction = trophy.GetComponent<MultiInteraction>();

            Debug.Log($"[TrophyIntegration] Found {existingInteractables.Length} existing InteractableBase components");
            Debug.Log($"[TrophyIntegration] MultiInteraction exists: {multiInteraction != null}");

            // Add DailyQuestGiver component
            dailyQuestGiver = trophy.AddComponent<DailyQuestGiver>();
            Debug.Log("[TrophyIntegration] Added DailyQuestGiver component");

            // If there are other interactables but no MultiInteraction, we need to set it up
            if (existingInteractables.Length > 0 && multiInteraction == null)
            {
                Debug.Log("[TrophyIntegration] Setting up MultiInteraction for multiple interactions");
                SetupMultiInteraction(trophy, existingInteractables);
            }
            else if (multiInteraction != null)
            {
                Debug.Log("[TrophyIntegration] MultiInteraction already exists, adding DailyQuestGiver to it");
                // MultiInteraction exists, need to add our giver to its list
                // This requires reflection or recreating the interaction
                // For now, log a warning
                Debug.LogWarning("[TrophyIntegration] Trophy already has MultiInteraction. May need manual integration.");
            }
        }

        /// <summary>
        /// Setup MultiInteraction for trophy with existing interactables + DailyQuestGiver
        /// </summary>
        private static void SetupMultiInteraction(GameObject trophy, InteractableBase[] existingInteractables)
        {
            try
            {
                // Add MultiInteraction component
                var multiInteraction = trophy.AddComponent<MultiInteraction>();

                // Get the private interactables list field using reflection
                var interactablesField = typeof(MultiInteraction).GetField(
                    "interactables",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                );

                if (interactablesField == null)
                {
                    Debug.LogError("[TrophyIntegration] Could not find 'interactables' field in MultiInteraction");
                    return;
                }

                // Create list of all interactables including our new one
                var interactablesList = new System.Collections.Generic.List<InteractableBase>(existingInteractables);
                interactablesList.Add(dailyQuestGiver);

                // Set the field
                interactablesField.SetValue(multiInteraction, interactablesList);

                Debug.Log($"[TrophyIntegration] Setup MultiInteraction with {interactablesList.Count} interactables");
            }
            catch (Exception e)
            {
                Debug.LogError($"[TrophyIntegration] Error setting up MultiInteraction: {e}");
            }
        }

        /// <summary>
        /// Check if integration is complete
        /// </summary>
        public static bool IsIntegrated()
        {
            return integrated && dailyQuestGiver != null;
        }

        /// <summary>
        /// Get the DailyQuestGiver instance
        /// </summary>
        public static DailyQuestGiver GetDailyQuestGiver()
        {
            return dailyQuestGiver;
        }

        /// <summary>
        /// Reset integration state (for testing)
        /// </summary>
        public static void ResetIntegration()
        {
            integrated = false;
            dailyQuestGiver = null;
            Debug.Log("[TrophyIntegration] Integration reset");
        }
    }
}
