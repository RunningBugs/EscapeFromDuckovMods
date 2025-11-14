using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Duckov.MiniMaps.UI;
using System.IO;
using SodaCraft.Localizations;

namespace BossLiveMapMod
{
    /// <summary>
    /// Properly handles scroll wheel events by calculating scroll amount based on content size.
    /// </summary>
    public sealed class BossListScrollHandler : MonoBehaviour, IScrollHandler
    {
        private ScrollRect _scrollRect;
        private RectTransform _content;
        private RectTransform _viewport;

        public void Initialize(ScrollRect scrollRect, RectTransform content, RectTransform viewport)
        {
            _scrollRect = scrollRect;
            _content = content;
            _viewport = viewport;
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (_scrollRect == null || _content == null || _viewport == null)
                return;

            // Calculate scroll amount based on content and viewport size
            float contentHeight = _content.rect.height;
            float viewportHeight = _viewport.rect.height;

            if (contentHeight <= viewportHeight)
                return; // No need to scroll if content fits

            // Calculate normalized scroll step
            // Each scroll tick should move approximately 3% of the viewport
            // float scrollStep = (viewportHeight * 0.01f) / (contentHeight - viewportHeight);
            float scrollStep = 0.001f;
            float delta = eventData.scrollDelta.y * scrollStep;
            // float delta = 10f;

            float newPosition = _scrollRect.verticalNormalizedPosition + delta;
            _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(newPosition);
        }
    }

    /// <summary>
    /// Runtime UI component attached to the MiniMapView to add live controls and the boss list scroll view.
    /// </summary>
    public sealed class MapViewUI : MonoBehaviour
    {
        private MiniMapView _view;
        private RectTransform _panel;

        private Toggle _toggleAll;
        private Toggle _toggleLive;
        private Toggle _toggleNames;
        private Toggle _toggleNearby;
        private Toggle _toggleBossList;
        private Toggle _toggleMarkerNames;

        private Slider _alphaSlider;
        private TextMeshProUGUI _alphaPct;

        private Slider _fontSizeSlider;
        private TextMeshProUGUI _fontSizePct;

        // Boss list UI elements (scroll view)
        private ScrollRect _bossScrollRect;
        private RectTransform _bossScrollRoot;
        private RectTransform _bossContent;
        private TextMeshProUGUI _bossListText;
        private LayoutElement _bossScrollLayoutElement;

        private float _bossScrollMaxHeightPixels;
        private const float BossListBaseFontSize = 28f;

        // Local copy of special preset names to display under boss list
        private List<string> _specialPresetList = new List<string>();

        private float _scale = 1f;
        private bool _initialized = false;
        private string _lastBossListContent = string.Empty;

        // UI update cooldown to reduce per-frame overhead
        private float _uiUpdateCooldown = 0f;
        private const float UiUpdateInterval = 0.3f; // Update every 0.3 seconds

        // Cached layout dimensions to avoid unnecessary rebuilds
        private float _lastScrollWidth = -1f;
        private float _lastScrollHeight = -1f;
        private float _lastCanvasScale = -1f;
        private float _lastPanelScale = -1f;
        private int _lastScreenWidth = -1;
        private int _lastScreenHeight = -1;
        private bool _forceLayoutPass;

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

        private Canvas GetRootCanvas()
        {
            if (_panel == null)
                return null;

            try
            {
                var canvas = _panel.GetComponentInParent<Canvas>();
                if (canvas == null)
                    return null;
                return canvas.rootCanvas != null ? canvas.rootCanvas : canvas;
            }
            catch
            {
                return null;
            }
        }

        private float GetCanvasScaleFactor()
        {
            var canvas = GetRootCanvas();
            if (canvas != null)
            {
                var scale = canvas.scaleFactor;
                if (scale > 0.0001f)
                    return scale;
            }
            return 1f;
        }

        private float GetPanelScale()
        {
            if (_panel == null)
                return 1f;

            try
            {
                var lossy = _panel.lossyScale;
                var scale = Mathf.Abs(lossy.y);
                if (scale < 0.0001f)
                    scale = 1f;
                return scale;
            }
            catch
            {
                return 1f;
            }
        }

        private float PixelsToUiUnits(float pixels)
        {
            float canvasScale = GetCanvasScaleFactor();
            float panelScale = GetPanelScale();
            float denom = canvasScale * panelScale;
            if (denom < 0.0001f)
                denom = 1f;
            return pixels / denom;
        }

        private float ComputeDefaultMaxHeightPixels()
        {
            float screenHeight = Screen.height > 0 ? Screen.height : 1080f;
            return Mathf.Clamp(screenHeight * 0.62f, 120f, screenHeight * 0.9f);
        }

        private bool UpdateScreenCache()
        {
            int width = Screen.width > 0 ? Screen.width : (_lastScreenWidth > 0 ? _lastScreenWidth : 1920);
            int height = Screen.height > 0 ? Screen.height : (_lastScreenHeight > 0 ? _lastScreenHeight : 1080);

            if (width != _lastScreenWidth || height != _lastScreenHeight)
            {
                _lastScreenWidth = width;
                _lastScreenHeight = height;
                return true;
            }

            return false;
        }

        private bool UpdateScaleCache()
        {
            float canvasScale = GetCanvasScaleFactor();
            float panelScale = GetPanelScale();

            if (Mathf.Abs(canvasScale - _lastCanvasScale) > 0.001f ||
                Mathf.Abs(panelScale - _lastPanelScale) > 0.001f)
            {
                _lastCanvasScale = canvasScale;
                _lastPanelScale = panelScale;
                return true;
            }

            return false;
        }

        private float GetBossFontSize()
        {
            float scale = Mathf.Clamp(ModConfig.BossFontScale, 0.1f, 1.5f);
            return BossListBaseFontSize * scale;
        }

        private bool ApplyBossFontSize()
        {
            if (_bossListText == null)
                return false;

            float fontSize = GetBossFontSize();
            if (Mathf.Abs(_bossListText.fontSize - fontSize) > 0.05f)
            {
                _bossListText.fontSize = fontSize;
                return true;
            }

            return false;
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

                // Update boss list content with cooldown to reduce per-frame overhead
                _uiUpdateCooldown -= Time.deltaTime;
                if (_uiUpdateCooldown > 0f)
                    return;

                _uiUpdateCooldown = UiUpdateInterval;

                if (_bossScrollRoot != null && _bossListText != null)
                {
                    bool shouldShow = ModConfig.ShowBossList;
                    if (_bossScrollRoot.gameObject.activeSelf != shouldShow)
                    {
                        _bossScrollRoot.gameObject.SetActive(shouldShow);
                        if (shouldShow)
                            _forceLayoutPass = true;
                    }

                    if (shouldShow)
                    {
                        // Use cached formatted lists (no allocations if unchanged)
                        var bossLines = ModBehaviour.BossNames ?? new List<string>();
                        var specialLines = ModBehaviour.SpecialNames ?? new List<string>();

                        var combined = new List<string>();
                        if (bossLines.Count > 0) combined.AddRange(bossLines);
                        if (specialLines.Count > 0)
                        {
                            if (combined.Count > 0) combined.Add("──────");
                            combined.AddRange(specialLines);
                        }

                        _bossListText.richText = true;
                        var newContent = combined.Count > 0 ? string.Join("\n", combined) : string.Empty;
                        bool contentChanged = newContent != _lastBossListContent;

                        if (contentChanged)
                        {
                            _bossListText.text = newContent;
                            _lastBossListContent = newContent;
                        }

                        bool fontSizeChanged = ApplyBossFontSize();
                        bool scaleChanged = UpdateScaleCache();
                        bool screenChanged = UpdateScreenCache();
                        if (screenChanged)
                        {
                            _bossScrollMaxHeightPixels = ComputeDefaultMaxHeightPixels();
                        }

                        bool forceLayout = _forceLayoutPass;
                        _forceLayoutPass = false;

                        bool layoutNeedsUpdate = contentChanged || fontSizeChanged || scaleChanged || screenChanged || forceLayout;

                        if (layoutNeedsUpdate && _bossContent != null)
                        {
                            LayoutRebuilder.ForceRebuildLayoutImmediate(_bossContent);

                            float paddingH = PixelsToUiUnits(8f);
                            float preferredHeight = LayoutUtility.GetPreferredHeight(_bossContent);
                            float contentHeight = Mathf.Ceil(preferredHeight + paddingH);

                            float maxHeightPixels = _bossScrollMaxHeightPixels > 0f ? _bossScrollMaxHeightPixels : ComputeDefaultMaxHeightPixels();
                            float maxHeightUi = PixelsToUiUnits(maxHeightPixels);
                            float minHeightUi = PixelsToUiUnits(28f);
                            float desiredScrollH = Mathf.Min(contentHeight, maxHeightUi);
                            desiredScrollH = Mathf.Max(minHeightUi, desiredScrollH);

                            float paddingW = PixelsToUiUnits(20f);
                            float minWidthUi = PixelsToUiUnits(280f);
                            float maxWidthUi = PixelsToUiUnits(600f);
                            float contentWidth = Mathf.Ceil(_bossListText.preferredWidth + paddingW);
                            float desiredScrollW = Mathf.Clamp(contentWidth, minWidthUi, maxWidthUi);

                            bool dimensionsChanged = Mathf.Abs(_lastScrollWidth - desiredScrollW) > 0.5f ||
                                                    Mathf.Abs(_lastScrollHeight - desiredScrollH) > 0.5f;

                            if (dimensionsChanged)
                            {
                                _bossScrollRoot.sizeDelta = new Vector2(desiredScrollW, desiredScrollH);

                                if (_bossScrollLayoutElement != null)
                                {
                                    _bossScrollLayoutElement.preferredWidth = desiredScrollW;
                                    _bossScrollLayoutElement.preferredHeight = desiredScrollH;
                                }

                                _lastScrollWidth = desiredScrollW;
                                _lastScrollHeight = desiredScrollH;
                            }

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

            // Checkbox row 1
            var checkboxRow1 = new GameObject("CheckboxRow1", typeof(RectTransform));
            checkboxRow1.transform.SetParent(panelGO.transform, false);
            var row1RT = checkboxRow1.GetComponent<RectTransform>();
            row1RT.sizeDelta = new Vector2(400f, 36f);
            var row1Layout = checkboxRow1.AddComponent<HorizontalLayoutGroup>();
            row1Layout.spacing = 8f;
            row1Layout.childForceExpandHeight = false;
            row1Layout.childForceExpandWidth = false;
            row1Layout.childAlignment = TextAnchor.MiddleLeft;
            var row1LayoutElement = checkboxRow1.AddComponent<LayoutElement>();
            row1LayoutElement.preferredWidth = 400f;
            row1LayoutElement.preferredHeight = 36f;

            _toggleAll = CreateToggle(checkboxRow1.transform, GetLocalizedText("mobs", "Mobs"), ModConfig.ShowAllEnemies, v => ModConfig.SetShowAllEnemies(v));
            _toggleNearby = CreateToggle(checkboxRow1.transform, GetLocalizedText("nearby", "Nearby"), ModConfig.ShowNearbyOnly, v => ModConfig.SetShowNearbyOnly(v));
            _toggleLive = CreateToggle(checkboxRow1.transform, GetLocalizedText("live", "Live"), ModConfig.ShowLivePositions, v => ModConfig.SetShowLivePositions(v));

            // Checkbox row 2
            var checkboxRow2 = new GameObject("CheckboxRow2", typeof(RectTransform));
            checkboxRow2.transform.SetParent(panelGO.transform, false);
            var row2RT = checkboxRow2.GetComponent<RectTransform>();
            row2RT.sizeDelta = new Vector2(400f, 36f);
            var row2Layout = checkboxRow2.AddComponent<HorizontalLayoutGroup>();
            row2Layout.spacing = 8f;
            row2Layout.childForceExpandHeight = false;
            row2Layout.childForceExpandWidth = false;
            row2Layout.childAlignment = TextAnchor.MiddleLeft;
            var row2LayoutElement = checkboxRow2.AddComponent<LayoutElement>();
            row2LayoutElement.preferredWidth = 400f;
            row2LayoutElement.preferredHeight = 36f;

            _toggleNames = CreateToggle(checkboxRow2.transform, GetLocalizedText("markers", "Markers"), ModConfig.ShowMarkers, v => ModConfig.SetShowMarkers(v));
            _toggleBossList = CreateToggle(checkboxRow2.transform, GetLocalizedText("bosslist", "Boss List"), ModConfig.ShowBossList, v => ModConfig.SetShowBossList(v));
            _toggleMarkerNames = CreateToggle(checkboxRow2.transform, GetLocalizedText("markernames", "Names"), ModConfig.ShowMarkerNames, v => ModConfig.SetShowMarkerNames(v));

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

            // Font Size slider
            var fontSizeContainer = new GameObject("FontSizeContainer", typeof(RectTransform));
            fontSizeContainer.transform.SetParent(panelGO.transform, false);
            var fontSizeContainerRT = fontSizeContainer.GetComponent<RectTransform>();
            fontSizeContainerRT.sizeDelta = new Vector2(380f, 40f);
            var fontSizeLayout = fontSizeContainer.AddComponent<LayoutElement>();
            fontSizeLayout.preferredWidth = 380f;
            fontSizeLayout.preferredHeight = 40f;

            var fontLabelGO = new GameObject("FontSizeLabel", typeof(RectTransform));
            fontLabelGO.transform.SetParent(fontSizeContainer.transform, false);
            var fontLabel = fontLabelGO.AddComponent<TextMeshProUGUI>();
            fontLabel.text = GetLocalizedText("fontsize", "Font Size");
            fontLabel.fontSize = 14;
            fontLabel.color = Color.white;
            fontLabel.alignment = TextAlignmentOptions.MidlineLeft;
            fontLabel.raycastTarget = false;
            var fontLabelRT = fontLabelGO.GetComponent<RectTransform>();
            fontLabelRT.anchorMin = new Vector2(0f, 0.5f);
            fontLabelRT.anchorMax = new Vector2(0f, 0.5f);
            fontLabelRT.pivot = new Vector2(0f, 0.5f);
            fontLabelRT.anchoredPosition = new Vector2(8f, 0f);
            fontLabelRT.sizeDelta = new Vector2(70f, 28f);

            var fontSliderGO = new GameObject("FontSizeSlider", typeof(RectTransform));
            fontSliderGO.transform.SetParent(fontSizeContainer.transform, false);
            var fontSliderRT = fontSliderGO.GetComponent<RectTransform>();
            fontSliderRT.anchorMin = new Vector2(0f, 0.5f);
            fontSliderRT.anchorMax = new Vector2(1f, 0.5f);
            fontSliderRT.pivot = new Vector2(0f, 0.5f);
            fontSliderRT.anchoredPosition = new Vector2(85f, 0f);
            fontSliderRT.sizeDelta = new Vector2(-145f, 20f);

            _fontSizeSlider = fontSliderGO.AddComponent<Slider>();
            _fontSizeSlider.minValue = 0.1f;
            _fontSizeSlider.maxValue = 1.5f;
            _fontSizeSlider.value = Mathf.Clamp(ModConfig.BossFontScale, 0.1f, 1.5f);
            _fontSizeSlider.onValueChanged.AddListener(OnFontSizeChanged);

            var fontBgGO = new GameObject("Background", typeof(RectTransform));
            fontBgGO.transform.SetParent(fontSliderGO.transform, false);
            var fontBgImg = fontBgGO.AddComponent<Image>();
            fontBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            var fontBgRT = fontBgGO.GetComponent<RectTransform>();
            fontBgRT.anchorMin = new Vector2(0f, 0.25f);
            fontBgRT.anchorMax = new Vector2(1f, 0.75f);
            fontBgRT.offsetMin = Vector2.zero;
            fontBgRT.offsetMax = Vector2.zero;

            var fontFillAreaGO = new GameObject("FillArea", typeof(RectTransform));
            fontFillAreaGO.transform.SetParent(fontSliderGO.transform, false);
            var fontFillAreaRT = fontFillAreaGO.GetComponent<RectTransform>();
            fontFillAreaRT.anchorMin = new Vector2(0f, 0.25f);
            fontFillAreaRT.anchorMax = new Vector2(1f, 0.75f);
            fontFillAreaRT.offsetMin = Vector2.zero;
            fontFillAreaRT.offsetMax = Vector2.zero;

            var fontFillGO = new GameObject("Fill", typeof(RectTransform));
            fontFillGO.transform.SetParent(fontFillAreaGO.transform, false);
            var fontFillImg = fontFillGO.AddComponent<Image>();
            fontFillImg.color = new Color(0.3f, 0.6f, 1f, 0.9f);
            var fontFillRT = fontFillGO.GetComponent<RectTransform>();
            fontFillRT.sizeDelta = Vector2.zero;
            _fontSizeSlider.fillRect = fontFillRT;

            var fontHandleAreaGO = new GameObject("HandleArea", typeof(RectTransform));
            fontHandleAreaGO.transform.SetParent(fontSliderGO.transform, false);
            var fontHandleAreaRT = fontHandleAreaGO.GetComponent<RectTransform>();
            fontHandleAreaRT.anchorMin = new Vector2(0f, 0f);
            fontHandleAreaRT.anchorMax = new Vector2(1f, 1f);
            fontHandleAreaRT.offsetMin = Vector2.zero;
            fontHandleAreaRT.offsetMax = Vector2.zero;

            var fontHandleGO = new GameObject("Handle", typeof(RectTransform));
            fontHandleGO.transform.SetParent(fontHandleAreaGO.transform, false);
            var fontHandleImg = fontHandleGO.AddComponent<Image>();
            fontHandleImg.color = Color.white;
            var fontHandleRT = fontHandleGO.GetComponent<RectTransform>();
            fontHandleRT.anchorMin = new Vector2(0f, 0.5f);
            fontHandleRT.anchorMax = new Vector2(0f, 0.5f);
            fontHandleRT.pivot = new Vector2(0.5f, 0.5f);
            fontHandleRT.sizeDelta = new Vector2(12f, 12f);

            _fontSizeSlider.handleRect = fontHandleRT;

            var fontPctGO = new GameObject("FontSizePct", typeof(RectTransform));
            fontPctGO.transform.SetParent(fontSizeContainer.transform, false);
            _fontSizePct = fontPctGO.AddComponent<TextMeshProUGUI>();
            _fontSizePct.text = $"{Mathf.RoundToInt(Mathf.Clamp(ModConfig.BossFontScale, 0.1f, 1.5f) * 100f)}%";
            _fontSizePct.fontSize = 14;
            _fontSizePct.color = Color.white;
            _fontSizePct.alignment = TextAlignmentOptions.MidlineRight;
            _fontSizePct.raycastTarget = false;
            var fontPctRT = fontPctGO.GetComponent<RectTransform>();
            fontPctRT.anchorMin = new Vector2(1f, 0.5f);
            fontPctRT.anchorMax = new Vector2(1f, 0.5f);
            fontPctRT.pivot = new Vector2(1f, 0.5f);
            fontPctRT.anchoredPosition = new Vector2(-6f, 0f);
            fontPctRT.sizeDelta = new Vector2(48f, 28f);

            // Boss list scroll view setup
            UpdateScreenCache();
            _bossScrollMaxHeightPixels = ComputeDefaultMaxHeightPixels();

            var bossScrollRootGO = new GameObject("BossListScroll", typeof(RectTransform));
            bossScrollRootGO.transform.SetParent(panelGO.transform, false);
            _bossScrollRoot = bossScrollRootGO.GetComponent<RectTransform>();
            float initialWidth = PixelsToUiUnits(280f);
            float initialHeight = PixelsToUiUnits(_bossScrollMaxHeightPixels);
            _bossScrollRoot.sizeDelta = new Vector2(initialWidth, initialHeight); // Start with min width, will adjust dynamically
            _bossScrollLayoutElement = bossScrollRootGO.AddComponent<LayoutElement>();
            _bossScrollLayoutElement.preferredWidth = initialWidth;
            _bossScrollLayoutElement.preferredHeight = initialHeight;

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

            // Add visible light grey background for raycasting
            var scrollBgImg = bossScrollRootGO.AddComponent<Image>();
            scrollBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.3f); // Light grey, 30% opacity
            scrollBgImg.raycastTarget = true;

            _bossScrollRect = bossScrollRootGO.AddComponent<ScrollRect>();
            _bossScrollRect.horizontal = false;
            _bossScrollRect.vertical = true;
            _bossScrollRect.movementType = ScrollRect.MovementType.Clamped;
            _bossScrollRect.scrollSensitivity = 0f; // Disabled - we handle scrolling manually

            var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportGO.transform.SetParent(bossScrollRootGO.transform, false);
            var viewportRT = viewportGO.GetComponent<RectTransform>();
            viewportRT.anchorMin = new Vector2(0f, 0f);
            viewportRT.anchorMax = new Vector2(1f, 1f);
            viewportRT.pivot = new Vector2(0.5f, 0.5f);
            viewportRT.sizeDelta = Vector2.zero;

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
            _bossListText.fontSize = GetBossFontSize();
            _bossListText.color = Color.white;
            _bossListText.enableWordWrapping = false;
            _bossListText.raycastTarget = false;
            _bossListText.richText = true;

            var uiOutline = bossTextGO.AddComponent<UnityEngine.UI.Outline>();
            uiOutline.effectColor = Color.black;
            float outlineSize = PixelsToUiUnits(1f);
            uiOutline.effectDistance = new Vector2(outlineSize, -outlineSize);

            var textLayout = bossTextGO.AddComponent<LayoutElement>();
            textLayout.preferredWidth = 0;
            textLayout.flexibleWidth = 1f;

            _bossScrollRect.content = _bossContent;
            _bossScrollRect.verticalNormalizedPosition = 1f;

            // Create proper scrollbar (based on reference implementation)
            var scrollbarBgGO = new GameObject("Scrollbar_Background", typeof(RectTransform));
            scrollbarBgGO.transform.SetParent(bossScrollRootGO.transform, false);
            var scrollbarBgRT = scrollbarBgGO.GetComponent<RectTransform>();
            scrollbarBgRT.anchorMin = new Vector2(1f, 0f);
            scrollbarBgRT.anchorMax = new Vector2(1f, 1f);
            scrollbarBgRT.pivot = new Vector2(1f, 1f);
            scrollbarBgRT.offsetMin = new Vector2(-13f, 10f);
            scrollbarBgRT.offsetMax = new Vector2(-5f, -10f);
            var scrollbarBgImg = scrollbarBgGO.AddComponent<Image>();
            scrollbarBgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            var scrollbarGO = new GameObject("Scrollbar", typeof(RectTransform));
            scrollbarGO.transform.SetParent(scrollbarBgGO.transform, false);
            var scrollbar = scrollbarGO.AddComponent<Scrollbar>();
            var scrollbarRT = scrollbarGO.GetComponent<RectTransform>();
            scrollbarRT.anchorMin = Vector2.zero;
            scrollbarRT.anchorMax = Vector2.one;
            scrollbarRT.offsetMin = Vector2.zero;
            scrollbarRT.offsetMax = Vector2.zero;

            var slidingAreaGO = new GameObject("Sliding Area", typeof(RectTransform));
            slidingAreaGO.transform.SetParent(scrollbarGO.transform, false);
            var slidingAreaRT = slidingAreaGO.GetComponent<RectTransform>();
            slidingAreaRT.anchorMin = Vector2.zero;
            slidingAreaRT.anchorMax = Vector2.one;
            slidingAreaRT.offsetMin = Vector2.zero;
            slidingAreaRT.offsetMax = Vector2.zero;

            var scrollbarHandleGO = new GameObject("Handle", typeof(RectTransform));
            scrollbarHandleGO.transform.SetParent(slidingAreaGO.transform, false);
            var scrollbarHandleRT = scrollbarHandleGO.GetComponent<RectTransform>();
            scrollbarHandleRT.anchorMin = Vector2.zero;
            scrollbarHandleRT.anchorMax = Vector2.one;
            scrollbarHandleRT.offsetMin = new Vector2(0f, 0f);
            scrollbarHandleRT.offsetMax = new Vector2(0f, 0f);
            var scrollbarHandleImg = scrollbarHandleGO.AddComponent<Image>();
            scrollbarHandleImg.color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
            scrollbarHandleImg.raycastTarget = true;

            scrollbar.handleRect = scrollbarHandleRT;
            scrollbar.targetGraphic = scrollbarHandleImg;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            _bossScrollRect.verticalScrollbar = scrollbar;
            _bossScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            _bossScrollRect.verticalScrollbarSpacing = -3f;

            // Add scroll handler to manually control scrolling
            var scrollHandler = bossScrollRootGO.AddComponent<BossListScrollHandler>();
            scrollHandler.Initialize(_bossScrollRect, _bossContent, viewportRT);

            _forceLayoutPass = true;
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

        private void OnFontSizeChanged(float sliderValue)
        {
            var scale = Mathf.Clamp(sliderValue, 0.1f, 1.5f);
            ModConfig.SetBossFontScale(scale);
            if (_fontSizePct != null) _fontSizePct.text = $"{Mathf.RoundToInt(scale * 100f)}%";
            ApplyBossFontSize();
            _forceLayoutPass = true;
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
