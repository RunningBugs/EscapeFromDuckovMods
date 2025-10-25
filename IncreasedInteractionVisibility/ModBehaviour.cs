using UnityEngine;
using System.Collections.Generic;

namespace IncreasedInteractionVisibility
{
    /// <summary>
    /// Increases the visibility distance of interaction markers/dots in the world.
    /// Makes it easier to see interactable objects from further away.
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const float UPDATE_INTERVAL = 0.5f;
        private const float MARKER_VISIBILITY_DISTANCE = 1000f;

        private float updateTimer = 0f;
        private HashSet<InteractMarker> processedMarkers = new HashSet<InteractMarker>();

        void Awake()
        {
        }

        void Update()
        {
            updateTimer += Time.deltaTime;

            if (updateTimer >= UPDATE_INTERVAL)
            {
                updateTimer = 0f;
                UpdateAllMarkers();
            }
        }

        private void UpdateAllMarkers()
        {
            InteractMarker[] markers = FindObjectsOfType<InteractMarker>();

            foreach (InteractMarker marker in markers)
            {
                if (marker != null && !processedMarkers.Contains(marker))
                {
                    ProcessMarker(marker);
                }
            }
        }

        private void ProcessMarker(InteractMarker marker)
        {
            try
            {
                Renderer[] renderers = marker.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    if (renderer != null)
                    {
                        foreach (Material mat in renderer.materials)
                        {
                            if (mat != null)
                            {
                                // Set _Near and _Far properties to increase visibility distance
                                if (mat.HasProperty("_Near"))
                                {
                                    mat.SetFloat("_Near", 0.1f);
                                }
                                if (mat.HasProperty("_Far"))
                                {
                                    mat.SetFloat("_Far", MARKER_VISIBILITY_DISTANCE);
                                }
                            }
                        }
                    }
                }

                processedMarkers.Add(marker);
            }
            catch (System.Exception)
            {
                // Silently ignore errors
            }
        }

        void OnDestroy()
        {
        }

        void OnEnable()
        {
            processedMarkers.Clear();
        }
    }
}
