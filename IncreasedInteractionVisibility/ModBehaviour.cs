using UnityEngine;
using HarmonyLib;

namespace IncreasedInteractionVisibility
{
    /// <summary>
    /// Increases the visibility distance of interaction markers/dots in the world.
    /// Makes it easier to see interactable objects from further away.
    /// Uses Harmony to patch InteractableBase.ActiveMarker() to modify markers as they spawn.
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const float MARKER_VISIBILITY_FAR = 1000f;  // Distance where marker starts to appear
        private const float MARKER_VISIBILITY_NEAR = 900f;  // Distance where marker is fully visible
        private const string HARMONY_ID = "com.duckov.increasedinteractionvisibility";

        private Harmony harmony;

        private void OnEnable()
        {
            if (harmony != null)
            {
                return;
            }

            // Create and apply Harmony patches
            harmony = new Harmony(HARMONY_ID);
            harmony.PatchAll(typeof(ModBehaviour).Assembly);

            // Process any existing markers in the scene
            ProcessAllExistingMarkers();
        }

        private void OnDisable()
        {
            // Unpatch when mod is deactivated
            if (harmony == null)
            {
                return;
            }

            harmony.UnpatchAll(harmony.Id);
            harmony = null;
        }

        private void ProcessAllExistingMarkers()
        {
            // Find all existing markers in the scene and process them
            InteractMarker[] markers = UnityEngine.Object.FindObjectsOfType<InteractMarker>();

            foreach (InteractMarker marker in markers)
            {
                if (marker != null && marker.gameObject != null)
                {
                    InteractableBase_ActiveMarker_Patch.ProcessMarker(marker);
                }
            }
        }

        /// <summary>
        /// Harmony Postfix patch for InteractablePickup.OnInit()
        /// This ensures pickups have an interaction marker, creating one if missing.
        /// </summary>
        [HarmonyPatch(typeof(InteractablePickup), "OnInit")]
        public static class InteractablePickup_OnInit_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(InteractablePickup __instance)
            {
                try
                {
                    if (__instance == null)
                    {
                        return;
                    }

                    // Check if marker already exists (using publicized field)
                    if (__instance.markerObject != null && __instance.markerObject.gameObject != null)
                    {
                        return;
                    }

                    // No marker exists, force enable it
                    __instance.MarkerActive = true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[IncreasedInteractionVisibility] Failed to ensure pickup marker: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Harmony Postfix patch for InteractableBase.ActiveMarker()
        /// This is called right after a marker is instantiated, allowing us to modify it immediately.
        /// </summary>
        [HarmonyPatch(typeof(InteractableBase), "ActiveMarker")]
        public static class InteractableBase_ActiveMarker_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(InteractableBase __instance)
            {
                try
                {
                    // Access the publicized markerObject field
                    InteractMarker marker = __instance.markerObject;

                    if (marker == null || marker.gameObject == null)
                    {
                        return;
                    }

                    // Modify the marker's materials
                    ProcessMarker(marker);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[IncreasedInteractionVisibility] Failed to process marker in postfix: {ex.Message}");
                }
            }

            public static void ProcessMarker(InteractMarker marker)
            {
                try
                {
                    // Get all renderers in the marker hierarchy
                    Renderer[] renderers = marker.GetComponentsInChildren<Renderer>(true);

                    foreach (Renderer renderer in renderers)
                    {
                        if (renderer == null) continue;

                        // Get material instances (this creates instances if needed)
                        Material[] materials = renderer.materials;
                        bool modified = false;

                        for (int i = 0; i < materials.Length; i++)
                        {
                            Material mat = materials[i];
                            if (mat == null) continue;

                            // Check if material has the distance fade properties
                            if (mat.HasProperty("_Near"))
                            {
                                mat.SetFloat("_Near", MARKER_VISIBILITY_NEAR);
                                modified = true;
                            }
                            if (mat.HasProperty("_Far"))
                            {
                                mat.SetFloat("_Far", MARKER_VISIBILITY_FAR);
                                modified = true;
                            }
                        }

                        // Update renderer with modified materials if we changed anything
                        if (modified)
                        {
                            renderer.materials = materials;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[IncreasedInteractionVisibility] Failed to process marker materials: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
}
