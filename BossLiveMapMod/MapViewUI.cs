using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Duckov.MiniMaps.UI;
using System.IO;
using SodaCraft.Localizations;

namespace BossLiveMapMod
{
    /// <summary>
    /// Runtime UI component attached to the MiniMapView to add live controls and the boss list scroll view.
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
        private Toggle _toggleBossList;

        private Slider _alphaSlider;
        private TextMeshProUGUI _alphaPct;

        // Boss list UI elements (scroll view)
        private ScrollRect _bossScrollRect;
        private RectTransform _bossScrollRoot;
        private RectTransform _bossContent;
        private TextMeshProUGUI _bossListText;

        // Maximum height the scroll rect may reach (screen-space)
        private float _bossScrollMaxHeight;

        // Local copy of special preset names to display under boss list
        private List<string> _specialPresetList = new List<string>();

        private float _scale = 1f;
        private bool _initialized = false;
        private string _lastBossListContent = string.Empty;

        public static MapViewUI Ensure()
        {
            var view = MiniMapView.Instance;
            if (view == null) return null;

            var existing = view.GetComponent<MapViewUI>();
            if (existing != null) return existing;

            var all = FindObjectsOfType<MapViewUI>();
            if (all != null && all.Length > 0)
            {
                foreach (var m in all)
                {
                    if (m == null) continue;
                    if (m._view == view)
                    {
                        m.transform.SetParent(view.transform, false);
                        return m;
                    }
                }

                var pick = all[0];
                pick.transform.SetParent(view.transform, false);
                pick.Initialize(view);
                return pick;
            }

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
            if (_initialized && _view == view) return;

            _view = view;
            InitializeLocalization();

            if (!_initialized)
            {
                Build();
                _initialized = true;
            }
            else
            {
                try
                {
                    var mapCanvas = _view.GetComponentInChildren<Canvas>();
                    var parentTransform = (mapCanvas != null) ? mapCanvas.transform : _view.transform;
                    if (_panel != null) _panel.transform.SetParent(parentTransform, false);
                }
                catch { }
            }

            var viewActive = (_view != null && _view.gameObject.activeInHierarchy);
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
            catch { }
        }

        private void Update()
        {
            try
            {
                if (_panel != null)
                {
                    bool shouldBeActive = _view != null && _view.gameObject.activeInHierarchy;
                    if (_panel.gameObject.activeSelf != shouldBeActive)
                    {
                        _panel.gameObject.SetActive(shouldBeActive);
                    }
                }

                // Update boss list content and adjust scroll height to fit content up to max
                if (_bossScrollRoot != null && _bossListText != null)
                {
                    bool shouldShow = ModConfig.ShowBossList;
                    if (_bossScrollRoot.gameObject.activeSelf != shouldShow)
                        _bossScrollRoot.gameObject.SetActive(shouldShow);

                    if (shouldShow)
                    {
                        // Compose lines: bosses then separator then special presets
                        var bossLines = ModBehaviour.BossNames ?? new List<string>();
                        var combined = new List<string>();
                        if (bossLines.Count > 0) combined.AddRange(bossLines);
                        // Build special lines from BossList entries whose preset name is in _specialPresetList
                        var specialLines = new List<string>();
                        try
                        {
                            var bl = ModBehaviour.BossList;
                            if (bl != null && _specialPresetList != null && _specialPresetList.Count > 0)
                            {
                                foreach (var be in bl)
                                {
                                    if (be == null) continue;
                                    try
                                    {
                                        var preset = be.Character?.characterPreset;
                                        if (preset == null) continue;
                                        string presetName = null;
                                        try { presetName = preset.name; } catch { }
                                        try { if (string.IsNullOrEmpty(presetName)) presetName = preset.nameKey; } catch { }
                                        try { if (string.IsNullOrEmpty(presetName)) presetName = preset.DisplayName; } catch { }
                                        if (string.IsNullOrEmpty(presetName)) continue;
                                        if (_specialPresetList.Exists(x => string.Equals(x, presetName, StringComparison.OrdinalIgnoreCase)))
                                        {
                                            var disp = string.IsNullOrEmpty(be.DisplayName) ? "*" : be.DisplayName;
                                            if (!be.Alive) disp = $"<s>{disp}</s>";
                                            specialLines.Add(disp);
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch { }
                        if (specialLines.Count > 0)
                        {
                            if (combined.Count > 0) combined.Add("───────────");
                            combined.AddRange(specialLines);
                        }

                        _bossListText.richText = true;
                        var newContent = combined.Count > 0 ? string.Join("\n", combined) : string.Empty;
                        bool contentChanged = newContent != _lastBossListContent;
                        _bossListText.text = newContent;
                        _lastBossListContent = newContent;

                        // Force layout rebuild then compute preferred height
                        if (_bossContent != null)
                        {
                            LayoutRebuilder.ForceRebuildLayoutImmediate(_bossContent);
                            float preferred = LayoutUtility.GetPreferredHeight(_bossContent);
                            float padding = 8f;
                            float contentHeight = Mathf.Ceil(preferred + padding);
                            float desiredScrollH = Mathf.Min(contentHeight, _bossScrollMaxHeight > 0f ? _bossScrollMaxHeight : Screen.height * 0.62f);
                            desiredScrollH = Mathf.Max(28f, desiredScrollH);

                            var rd = _bossScrollRoot.sizeDelta;
                            _bossScrollRoot.sizeDelta = new Vector2(rd.x, desiredScrollH);

                            // Only reset scroll position when content actually changes
                            if (contentChanged && _bossScrollRect != null)
                            {
                                _bossScrollRect.verticalNormalizedPosition = 1f;
                            }
                        }
                    }
                }
            }
            catch
            {
                // swallow UI layout errors to avoid disrupting game
            }
        }

        private void OnDestroy()
        {
            if (_toggleAll != null) _toggleAll.onValueChanged.RemoveAllListeners();
            if (_toggleLive != null) _toggleLive.onValueChanged.RemoveAllListeners();
            if (_toggleNames != null) _toggleNames.onValueChanged.RemoveAllListeners();
            if (_toggleNearby != null) _toggleNearby.onValueChanged.RemoveAllListeners();
            if (_toggleBossList != null) _toggleBossList.onValueChanged.RemoveAllListeners();
            if (_alphaSlider != null) _alphaSlider.onValueChanged.RemoveAllListeners();

            try { if (_panel != null && _panel.gameObject != null) Destroy(_panel.gameObject); } catch { }
            _panel = null;
            _initialized = false;
        }

        private void Build()
        {
            Transform parentTransform = _view.transform;
            Canvas mapCanvas = _view.GetComponentInChildren<Canvas>();
            if (mapCanvas != null) parentTransform = mapCanvas.transform;

            var existingPanel = FindChildByName(parentTransform, "BLM_ControlsPanel");
            if (existingPanel != null) { try { Destroy(existingPanel.gameObject); } catch { } }

            var panelGO = new GameObject("BLM_ControlsPanel", typeof(RectTransform));
            panelGO.transform.SetParent(parentTransform, false);
            _panel = panelGO.GetComponent<RectTransform>();

            _scale = Mathf.Clamp(ModConfig.UiScale, 0.5f, 2f);
            _panel.localScale = new Vector3(_scale, _scale, 1f);

            _panel.anchorMin = new Vector2(0f, 1f);
            _panel.anchorMax = new Vector2(0f, 1f);
            _panel.pivot = new Vector2(0f, 1f);
            _panel.anchoredPosition = new Vector2(10f, -10f);
            _panel.sizeDelta = new Vector2(420f, 420f);
            panelGO.transform.SetAsLastSibling();

            var cg = panelGO.AddComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.alpha = 1f;

            var v = panelGO.AddComponent<VerticalLayoutGroup>();
            v.spacing = 8f;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = false;
            v.childAlignment = TextAnchor.UpperLeft;
            v.padding = new RectOffset(12, 12, 8, 8);

            var fitter = panelGO.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Checkbox row
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

            _toggleAll = CreateToggle(checkboxRow.transform, GetLocalizedText("mobs", "Mobs"), ModConfig.ShowAllEnemies, v => ModConfig.SetShowAllEnemies(v));
            _toggleNearby = CreateToggle(checkboxRow.transform, GetLocalizedText("nearby", "Nearby"), ModConfig.ShowNearbyOnly, v => ModConfig.SetShowNearbyOnly(v));
            _toggleLive = CreateToggle(checkboxRow.transform, GetLocalizedText("live", "Live"), ModConfig.ShowLivePositions, v => ModConfig.SetShowLivePositions(v));
            _toggleNames = CreateToggle(checkboxRow.transform, GetLocalizedText("markers", "Markers"), ModConfig.ShowMarkers, v => ModConfig.SetShowMarkers(v));
            _toggleBossList = CreateToggle(checkboxRow.transform, GetLocalizedText("bosslist", "Boss List"), ModConfig.ShowBossList, v => ModConfig.SetShowBossList(v));

            // Alpha slider
            var sliderContainer = new GameObject("AlphaContainer", typeof(RectTransform));
            sliderContainer.transform.SetParent(panelGO.transform, false);
            var srt = sliderContainer.GetComponent<RectTransform>();
            srt.sizeDelta = new Vector2(380f, 40f);
            var sliderLayout = sliderContainer.AddComponent<LayoutElement>();
            sliderLayout.preferredWidth = 380f;
            sliderLayout.preferredHeight = 40f;

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

            // Background track
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

            // Boss list scroll view setup
            float dpi = 0f;
            try { dpi = Screen.dpi; } catch { dpi = 0f; }
            float dpiScale = 1f;
            if (dpi > 0f) dpiScale = Mathf.Clamp(dpi / 96f, 0.75f, 3.0f);
            const float baseFontSize = 56f;
            float screenH = (Screen.height > 0) ? Screen.height : 1080f;
            float maxScrollHeight = Mathf.Clamp(screenH * 0.62f, 120f, screenH * 0.9f);
            int fontSize = Mathf.RoundToInt(baseFontSize * dpiScale);

            var bossScrollRootGO = new GameObject("BossListScroll", typeof(RectTransform));
            bossScrollRootGO.transform.SetParent(panelGO.transform, false);
            _bossScrollRoot = bossScrollRootGO.GetComponent<RectTransform>();
            _bossScrollMaxHeight = maxScrollHeight;
            _bossScrollRoot.sizeDelta = new Vector2(380f, maxScrollHeight);
            var bossScrollLayoutElement = bossScrollRootGO.AddComponent<LayoutElement>();
            bossScrollLayoutElement.preferredWidth = 380f;
            bossScrollLayoutElement.preferredHeight = maxScrollHeight;

            // Load special preset list for UI display
            try
            {
                _specialPresetList.Clear();
                var assemblyLocation = typeof(MapViewUI).Assembly.Location;
                var modFolder = Path.GetDirectoryName(assemblyLocation);
                if (!string.IsNullOrEmpty(modFolder))
                {
                    var specialPath = Path.Combine(modFolder, "special_presets.txt");
                    if (File.Exists(specialPath))
                    {
                        foreach (var raw in File.ReadAllLines(specialPath))
                        {
                            if (string.IsNullOrWhiteSpace(raw)) continue;
                            var t = raw.Trim();
                            if (t.StartsWith("#")) continue;
                            _specialPresetList.Add(t);
                        }
                    }
                }
            }
            catch { }

            _bossScrollRect = bossScrollRootGO.AddComponent<ScrollRect>();
            _bossScrollRect.horizontal = false;
            _bossScrollRect.vertical = true;
            _bossScrollRect.movementType = ScrollRect.MovementType.Clamped;
            _bossScrollRect.scrollSensitivity = 5f;

            // Create simple scrollbar indicator (right side) - must be created AFTER viewport

            var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewportGO.transform.SetParent(bossScrollRootGO.transform, false);
            var viewportRT = viewportGO.GetComponent<RectTransform>();
            viewportRT.anchorMin = new Vector2(0f, 0f);
            viewportRT.anchorMax = new Vector2(1f, 1f);
            viewportRT.pivot = new Vector2(0.5f, 0.5f);
            viewportRT.sizeDelta = Vector2.zero;
            var viewportImg = viewportGO.GetComponent<Image>();
            viewportImg.color = new Color(0f, 0f, 0f, 0f);

            _bossScrollRect.viewport = viewportRT;

            var contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(viewportGO.transform, false);
            _bossContent = contentGO.GetComponent<RectTransform>();
            _bossContent.anchorMin = new Vector2(0f, 1f);
            _bossContent.anchorMax = new Vector2(1f, 1f);
            _bossContent.pivot = new Vector2(0.5f, 1f);
            _bossContent.anchoredPosition = Vector2.zero;
            _bossContent.sizeDelta = new Vector2(0f, 0f);

            var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.spacing = 0f;

            var contentFitter = contentGO.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var bossTextGO = new GameObject("BossListText", typeof(RectTransform));
            bossTextGO.transform.SetParent(contentGO.transform, false);
            _bossListText = bossTextGO.AddComponent<TextMeshProUGUI>();
            _bossListText.text = string.Empty;
            _bossListText.fontSize = fontSize;
            _bossListText.color = Color.white;
            _bossListText.enableWordWrapping = true;
            _bossListText.raycastTarget = false;
            _bossListText.richText = true;

            var uiOutline = bossTextGO.AddComponent<UnityEngine.UI.Outline>();
            uiOutline.effectColor = Color.black;
            uiOutline.effectDistance = new Vector2(Mathf.Max(1f, dpiScale), -Mathf.Max(1f, dpiScale));

            var textLayout = bossTextGO.AddComponent<LayoutElement>();
            textLayout.preferredWidth = 0;
            textLayout.flexibleWidth = 1f;

            _bossScrollRect.content = _bossContent;
            _bossScrollRect.verticalNormalizedPosition = 1f;

            // Create simple scrollbar indicator AFTER viewport (so it renders on top)
            var scrollbarGO = new GameObject("ScrollbarIndicator", typeof(RectTransform));
            scrollbarGO.transform.SetParent(bossScrollRootGO.transform, false);
            scrollbarGO.transform.SetAsLastSibling(); // Render on top
            var scrollbarRT = scrollbarGO.GetComponent<RectTransform>();
            scrollbarRT.anchorMin = new Vector2(1f, 0f);
            scrollbarRT.anchorMax = new Vector2(1f, 1f);
            scrollbarRT.pivot = new Vector2(1f, 0.5f);
            scrollbarRT.sizeDelta = new Vector2(8f, 0f);
            scrollbarRT.anchoredPosition = new Vector2(-4f, 0f);

            var scrollbarImg = scrollbarGO.AddComponent<Image>();
            scrollbarImg.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);
            scrollbarImg.raycastTarget = false;

            _bossScrollRoot.gameObject.SetActive(ModConfig.ShowBossList);
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

            var checkboxGO = new GameObject("Checkbox", typeof(RectTransform));
            checkboxGO.transform.SetParent(go.transform, false);
            var checkboxRT = checkboxGO.GetComponent<RectTransform>();
            checkboxRT.anchorMin = new Vector2(0f, 0.5f);
            checkboxRT.anchorMax = new Vector2(0f, 0.5f);
            checkboxRT.pivot = new Vector2(0f, 0.5f);
            checkboxRT.anchoredPosition = new Vector2(8f, 0f);
            checkboxRT.sizeDelta = new Vector2(20f, 20f);

            var checkboxBg = checkboxGO.AddComponent<Image>();
            checkboxBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            var checkmarkGO = new GameObject("Checkmark", typeof(RectTransform));
            checkmarkGO.transform.SetParent(checkboxGO.transform, false);
            var checkmarkRT = checkmarkGO.GetComponent<RectTransform>();
            checkmarkRT.anchorMin = Vector2.zero;
            checkmarkRT.anchorMax = Vector2.one;
            checkmarkRT.offsetMin = new Vector2(3f, 3f);
            checkmarkRT.offsetMax = new Vector2(-3f, -3f);

            var checkmark = checkmarkGO.AddComponent<Image>();
            checkmark.color = new Color(0.2f, 1f, 0.3f, 1f);

            toggle.graphic = checkmark;

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
            toggle.onValueChanged.AddListener(v => { onChanged?.Invoke(v); });

            return toggle;
        }

        private void OnAlphaChanged(float sliderValue)
        {
            var alpha = Mathf.Clamp01(sliderValue / 10f);
            ModConfig.SetTransparency(alpha);
            if (_alphaPct != null) _alphaPct.text = $"{Mathf.RoundToInt(alpha * 100f)}%";
        }

        private void OnScalePercentChanged(float valuePercent)
        {
            _scale = Mathf.Clamp(valuePercent / 100f, 0.5f, 2f);
            ModConfig.SetUiScale(_scale);
            if (_panel != null) _panel.localScale = new Vector3(_scale, _scale, 1f);
        }

        private static string GetLocalizedText(string key, string fallback)
        {
            try
            {
                return ModLocalization.GetText(key, LocalizationManager.CurrentLanguage, fallback);
            }
            catch { }
            return fallback;
        }
    }
}
