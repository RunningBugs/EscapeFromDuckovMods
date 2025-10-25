using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Duckov.MiniMaps.UI;
using System.IO;
using SodaCraft.Localizations;

namespace BossLiveMapMod
{
    /// <summary>
    /// Runtime UI component attached to the MiniMapView to add live controls.
    /// </summary>
    public sealed class MapViewUI : MonoBehaviour
    {
        private MiniMapView _view;
        private RectTransform _panel;
        private RectTransform _titleRT;
        private Toggle _toggleAll;
        private Toggle _toggleLive;
        private Toggle _toggleNames;
        private Toggle _toggleNearby;
        private Slider _alphaSlider;
        private TextMeshProUGUI _alphaPct;
        private float _scale = 1f;

        /// <summary>
        /// Ensure that a MapViewUI exists on the current MiniMapView instance.
        /// </summary>
        public static MapViewUI Ensure()
        {
            var view = MiniMapView.Instance;
            if (view == null)
                return null;
            var existing = view.GetComponent<MapViewUI>();
            if (existing != null)
                return existing;
            return Create(view);
        }

        private static MapViewUI Create(MiniMapView view)
        {
            var go = new GameObject("BLM_MapViewUI", typeof(RectTransform));
            go.transform.SetParent(view.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            var ui = go.AddComponent<MapViewUI>();
            ui.Initialize(view);
            return ui;
        }

        private void Initialize(MiniMapView view)
        {
            _view = view;
            InitializeLocalization();
            Build();
            var viewActive = (_view != null && _view.gameObject.activeInHierarchy);
            Debug.Log("[BossLiveMapMod] MapViewUI initialized. ViewActive=" + viewActive);
            // Keep this component active; toggle only the panel visibility
            if (_panel != null) _panel.gameObject.SetActive(viewActive);
        }

        private void InitializeLocalization()
        {
            try
            {
                var assemblyLocation = typeof(MapViewUI).Assembly.Location;
                var modFolder = Path.GetDirectoryName(assemblyLocation);
                ModLocalization.Initialize(modFolder);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BossLiveMapMod] Failed to initialize localization: {ex}");
            }
        }

        private void Update()
        {
            // Keep visibility in sync with map view (toggle only panel so this component stays active)
            if (_panel != null)
            {
                bool shouldBeActive = _view != null && _view.gameObject.activeInHierarchy;
                if (_panel.gameObject.activeSelf != shouldBeActive)
                {
                    _panel.gameObject.SetActive(shouldBeActive);
                    Debug.Log("[BossLiveMapMod] Panel visibility -> " + shouldBeActive);
                }
            }
        }

        private void OnDestroy()
        {
            if (_toggleAll != null) _toggleAll.onValueChanged.RemoveAllListeners();
            if (_toggleLive != null) _toggleLive.onValueChanged.RemoveAllListeners();
            if (_toggleNames != null) _toggleNames.onValueChanged.RemoveAllListeners();
            if (_toggleNearby != null) _toggleNearby.onValueChanged.RemoveAllListeners();
            if (_alphaSlider != null) _alphaSlider.onValueChanged.RemoveAllListeners();

        }

        private void Build()
        {
            // Root panel setup
            var panelGO = new GameObject("BLM_ControlsPanel", typeof(RectTransform));

            // Find the map title for reference (optional - helps with positioning)
            _titleRT = null;
            try
            {
                if (_view != null)
                {
                    var all = _view.GetComponentsInChildren<Transform>(true);
                    foreach (var t in all)
                    {
                        var n = t.name;
                        if (string.Equals(n, "mapNameText", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(n, "mapName", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(n, "MapNameText", StringComparison.OrdinalIgnoreCase))
                        {
                            _titleRT = t.GetComponent<TextMeshProUGUI>()?.rectTransform;
                            break;
                        }
                    }

                    // If not found, fall back to any child that has a TextMeshProUGUI component
                    if (_titleRT == null)
                    {
                        foreach (var t in all)
                        {
                            var tmp = t.GetComponent<TextMeshProUGUI>();
                            if (tmp != null)
                            {
                                _titleRT = tmp.rectTransform;
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                _titleRT = null;
            }

            // Parent directly to the map view to keep controls attached to it
            // Find the Canvas child of the map view (if exists) for proper UI layering
            Transform parentTransform = _view.transform;
            Canvas mapCanvas = _view.GetComponentInChildren<Canvas>();
            if (mapCanvas != null)
            {
                parentTransform = mapCanvas.transform;
                Debug.Log("[BossLiveMapMod] Found Canvas child '" + mapCanvas.name + "' in map view, using as parent");
            }
            else
            {
                Debug.Log("[BossLiveMapMod] No Canvas found in map view, parenting directly to MiniMapView");
            }

            panelGO.transform.SetParent(parentTransform, false);
            _scale = Mathf.Clamp(ModConfig.UiScale, 0.5f, 2f);
            _panel = panelGO.GetComponent<RectTransform>();
            _panel.localScale = new Vector3(_scale, _scale, 1f);

            // Anchor to top-left corner of the map view with padding
            // This keeps the controls attached to the map view itself
            _panel.anchorMin = new Vector2(0f, 1f);
            _panel.anchorMax = new Vector2(0f, 1f);
            _panel.pivot = new Vector2(0f, 1f);
            _panel.anchoredPosition = new Vector2(10f, -10f); // 10px padding from top-left
            _panel.sizeDelta = new Vector2(400f, 120f);

            // Ensure panel renders above other siblings
            panelGO.transform.SetAsLastSibling();

            // Add CanvasGroup to help visibility and input handling
            var cg = panelGO.AddComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.alpha = 1f;

            Debug.Log("[BossLiveMapMod] Created panel '" + panelGO.name + "' parent='" + (parentTransform != null ? parentTransform.name : "null") + "' anchored=" + _panel.anchoredPosition);

            // No background - transparent

            // Vertical layout - checkboxes on top, slider below
            var v = panelGO.AddComponent<VerticalLayoutGroup>();
            v.spacing = 8f;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = false;
            v.childAlignment = TextAnchor.UpperLeft;
            v.padding = new RectOffset(12, 12, 8, 8);

            var fitter = panelGO.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Create horizontal row for checkboxes
            var checkboxRow = new GameObject("CheckboxRow", typeof(RectTransform));
            checkboxRow.transform.SetParent(panelGO.transform, false);
            var rowRT = checkboxRow.GetComponent<RectTransform>();
            rowRT.sizeDelta = new Vector2(500f, 48f);

            var rowLayout = checkboxRow.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8f;
            rowLayout.childForceExpandHeight = false;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;

            var rowLayoutElement = checkboxRow.AddComponent<LayoutElement>();
            rowLayoutElement.preferredWidth = 500f;
            rowLayoutElement.preferredHeight = 48f;

            // Create toggles in the horizontal row
            _toggleAll = CreateToggle(checkboxRow.transform, GetLocalizedText("mobs", "Mobs"), ModConfig.ShowAllEnemies, v => ModConfig.SetShowAllEnemies(v));
            _toggleNearby = CreateToggle(checkboxRow.transform, GetLocalizedText("nearby", "Nearby"), ModConfig.ShowNearbyOnly, v => ModConfig.SetShowNearbyOnly(v));
            _toggleLive = CreateToggle(checkboxRow.transform, GetLocalizedText("live", "Live"), ModConfig.ShowLivePositions, v => ModConfig.SetShowLivePositions(v));
            _toggleNames = CreateToggle(checkboxRow.transform, GetLocalizedText("names", "Names"), ModConfig.ShowNames, v => ModConfig.SetShowNames(v));

            // Create alpha slider container
            var sliderContainer = new GameObject("AlphaContainer", typeof(RectTransform));
            sliderContainer.transform.SetParent(panelGO.transform, false);
            var srt = sliderContainer.GetComponent<RectTransform>();
            srt.sizeDelta = new Vector2(380f, 40f);
            var sliderLayout = sliderContainer.AddComponent<LayoutElement>();
            sliderLayout.preferredWidth = 380f;
            sliderLayout.preferredHeight = 40f;

            // Label for slider
            var labelGO = new GameObject("AlphaLabel", typeof(RectTransform));
            labelGO.transform.SetParent(sliderContainer.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = GetLocalizedText("alpha", "Alpha");
            label.fontSize = 14;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget = false;
            var lrt = labelGO.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0f, 0.5f);
            lrt.anchorMax = new Vector2(0f, 0.5f);
            lrt.pivot = new Vector2(0f, 0.5f);
            lrt.anchoredPosition = new Vector2(8f, 0f);
            lrt.sizeDelta = new Vector2(50f, 28f);

            // Slider
            var sliderGO = new GameObject("AlphaSlider", typeof(RectTransform));
            sliderGO.transform.SetParent(sliderContainer.transform, false);
            var sliderRT = sliderGO.GetComponent<RectTransform>();
            sliderRT.anchorMin = new Vector2(0f, 0.5f);
            sliderRT.anchorMax = new Vector2(1f, 0.5f);
            sliderRT.pivot = new Vector2(0f, 0.5f);
            sliderRT.anchoredPosition = new Vector2(65f, 0f);
            sliderRT.sizeDelta = new Vector2(-125f, 20f);

            _alphaSlider = sliderGO.AddComponent<Slider>();
            _alphaSlider.minValue = 0;
            _alphaSlider.maxValue = 10;
            _alphaSlider.wholeNumbers = true;
            _alphaSlider.value = Mathf.RoundToInt(ModConfig.Transparency * 10f);
            _alphaSlider.onValueChanged.AddListener(OnAlphaChanged);

            // Slider background track
            var sliderBgGO = new GameObject("Background", typeof(RectTransform));
            sliderBgGO.transform.SetParent(sliderGO.transform, false);
            var sliderBg = sliderBgGO.AddComponent<Image>();
            sliderBg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            var sbRt = sliderBgGO.GetComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(0f, 0.5f);
            sbRt.anchorMax = new Vector2(1f, 0.5f);
            sbRt.pivot = new Vector2(0.5f, 0.5f);
            sbRt.sizeDelta = new Vector2(0f, 4f);
            _alphaSlider.targetGraphic = sliderBg;

            // Fill area
            var fillAreaGO = new GameObject("FillArea", typeof(RectTransform));
            fillAreaGO.transform.SetParent(sliderGO.transform, false);
            var fillAreaRT = fillAreaGO.GetComponent<RectTransform>();
            fillAreaRT.anchorMin = new Vector2(0f, 0.5f);
            fillAreaRT.anchorMax = new Vector2(1f, 0.5f);
            fillAreaRT.pivot = new Vector2(0.5f, 0.5f);
            fillAreaRT.sizeDelta = new Vector2(0f, 4f);

            var fillGO = new GameObject("Fill", typeof(RectTransform));
            fillGO.transform.SetParent(fillAreaGO.transform, false);
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.8f, 1f, 1f);
            var fRt = fillGO.GetComponent<RectTransform>();
            fRt.anchorMin = new Vector2(0f, 0f);
            fRt.anchorMax = new Vector2(0f, 1f);
            fRt.pivot = new Vector2(0.5f, 0.5f);
            fRt.sizeDelta = new Vector2(0f, 0f);

            _alphaSlider.fillRect = fRt;

            // Slider handle
            var handleAreaGO = new GameObject("HandleArea", typeof(RectTransform));
            handleAreaGO.transform.SetParent(sliderGO.transform, false);
            var handleAreaRT = handleAreaGO.GetComponent<RectTransform>();
            handleAreaRT.anchorMin = new Vector2(0f, 0f);
            handleAreaRT.anchorMax = new Vector2(1f, 1f);
            handleAreaRT.offsetMin = Vector2.zero;
            handleAreaRT.offsetMax = Vector2.zero;

            var handleGO = new GameObject("Handle", typeof(RectTransform));
            handleGO.transform.SetParent(handleAreaGO.transform, false);
            var handleImg = handleGO.AddComponent<Image>();
            handleImg.color = Color.white;
            var handleRT = handleGO.GetComponent<RectTransform>();
            handleRT.anchorMin = new Vector2(0f, 0.5f);
            handleRT.anchorMax = new Vector2(0f, 0.5f);
            handleRT.pivot = new Vector2(0.5f, 0.5f);
            handleRT.sizeDelta = new Vector2(12f, 12f);

            _alphaSlider.handleRect = handleRT;

            // percentage text
            var pctGO = new GameObject("AlphaPct", typeof(RectTransform));
            pctGO.transform.SetParent(sliderContainer.transform, false);
            _alphaPct = pctGO.AddComponent<TextMeshProUGUI>();
            _alphaPct.text = $"{Mathf.RoundToInt(ModConfig.Transparency * 100f)}%";
            _alphaPct.fontSize = 14;
            _alphaPct.color = Color.white;
            _alphaPct.alignment = TextAlignmentOptions.MidlineRight;
            _alphaPct.raycastTarget = false;
            var pctRt = pctGO.GetComponent<RectTransform>();
            pctRt.anchorMin = new Vector2(1f, 0.5f);
            pctRt.anchorMax = new Vector2(1f, 0.5f);
            pctRt.pivot = new Vector2(1f, 0.5f);
            pctRt.anchoredPosition = new Vector2(-6f, 0f);
            pctRt.sizeDelta = new Vector2(48f, 28f);


        }

        private static Transform FindChildByName(Transform parent, string name)
        {
            if (parent == null) return null;
            var direct = parent.Find(name);
            if (direct != null) return direct;
            foreach (Transform child in parent)
            {
                var res = FindChildByName(child, name);
                if (res != null) return res;
            }
            return null;
        }

        private static Transform FindChildWithTMP(Transform parent)
        {
            if (parent == null) return null;
            foreach (Transform child in parent)
            {
                if (child.GetComponent<TextMeshProUGUI>() != null)
                    return child;
                var res = FindChildWithTMP(child);
                if (res != null) return res;
            }
            return null;
        }


        // Build a simple hierarchy path for diagnostics
        private static string GetHierarchyPath(Transform t)
        {
            if (t == null) return "<null>";
            var path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return string.IsNullOrEmpty(path) ? "<null>" : path;
        }

        private Toggle CreateToggle(Transform parent, string labelText, bool startValue, Action<bool> onChanged)
        {
            var go = new GameObject("Toggle_" + labelText, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120f, 48f);

            var toggleLayout = go.AddComponent<LayoutElement>();
            toggleLayout.preferredWidth = 120f;
            toggleLayout.preferredHeight = 48f;

            var toggle = go.AddComponent<Toggle>();

            // Background image
            var bgGO = new GameObject("Background", typeof(RectTransform));
            bgGO.transform.SetParent(go.transform, false);
            var bg = bgGO.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0.08f);
            var bgRt = bgGO.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0f);
            bgRt.anchorMax = new Vector2(1f, 1f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            toggle.targetGraphic = bg;

            // Checkbox area on the left
            var checkboxGO = new GameObject("Checkbox", typeof(RectTransform));
            checkboxGO.transform.SetParent(go.transform, false);
            var checkboxRT = checkboxGO.GetComponent<RectTransform>();
            checkboxRT.anchorMin = new Vector2(0f, 0.5f);
            checkboxRT.anchorMax = new Vector2(0f, 0.5f);
            checkboxRT.pivot = new Vector2(0f, 0.5f);
            checkboxRT.anchoredPosition = new Vector2(8f, 0f);
            checkboxRT.sizeDelta = new Vector2(20f, 20f);

            // Checkbox background
            var checkboxBg = checkboxGO.AddComponent<Image>();
            checkboxBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Checkmark (visible when toggled on)
            var checkmarkGO = new GameObject("Checkmark", typeof(RectTransform));
            checkmarkGO.transform.SetParent(checkboxGO.transform, false);
            var checkmarkRT = checkmarkGO.GetComponent<RectTransform>();
            checkmarkRT.anchorMin = Vector2.zero;
            checkmarkRT.anchorMax = Vector2.one;
            checkmarkRT.offsetMin = new Vector2(3f, 3f);
            checkmarkRT.offsetMax = new Vector2(-3f, -3f);

            var checkmark = checkmarkGO.AddComponent<Image>();
            checkmark.color = new Color(0.2f, 1f, 0.3f, 1f); // Green checkmark

            toggle.graphic = checkmark;

            // Label (positioned after checkbox)
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = labelText;
            label.fontSize = 16;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget = false;
            var labelRt = labelGO.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0f, 0.5f);
            labelRt.anchorMax = new Vector2(1f, 0.5f);
            labelRt.pivot = new Vector2(0f, 0.5f);
            labelRt.anchoredPosition = new Vector2(36f, 0f);
            labelRt.sizeDelta = new Vector2(-44f, 24f);

            toggle.isOn = startValue;
            toggle.onValueChanged.AddListener(v =>
            {
                onChanged?.Invoke(v);
            });

            return toggle;
        }

        private void OnAlphaChanged(float sliderValue)
        {
            var alpha = Mathf.Clamp01(sliderValue / 10f);
            ModConfig.SetTransparency(alpha);
            if (_alphaPct != null)
                _alphaPct.text = $"{Mathf.RoundToInt(alpha * 100f)}%";
        }

        private void OnScalePercentChanged(float valuePercent)
        {
            _scale = Mathf.Clamp(valuePercent / 100f, 0.5f, 2f);
            ModConfig.SetUiScale(_scale);
            if (_panel != null)
                _panel.localScale = new Vector3(_scale, _scale, 1f);
        }

        private static string GetLocalizedText(string key, string fallback)
        {
            return ModLocalization.GetText(key, LocalizationManager.CurrentLanguage, fallback);
        }
    }
}
