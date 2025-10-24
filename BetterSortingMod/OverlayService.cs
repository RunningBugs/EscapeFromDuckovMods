using System;
using System.Collections.Generic;
using Duckov.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BetterSortingMod
{
    /// <summary>
    /// Centralized global overlay manager for BetterSortingMod.
    /// - Hosts a single, shared overlay root under GameplayUIManager so every floating UI renders above game UI consistently.
    /// - Provides helpers to create per-menu overlay layers with a transparent, raycastable background to dismiss on click.
    /// - Provides utilities for full-stretch rects and screen->overlay coordinate conversion.
    /// </summary>
    internal static class OverlayService
    {
        private const string GlobalRootName = "BetterSortingMod_GlobalOverlayRoot";

        private static RectTransform s_root;
        private static readonly List<RectTransform> s_layers = new List<RectTransform>(8);

        /// <summary>
        /// Ensure and return the global overlay root RectTransform, parented to GameplayUIManager.
        /// </summary>
        /// <remarks>
        /// The root does not add any Canvas; it relies on the existing UI graph under GameplayUIManager.
        /// </remarks>
        public static RectTransform EnsureRoot()
        {
            if (s_root != null)
            {
                return s_root;
            }

            GameplayUIManager uiManager = GameplayUIManager.Instance;
            if (uiManager == null)
            {
                // UI not ready; caller should retry later
                return null;
            }

            GameObject rootObj = new GameObject(GlobalRootName, typeof(RectTransform));
            s_root = rootObj.GetComponent<RectTransform>();
            s_root.SetParent(uiManager.transform, worldPositionStays: false);
            SetFullStretch(s_root);
            s_root.SetAsLastSibling();
            return s_root;
        }

        /// <summary>
        /// Create a new overlay layer under the global root with a transparent, raycastable background.
        /// </summary>
        /// <param name="name">Layer GameObject name</param>
        /// <param name="onBackgroundClick">Callback when user clicks empty background (typically to dismiss menu)</param>
        /// <param name="raycastBackground">If true, background Image will block raycasts</param>
        public static OverlayLayer CreateLayer(string name, Action onBackgroundClick, bool raycastBackground = true)
        {
            RectTransform root = EnsureRoot();
            if (root == null)
            {
                // Return an empty layer; caller can detect by null Root
                return default;
            }

            GameObject layerObj = new GameObject(string.IsNullOrEmpty(name) ? "BSM_OverlayLayer" : name,
                                                 typeof(RectTransform), typeof(Image), typeof(SortMenuOverlay));
            RectTransform layer = layerObj.GetComponent<RectTransform>();
            layer.SetParent(root, worldPositionStays: false);
            SetFullStretch(layer);

            Image bg = layerObj.GetComponent<Image>();
            bg.color = Color.clear;
            bg.raycastTarget = raycastBackground;

            SortMenuOverlay overlay = layerObj.GetComponent<SortMenuOverlay>();
            overlay.Initialize(onBackgroundClick);

            s_layers.Add(layer);
            BringToFront(layer);

            return new OverlayLayer
            {
                Root = layer,
                Background = bg,
                DismissOverlay = overlay
            };
        }

        /// <summary>
        /// Destroys an overlay layer created by CreateLayer.
        /// </summary>
        public static void DestroyLayer(RectTransform layer)
        {
            if (layer == null)
            {
                return;
            }
            int idx = s_layers.IndexOf(layer);
            if (idx >= 0)
            {
                s_layers.RemoveAt(idx);
            }
            UnityEngine.Object.Destroy(layer.gameObject);
        }

        /// <summary>
        /// Hide (deactivate) all overlay layers under the global root.
        /// </summary>
        public static void HideAllLayers()
        {
            RectTransform root = s_root;
            if (root == null)
            {
                return;
            }
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child != null && child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Show (activate) all overlay layers under the global root.
        /// </summary>
        public static void ShowAllLayers()
        {
            RectTransform root = s_root;
            if (root == null)
            {
                return;
            }
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child != null && !child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Bring a given layer to the front (last sibling) so it renders above other overlay layers.
        /// </summary>
        public static void BringToFront(RectTransform layer)
        {
            if (layer == null)
            {
                return;
            }
            layer.SetAsLastSibling();
        }

        /// <summary>
        /// Helper: Set a RectTransform to full stretch within its parent.
        /// </summary>
        public static void SetFullStretch(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        /// <summary>
        /// Convert a screen-space point into a local anchored point of an overlay rect for placement.
        /// </summary>
        /// <param name="overlayRect">Target overlay root or child rect</param>
        /// <param name="screenPoint">Screen-space coordinate</param>
        /// <param name="sourceCanvas">
        /// The canvas where the screenPoint came from (the display’s parent canvas).
        /// Its renderMode and camera are used for the conversion.
        /// </param>
        /// <param name="localAnchored">Result local point in overlay’s rect space</param>
        public static bool TryScreenToOverlayLocal(RectTransform overlayRect, Vector2 screenPoint, Canvas sourceCanvas, out Vector2 localAnchored)
        {
            localAnchored = default;
            if (overlayRect == null || sourceCanvas == null)
            {
                return false;
            }

            Camera cam = sourceCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : sourceCanvas.worldCamera;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRect, screenPoint, cam, out localAnchored);
        }

        /// <summary>
        /// Convenience: Create a menu container (background + contentRect) inside a new overlay layer.
        /// </summary>
        /// <param name="layerName">Overlay layer name</param>
        /// <param name="onBackgroundClick">Click-to-dismiss callback</param>
        /// <param name="menuBackgroundColor">Menu background color</param>
        /// <returns>Tuple of (layer, contentRect) where contentRect is the menu parent</returns>
        public static (OverlayLayer layer, RectTransform contentRect) CreateMenuLayer(string layerName, Action onBackgroundClick, Color menuBackgroundColor)
        {
            OverlayLayer layer = CreateLayer(layerName, onBackgroundClick, raycastBackground: true);
            if (layer.Root == null)
            {
                return (default, null);
            }

            // Menu content container (positioned by caller)
            GameObject menuObj = new GameObject("BSM_Menu", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            RectTransform menu = menuObj.GetComponent<RectTransform>();
            menu.SetParent(layer.Root, worldPositionStays: false);
            // Default menu anchor: top-left; callers can change as needed
            menu.anchorMin = new Vector2(0f, 1f);
            menu.anchorMax = new Vector2(0f, 1f);
            menu.pivot = new Vector2(0f, 1f);
            menu.anchoredPosition = Vector2.zero;

            Image bg = menuObj.GetComponent<Image>();
            bg.color = menuBackgroundColor;
            bg.raycastTarget = true;

            VerticalLayoutGroup vlg = menuObj.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 4f;
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter fitter = menuObj.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return (layer, menu);
        }

        /// <summary>
        /// Data container for an overlay layer created by OverlayService.
        /// </summary>
        public struct OverlayLayer
        {
            public RectTransform Root;
            public Image Background;
            public SortMenuOverlay DismissOverlay;
        }
    }
}
