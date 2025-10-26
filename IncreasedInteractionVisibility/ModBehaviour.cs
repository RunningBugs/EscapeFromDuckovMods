using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace IncreasedInteractionVisibility
{
    /// <summary>
    /// Increases the visibility distance of interaction markers/dots in the world.
    /// Makes it easier to see interactable objects from further away.
    /// Optimized version using event-based detection with periodic checks for dynamic spawns.
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const float MARKER_VISIBILITY_DISTANCE = 1000f;
        private const float NEW_MARKER_CHECK_INTERVAL = 0.5f; // Check for new markers every 0.5 seconds

        private Dictionary<InteractMarker, Material[]> processedMarkers = new Dictionary<InteractMarker, Material[]>();
        private Coroutine checkCoroutine;

        protected override void OnAfterSetup()
        {
            base.OnAfterSetup();

            // Hook into level initialization events (static event)
            LevelManager.OnAfterLevelInitialized += OnLevelInitialized;

            // Hook into scene loading events (static event)
            SceneLoader.onAfterSceneInitialize += OnSceneInitialized;

            // Process any existing markers in current scene
            ProcessAllExistingMarkers();

            // Start coroutine to check for newly spawned markers
            if (checkCoroutine == null)
            {
                checkCoroutine = StartCoroutine(CheckForNewMarkersCoroutine());
            }
        }

        protected override void OnBeforeDeactivate()
        {
            base.OnBeforeDeactivate();

            // Unsubscribe from events
            LevelManager.OnAfterLevelInitialized -= OnLevelInitialized;
            SceneLoader.onAfterSceneInitialize -= OnSceneInitialized;

            // Stop coroutine
            if (checkCoroutine != null)
            {
                StopCoroutine(checkCoroutine);
                checkCoroutine = null;
            }

            // Restore original materials if needed
            RestoreAllMarkers();
            processedMarkers.Clear();
        }

        private void OnLevelInitialized()
        {
            ProcessAllExistingMarkers();
        }

        private void OnSceneInitialized(SceneLoadingContext context)
        {
            ProcessAllExistingMarkers();
        }

        private IEnumerator CheckForNewMarkersCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(NEW_MARKER_CHECK_INTERVAL);

                // Clean up destroyed markers
                CleanupDestroyedMarkers();

                // Check for new markers only
                CheckForNewMarkers();
            }
        }

        private void CheckForNewMarkers()
        {
            // Find all markers in scene
            InteractMarker[] markers = FindObjectsOfType<InteractMarker>();

            // Process only markers we haven't seen before
            foreach (InteractMarker marker in markers)
            {
                if (marker != null && !processedMarkers.ContainsKey(marker))
                {
                    ProcessMarker(marker);
                }
            }
        }

        private void ProcessAllExistingMarkers()
        {
            // Clean up destroyed markers first
            CleanupDestroyedMarkers();

            // Find all markers in scene (only done once per scene load)
            InteractMarker[] markers = FindObjectsOfType<InteractMarker>();

            foreach (InteractMarker marker in markers)
            {
                if (marker != null && !processedMarkers.ContainsKey(marker))
                {
                    ProcessMarker(marker);
                }
            }
        }

        private void CleanupDestroyedMarkers()
        {
            // Remove destroyed markers from dictionary to prevent memory leak
            List<InteractMarker> toRemove = new List<InteractMarker>();

            foreach (var kvp in processedMarkers)
            {
                if (kvp.Key == null)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var marker in toRemove)
            {
                processedMarkers.Remove(marker);
            }
        }

        private void ProcessMarker(InteractMarker marker)
        {
            try
            {
                // Use includeInactive=false to reduce search scope
                Renderer[] renderers = marker.GetComponentsInChildren<Renderer>(false);
                List<Material> modifiedMaterials = new List<Material>();

                foreach (Renderer renderer in renderers)
                {
                    if (renderer == null) continue;

                    // Use sharedMaterials to avoid creating garbage (read-only access)
                    // If we need to modify, we'll create instances only once
                    Material[] sharedMats = renderer.sharedMaterials;

                    for (int i = 0; i < sharedMats.Length; i++)
                    {
                        Material mat = sharedMats[i];
                        if (mat == null) continue;

                        // Check if material has the properties we want to modify
                        if (mat.HasProperty("_Near") || mat.HasProperty("_Far"))
                        {
                            // Create material instance only if needed (once per marker)
                            if (!modifiedMaterials.Contains(mat))
                            {
                                // Create instance for this renderer
                                Material[] materialInstances = renderer.materials;
                                Material instanceMat = materialInstances[i];

                                if (instanceMat.HasProperty("_Near"))
                                {
                                    instanceMat.SetFloat("_Near", 0.1f);
                                }
                                if (instanceMat.HasProperty("_Far"))
                                {
                                    instanceMat.SetFloat("_Far", MARKER_VISIBILITY_DISTANCE);
                                }

                                // Update renderer with modified materials
                                renderer.materials = materialInstances;
                                modifiedMaterials.Add(instanceMat);
                            }
                        }
                    }
                }

                // Cache the modified materials for potential restoration
                if (modifiedMaterials.Count > 0)
                {
                    processedMarkers[marker] = modifiedMaterials.ToArray();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[IncreasedInteractionVisibility] Failed to process marker: {ex.Message}");
            }
        }

        private void RestoreAllMarkers()
        {
            // Optional: restore original visibility if needed
            // Currently just clears references
            processedMarkers.Clear();
        }

        void OnDestroy()
        {
            OnBeforeDeactivate();
        }
    }
}
