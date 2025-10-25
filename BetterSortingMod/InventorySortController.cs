using System;
using System.Collections.Generic;
using Duckov.UI;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterSortingMod;

public class InventorySortController : MonoBehaviour
{
    private readonly List<Button> _menuButtons = new List<Button>();
    private InventoryDisplay _display;
    private Button _metricsButton;
    private Button _actionButton;
    private Image _metricsIcon;
    private RectTransform _containerRect;
    private RectTransform _menuRect;
    private RectTransform _overlayRect;
    private SortMenuOverlay _overlayComponent;

    private bool _uiInitialized;

    private static readonly Vector3[] WorldCornersBuffer = new Vector3[4];

    private static Sprite triangleSprite;
    // Global, shared overlay root under GameplayUIManager to simplify z-ordering and scaling
    private static RectTransform s_globalOverlayRoot;
    // Cached stripped button template for menu options
    private static Button s_optionTemplate;
    // removed per-canvas overlayContexts; using global overlay root instead

    // removed OverlayContext; using global overlay root instead

    private static RectTransform EnsureGlobalOverlayRoot()
    {
        if (s_globalOverlayRoot != null)
        {
            return s_globalOverlayRoot;
        }
        var manager = GameplayUIManager.Instance;
        if (manager == null)
        {
            return null;
        }
        GameObject rootObj = new GameObject("BetterSortingMod_GlobalOverlayRoot", typeof(RectTransform));
        s_globalOverlayRoot = rootObj.GetComponent<RectTransform>();
        s_globalOverlayRoot.SetParent(manager.transform, false);
        s_globalOverlayRoot.anchorMin = Vector2.zero;
        s_globalOverlayRoot.anchorMax = Vector2.one;
        s_globalOverlayRoot.offsetMin = Vector2.zero;
        s_globalOverlayRoot.offsetMax = Vector2.zero;
        s_globalOverlayRoot.pivot = new Vector2(0.5f, 0.5f);
        s_globalOverlayRoot.SetAsLastSibling();
        return s_globalOverlayRoot;
    }

    private static Button GetOrCreateOptionTemplate(Button sourceButton)
    {
        if (s_optionTemplate != null)
        {
            return s_optionTemplate;
        }
        if (sourceButton == null)
        {
            return null;
        }
        // Build a stripped, neutral option template from the source button only once
        Button template = UnityEngine.Object.Instantiate(sourceButton);
        template.gameObject.name = "BetterSortOptionTemplate";
        template.gameObject.SetActive(false);
        var rect = template.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Remove unwanted components on template root
        var hlg = template.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null) UnityEngine.Object.Destroy(hlg);
        var csf = template.GetComponent<ContentSizeFitter>();
        if (csf != null) UnityEngine.Object.Destroy(csf);
        var cg = template.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        foreach (var mb in template.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
        {
            if (mb == null) continue;
            var t = mb.GetType();
            if (t.FullName == "SodaCraft.Localizations.TextLocalizor" || t.Namespace == "Duckov.UI.Animations")
            {
                UnityEngine.Object.Destroy(mb);
            }
        }
        var tmp = template.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
        if (tmp != null)
        {
            tmp.text = string.Empty;
            tmp.raycastTarget = false;
            var c = tmp.color; c.a = 1f; tmp.color = c;
        }
        // Keep it detached in memory (no parent). We'll clone it for each option.
        s_optionTemplate = template;
        return s_optionTemplate;
    }

    public InventoryDisplay TargetDisplay => _display;

    internal void Initialize(InventoryDisplay target)
    {
        _display = target;
        EnsureUi();
    }

    private void Awake()
    {
        if (_display == null)
        {
            _display = GetComponent<InventoryDisplay>();
        }
    }

    private void OnEnable()
    {
        EnsureUi();
        SyncVisibility();
        HideMenu();
    }



    private void OnDisable()
    {
        HideMenu();
    }

    private void OnDestroy()
    {
        HideMenu();
    }

    private void EnsureUi()
    {
        if (_uiInitialized)
        {
            return;
        }
        if (_display == null || _display.sortButton == null)
        {
            return;
        }
        BuildButtonContainer(_display.sortButton);
        EnsureOverlayAndMenu();
        BuildMenuOptions();
        _uiInitialized = true;
    }

    private void BuildButtonContainer(Button originalButton)
    {
        RectTransform originalRect = originalButton.GetComponent<RectTransform>();
        if (originalRect == null)
        {
            Debug.LogWarning("BetterSortingMod: sort button missing RectTransform.", originalButton);
            return;
        }
        RectTransform parentRect = originalRect.parent as RectTransform;
        if (parentRect == null)
        {
            Debug.LogWarning("BetterSortingMod: sort button parent missing RectTransform.", originalButton);
            return;
        }
        int siblingIndex = originalRect.GetSiblingIndex();
        float baseWidth = originalRect.rect.width;
        float baseHeight = originalRect.rect.height;
        if (baseWidth <= 0f)
        {
            baseWidth = 160f;
        }
        if (baseHeight <= 0f)
        {
            baseHeight = 52f;
        }
        // Reuse existing container if present to avoid duplicate clones on reopen
        Transform existingContainer = parentRect.Find("BetterSortButtonContainer");
        if (existingContainer != null)
        {
            _containerRect = existingContainer.GetComponent<RectTransform>();
            _containerRect.SetSiblingIndex(siblingIndex);
            // Find existing metrics and action clones by name
            Transform metricsT = existingContainer.Find("BetterSortMetricButton");
            if (metricsT != null) _metricsButton = metricsT.GetComponent<Button>();
            Transform actionT = existingContainer.Find("BetterSortActionButton");
            if (actionT != null) _actionButton = actionT.GetComponent<Button>();
            if (_metricsButton != null)
            {
                _metricsButton.onClick.RemoveAllListeners();
                _metricsButton.onClick.AddListener(ToggleMenu);
                EnsureMetricsIcon();
                UpdateMetricsIcon(expanded: false);
            }
            if (_actionButton != null)
            {
                _actionButton.onClick.RemoveAllListeners();
                _actionButton.onClick.AddListener(RunVanillaSort);
            }
            // Keep the original sort button active; it serves as the action button
            originalRect.gameObject.SetActive(true);
            return;
        }

        GameObject containerObject = new GameObject("BetterSortButtonContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        _containerRect = containerObject.GetComponent<RectTransform>();
        _containerRect.SetParent(parentRect, worldPositionStays: false);
        CopyRectTransform(originalRect, _containerRect);
        _containerRect.SetSiblingIndex(siblingIndex);
        _containerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, baseWidth);
        _containerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, baseHeight);
        HorizontalLayoutGroup layout = containerObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        ContentSizeFitter fitter = containerObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        CopyLayoutElement(originalRect, _containerRect);
        var __relay = ActiveStateRelay.Ensure(_containerRect.gameObject);
        __relay.Configure(() => SyncVisibility(), () => SyncVisibility(), invokeImmediately: true);

        _metricsButton = CreateButtonFromSource("BetterSortMetricButton", _containerRect, originalButton);
        _metricsButton.onClick.RemoveAllListeners();
        _metricsButton.onClick.AddListener(ToggleMenu);
        LayoutElement metricsLayout = RequireLayoutElement(_metricsButton.gameObject);
        metricsLayout.flexibleWidth = 0f;
        float metricsWidth = ComputeMetricsWidth(baseWidth, baseHeight, layout.spacing);
        metricsLayout.preferredWidth = metricsWidth;
        metricsLayout.minWidth = metricsWidth;
        RectTransform metricsRect = _metricsButton.GetComponent<RectTransform>();
        metricsRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, metricsWidth);
        metricsRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, baseHeight);
        metricsRect.SetSiblingIndex(0);

        // Use the original sort button as action (do not clone)
        originalRect.SetParent(_containerRect, false);
        originalRect.anchorMin = new Vector2(0f, 0f);
        originalRect.anchorMax = new Vector2(1f, 1f);
        originalRect.offsetMin = Vector2.zero;
        originalRect.offsetMax = Vector2.zero;
        originalRect.pivot = new Vector2(0.5f, 0.5f);
        originalRect.SetSiblingIndex(1);
        originalRect.gameObject.name = "BetterSortActionButton";
        _actionButton = originalButton;
        _actionButton.onClick.RemoveAllListeners();
        _actionButton.onClick.AddListener(RunVanillaSort);
        LayoutElement actionLayout = RequireLayoutElement(originalRect.gameObject);
        actionLayout.flexibleWidth = 0f;
        float spacing = layout.spacing;
        float minActionWidth = Mathf.Max(48f, baseHeight);
        float available = baseWidth - spacing - metricsWidth;
        float actionWidth = Mathf.Max(minActionWidth, available);
        if (available < minActionWidth)
        {
            metricsWidth = Mathf.Clamp(baseWidth - spacing - minActionWidth, 36f, metricsWidth);
            metricsLayout.preferredWidth = metricsWidth;
            metricsLayout.minWidth = metricsWidth;
            metricsRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, metricsWidth);
            available = baseWidth - spacing - metricsWidth;
            actionWidth = Mathf.Max(minActionWidth, available);
        }
        actionLayout.preferredWidth = actionWidth;
        actionLayout.minWidth = actionWidth;

        EnsureMetricsIcon();
        UpdateMetricsIcon(expanded: false);
    }


    private static float ComputeMetricsWidth(float baseWidth, float baseHeight, float spacing)
    {
        if (baseHeight <= 0f)
        {
            baseHeight = 52f;
        }
        if (baseWidth <= 0f)
        {
            baseWidth = 160f;
        }
        float minWidth = 36f;
        float metricsWidth = Mathf.Max(minWidth, baseHeight);
        float maxWidth = Mathf.Max(minWidth, baseWidth - spacing - 48f);
        if (metricsWidth > maxWidth)
        {
            metricsWidth = maxWidth;
        }
        return Mathf.Clamp(metricsWidth, minWidth, 96f);
    }




    private void EnsureOverlayAndMenu()
    {
        if (_overlayRect != null && _menuRect != null)
        {
            return;
        }
        Canvas sourceCanvas = _actionButton != null ? _actionButton.GetComponentInParent<Canvas>() : GetComponentInParent<Canvas>();
        var globalRoot = OverlayService.EnsureRoot();
        if (globalRoot == null)
        {
            Debug.LogWarning("BetterSortingMod: Unable to create overlay root.", this);
            return;
        }

        var created = OverlayService.CreateMenuLayer("BetterSortOverlay", HideMenu, new Color(0.08f, 0.08f, 0.08f, 0.9f));
        if (created.layer.Root == null || created.contentRect == null)
        {
            Debug.LogWarning("BetterSortingMod: OverlayService failed to create overlay layer.", this);
            return;
        }
        _overlayRect = created.layer.Root;
        _overlayComponent = created.layer.DismissOverlay;
        _menuRect = created.contentRect;
        LayoutElement menuLayoutElement = _menuRect.GetComponent<LayoutElement>();
        if (menuLayoutElement == null)
        {
            menuLayoutElement = _menuRect.gameObject.AddComponent<LayoutElement>();
        }
        menuLayoutElement.minWidth = Mathf.Max(menuLayoutElement.minWidth, 220f);
        // Ensure overlay starts hidden
        _overlayRect.gameObject.SetActive(false);
        menuLayoutElement.flexibleWidth = 0f;
    }

    private void EnsureMetricsIcon()
    {
        if (_metricsButton == null || _metricsIcon != null)
        {
            return;
        }
        RectTransform buttonRect = _metricsButton.GetComponent<RectTransform>();
        Transform existing = buttonRect.Find("BetterSortTriangle");
        Image icon;
        if (existing != null)
        {
            icon = existing.GetComponent<Image>();
            if (icon == null)
            {
                icon = existing.gameObject.AddComponent<Image>();
            }
        }
        else
        {
            GameObject iconObject = new GameObject("BetterSortTriangle", typeof(RectTransform), typeof(Image));
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.SetParent(buttonRect, false);
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            icon = iconObject.GetComponent<Image>();
            icon.sprite = GetTriangleSprite();
            icon.raycastTarget = false;
        }
        TextMeshProUGUI tmp = _metricsButton.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
        Color iconColor = Color.white;
        if (tmp != null)
        {
            iconColor = tmp.color;
            LocalizedLabelController controller = tmp.GetComponent<LocalizedLabelController>();
            if (controller != null)
            {
                Destroy(controller);
            }
            tmp.text = string.Empty;
        }
        RectTransform metricsRect = _metricsButton.GetComponent<RectTransform>();
        float iconSize = Mathf.Min(metricsRect.rect.width, metricsRect.rect.height);
        if (iconSize <= 0f)
        {
            iconSize = 36f;
        }
        iconSize = Mathf.Clamp(iconSize * 0.55f, 20f, 42f);
        RectTransform iconRectTransform = icon.rectTransform;
        iconRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, iconSize);
        iconRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, iconSize);
        icon.color = iconColor;
        _metricsIcon = icon;
    }

    private void UpdateMetricsIcon(bool expanded)
    {
        EnsureMetricsIcon();
        if (_metricsIcon == null)
        {
            return;
        }
        _metricsIcon.rectTransform.localRotation = Quaternion.Euler(0f, 0f, expanded ? -90f : 0f);
    }

    private static Sprite GetTriangleSprite()
    {
        if (triangleSprite != null)
        {
            return triangleSprite;
        }
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, mipChain: false);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color filled = Color.white;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                texture.SetPixel(x, y, clear);
            }
        }
        int center = size / 2;
        for (int x = 0; x < size; x++)
        {
            float progress = 1f - (float)x / (size - 1);
            int halfThickness = Mathf.Max(1, Mathf.RoundToInt(progress * (size - 1) * 0.5f));
            int minY = Mathf.Clamp(center - halfThickness, 0, size - 1);
            int maxY = Mathf.Clamp(center + halfThickness, 0, size - 1);
            for (int y = minY; y <= maxY; y++)
            {
                texture.SetPixel(x, y, filled);
            }
        }
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        triangleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        triangleSprite.name = "BetterSortingMod_Triangle";
        return triangleSprite;
    }

    private void BuildMenuOptions()
    {
        if (_menuRect == null)
        {
            return;
        }
        foreach (Button button in _menuButtons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }
        _menuButtons.Clear();

        var options = new List<MenuBuilder.SortOption>
        {
            new MenuBuilder.SortOption(LocalizedText.DefaultSort.Key, LocalizedText.DefaultSort.Fallback, () =>
            {
                HideMenu();
                RunVanillaSort();
            }),
            new MenuBuilder.SortOption(LocalizedText.WeightDescending.Key, LocalizedText.WeightDescending.Fallback, () =>
            {
                HideMenu();
                ApplyMetricSort("WeightDesc", WeightKey, true);
            }),
            new MenuBuilder.SortOption(LocalizedText.WeightAscending.Key, LocalizedText.WeightAscending.Fallback, () =>
            {
                HideMenu();
                ApplyMetricSort("WeightAsc", WeightKey, false);
            }),
            new MenuBuilder.SortOption(LocalizedText.ValueDescending.Key, LocalizedText.ValueDescending.Fallback, () =>
            {
                HideMenu();
                ApplyMetricSort("ValueDesc", StackValueKey, true);
            }),
            new MenuBuilder.SortOption(LocalizedText.ValueAscending.Key, LocalizedText.ValueAscending.Fallback, () =>
            {
                HideMenu();
                ApplyMetricSort("ValueAsc", StackValueKey, false);
            }),
            new MenuBuilder.SortOption(LocalizedText.ValuePerWeightDescending.Key, LocalizedText.ValuePerWeightDescending.Fallback, () =>
            {
                HideMenu();
                ApplyMetricSort("ValuePerWeightDesc", ValuePerWeightKey, true);
            }),
            new MenuBuilder.SortOption(LocalizedText.ValuePerWeightAscending.Key, LocalizedText.ValuePerWeightAscending.Fallback, () =>
            {
                HideMenu();
                ApplyMetricSort("ValuePerWeightAsc", ValuePerWeightKey, false);
            }),
            new MenuBuilder.SortOption(LocalizedText.MaxStackValueDescending.Key, LocalizedText.MaxStackValueDescending.Fallback, () =>
            {
                HideMenu();
                ApplyMetricSort("MaxStackValueDesc", MaxStackValueKey, true);
            }),
            new MenuBuilder.SortOption(LocalizedText.MaxStackValueAscending.Key, LocalizedText.MaxStackValueAscending.Fallback, () =>
            {
                HideMenu();
                ApplyMetricSort("MaxStackValueAsc", MaxStackValueKey, false);
            }),
        };

        var template = GetOrCreateOptionTemplate(_actionButton);
        _menuButtons.AddRange(MenuBuilder.BuildOptions(_menuRect, template, options));
        // Ensure menu buttons are hidden until menu is explicitly shown
        foreach (var button in _menuButtons)
        {
            if (button != null) button.gameObject.SetActive(false);
        }
    }



    private Button CreateButtonFromSource(string name, Transform parent, Button sourceButton)
    {
        if (sourceButton == null)
        {
            throw new ArgumentNullException(nameof(sourceButton));
        }

        Button template = GetOrCreateOptionTemplate(sourceButton);
        Button clone = Instantiate(template, parent);
        clone.gameObject.name = name;
        // Ensure the cloned button and its children are active (source template may be inactive)
        clone.gameObject.SetActive(true);
        for (int i = 0; i < clone.transform.childCount; i++)
        {
            var ch = clone.transform.GetChild(i);
            if (ch != null) ch.gameObject.SetActive(true);
        }
        RectTransform rect = clone.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.anchoredPosition3D = Vector3.zero;
        // Stretch horizontally within menu so width is never zero
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Ensure any CanvasGroup on the clone is visible and interactive
        var cg = clone.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }



        TextMeshProUGUI label = clone.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);

        if (label != null)
        {
            label.text = string.Empty;
            label.raycastTarget = false;
            var c = label.color;
            c.a = 1f;
            label.color = c;
        }

        return clone;
    }

    private void ToggleMenu()
    {
        if (_overlayRect == null)
        {
            return;
        }
        if (_overlayRect.gameObject.activeSelf)
        {
            HideMenu();
        }
        else
        {
            ShowMenu();
        }
    }

    private void ShowMenu()
    {
        if (_overlayRect == null || _menuRect == null || _metricsButton == null)
        {
            return;
        }
        _overlayRect.gameObject.SetActive(true);
        _overlayRect.SetAsLastSibling();

        // Ensure all menu children are active before layout
        for (int i = 0; i < _menuRect.childCount; i++)
        {
            Transform child = _menuRect.GetChild(i);
            if (child != null) child.gameObject.SetActive(true);
            for (int j = 0; child != null && j < child.childCount; j++)
            {
                Transform grand = child.GetChild(j);
                if (grand != null) grand.gameObject.SetActive(true);
            }
        }

        // Single layout pass before positioning
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_menuRect);

        UpdateMenuPosition();

        // Final clamp using the source canvas
        ClampMenuWithinCanvas(_metricsButton.GetComponentInParent<Canvas>());

        UpdateMetricsIcon(expanded: true);
    }

    private void HideMenu()
    {
        if (_overlayRect == null)
        {
            return;
        }
        _overlayRect.gameObject.SetActive(false);
        UpdateMetricsIcon(expanded: false);
    }

    private void UpdateMenuPosition()
    {
        if (_metricsButton == null || _menuRect == null || _overlayRect == null)
        {
            return;
        }
        RectTransform metricsRect = _metricsButton.GetComponent<RectTransform>();
        if (metricsRect == null)
        {
            return;
        }
        Canvas sourceCanvas = _metricsButton.GetComponentInParent<Canvas>();
        if (sourceCanvas == null)
        {
            return;
        }
        var globalRoot = EnsureGlobalOverlayRoot();
        if (_overlayRect != null && globalRoot != null && _overlayRect.parent as RectTransform != globalRoot)
        {
            _overlayRect.SetParent(globalRoot, false);
        }
        Canvas targetCanvas = sourceCanvas != null ? sourceCanvas.rootCanvas : null;
        Camera sourceCamera = sourceCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : sourceCanvas.worldCamera;
        RectTransform triRect = metricsRect;
        triRect.GetWorldCorners(WorldCornersBuffer);
        Vector3 bottomLeft = WorldCornersBuffer[0];
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(sourceCamera, bottomLeft);
        RectTransform targetRect = _overlayRect;
        Canvas overlayCanvas = _overlayRect != null ? _overlayRect.GetComponentInParent<Canvas>() : null;
        Camera targetCam = overlayCanvas != null && overlayCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : overlayCanvas?.worldCamera;
        if (targetRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, screenPoint, targetCam, out Vector2 localPos))
        {
            _menuRect.anchorMin = new Vector2(0f, 1f);
            _menuRect.anchorMax = new Vector2(0f, 1f);
            _menuRect.pivot = new Vector2(0f, 1f);
            // Place menu so its top-left aligns to triangle bottom-left without hardcoded offsets
            Vector3 menuWorld;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(targetRect, screenPoint, targetCam, out menuWorld))
            {
                _menuRect.position = menuWorld;
                // Small downward offset for spacing
                _menuRect.anchoredPosition += new Vector2(0f, -6f);
            }
        }
        ClampMenuWithinCanvas(overlayCanvas ?? targetCanvas);
    }

    private void ClampMenuWithinCanvas(Canvas canvas)
    {
        if (_menuRect == null || canvas == null)
        {
            return;
        }
        RectTransform canvasRect = _overlayRect;
        if (canvasRect == null)
        {
            return;
        }
        _menuRect.GetWorldCorners(WorldCornersBuffer);
        Vector3 worldMin = WorldCornersBuffer[0];
        Vector3 worldMax = WorldCornersBuffer[2];
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, worldMin);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(cam, worldMax);
        Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
        Vector3 adjustment = Vector3.zero;
        if (min.x < screenRect.xMin)
        {
            adjustment.x += screenRect.xMin - min.x;
        }
        if (max.x > screenRect.xMax)
        {
            adjustment.x -= max.x - screenRect.xMax;
        }
        if (min.y < screenRect.yMin)
        {
            adjustment.y += screenRect.yMin - min.y;
        }
        if (max.y > screenRect.yMax)
        {
            adjustment.y -= max.y - screenRect.yMax;
        }
        if (adjustment != Vector3.zero)
        {
            Vector2 menuScreen = RectTransformUtility.WorldToScreenPoint(cam, _menuRect.position);
            menuScreen += new Vector2(adjustment.x, adjustment.y);
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, menuScreen, cam, out Vector3 finalWorld))
            {
                _menuRect.position = finalWorld;
            }
        }
    }

    private void RunVanillaSort()
    {
        if (_display == null)
        {
            return;
        }
        Inventory target = _display.Target;
        if (!_display.Editable || target == null || target.Loading)
        {
            return;
        }
        target.Sort();
    }

    private void ApplyMetricSort(string label, Func<Item, double> selector, bool descending)
    {
        if (_display == null)
        {
            return;
        }
        Inventory inventory = _display.Target;
        if (inventory == null || inventory.Loading)
        {
            return;
        }
        HashSet<int> locked = new HashSet<int>(inventory.lockedIndexes);
        List<int> unlockedIndices = new List<int>();
        List<UnlockedEntry> entries = new List<UnlockedEntry>();
        int capacity = inventory.Capacity;
        for (int i = 0; i < capacity; i++)
        {
            if (locked.Contains(i))
            {
                continue;
            }
            Item item = inventory.GetItemAt(i);
            unlockedIndices.Add(i);
            entries.Add(new UnlockedEntry(item, i));
        }
        if (entries.Count == 0)
        {
            return;
        }
        entries.Sort((a, b) => CompareEntries(a, b, selector, descending));
        ApplySortedEntries(inventory, unlockedIndices, entries);
    }

    private void ApplySortedEntries(Inventory inventory, List<int> unlockedIndices, List<UnlockedEntry> sortedEntries)
    {
        for (int i = unlockedIndices.Count - 1; i >= 0; i--)
        {
            int index = unlockedIndices[i];
            Item current = inventory.GetItemAt(index);
            if (current == null)
            {
                continue;
            }
            inventory.RemoveAt(index, out _);
        }
        for (int i = 0; i < unlockedIndices.Count && i < sortedEntries.Count; i++)
        {
            Item item = sortedEntries[i].Item;
            int targetIndex = unlockedIndices[i];
            if (item == null)
            {
                continue;
            }
            inventory.AddAt(item, targetIndex);
        }
    }

    private void SyncVisibility()
    {
        if (_containerRect == null || _actionButton == null)
        {
            return;
        }
        bool visible = _containerRect.parent != null && _containerRect.parent.gameObject.activeInHierarchy;
        if (_containerRect.gameObject.activeSelf != visible)
        {
            _containerRect.gameObject.SetActive(visible);
        }
        if (_metricsButton != null) _metricsButton.gameObject.SetActive(visible);
        if (_actionButton != null) _actionButton.gameObject.SetActive(visible);
        if (!visible)
        {
            HideMenu();
        }
    }

    private static void CopyRectTransform(RectTransform source, RectTransform destination)
    {
        destination.anchorMin = source.anchorMin;
        destination.anchorMax = source.anchorMax;
        destination.pivot = source.pivot;
        destination.sizeDelta = source.sizeDelta;
        destination.anchoredPosition = source.anchoredPosition;
        destination.localRotation = source.localRotation;
        destination.localScale = source.localScale;
    }

    private static void CopyLayoutElement(RectTransform source, RectTransform destination)
    {
        LayoutElement sourceLayout = source.GetComponent<LayoutElement>();
        if (sourceLayout == null)
        {
            return;
        }
        LayoutElement targetLayout = destination.gameObject.AddComponent<LayoutElement>();
        targetLayout.ignoreLayout = sourceLayout.ignoreLayout;
        targetLayout.preferredWidth = sourceLayout.preferredWidth;
        targetLayout.preferredHeight = sourceLayout.preferredHeight;
        targetLayout.minWidth = sourceLayout.minWidth;
        targetLayout.minHeight = sourceLayout.minHeight;
        targetLayout.flexibleWidth = sourceLayout.flexibleWidth;
        targetLayout.flexibleHeight = sourceLayout.flexibleHeight;
        targetLayout.layoutPriority = sourceLayout.layoutPriority;
    }

    private static LayoutElement RequireLayoutElement(GameObject obj)
    {
        LayoutElement layout = obj.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = obj.AddComponent<LayoutElement>();
        }
        return layout;
    }



    private static double WeightKey(Item item)
    {
        return item?.TotalWeight ?? double.MinValue;
    }

    private static double StackValueKey(Item item)
    {
        if (item == null)
        {
            return double.MinValue;
        }
        return (double)item.Value * item.StackCount;
    }

    private static double ValuePerWeightKey(Item item)
    {
        if (item == null)
        {
            return double.MinValue;
        }
        double weight = Math.Max(0.01, item.TotalWeight);
        return ((double)item.Value * item.StackCount) / weight;
    }

    private static double MaxStackValueKey(Item item)
    {
        if (item == null)
        {
            return double.MinValue;
        }
        return (double)item.Value * Math.Max(1, item.MaxStackCount);
    }

    private static int CompareEntries(UnlockedEntry a, UnlockedEntry b, Func<Item, double> selector, bool descending)
    {
        bool aNull = a.Item == null;
        bool bNull = b.Item == null;
        if (aNull && bNull)
        {
            return a.OriginalIndex.CompareTo(b.OriginalIndex);
        }
        if (aNull)
        {
            return 1;
        }
        if (bNull)
        {
            return -1;
        }
        double valueA = selector(a.Item);
        double valueB = selector(b.Item);
        if (double.IsNaN(valueA))
        {
            valueA = 0d;
        }
        if (double.IsNaN(valueB))
        {
            valueB = 0d;
        }
        int comparison = valueA.CompareTo(valueB);
        if (descending)
        {
            comparison = -comparison;
        }
        if (comparison != 0)
        {
            return comparison;
        }
        return a.OriginalIndex.CompareTo(b.OriginalIndex);
    }

    private readonly struct UnlockedEntry
    {
        public UnlockedEntry(Item item, int originalIndex)
        {
            Item = item;
            OriginalIndex = originalIndex;
        }

        public Item Item { get; }

        public int OriginalIndex { get; }
    }

    private readonly struct LocalizedText
    {
        private readonly Dictionary<SystemLanguage, string> fallback;

        public LocalizedText(string key, string chineseSimplified, string english)
        {
            Key = key;
            fallback = new Dictionary<SystemLanguage, string>();
            if (!string.IsNullOrEmpty(chineseSimplified))
            {
                fallback[SystemLanguage.ChineseSimplified] = chineseSimplified;
                fallback[SystemLanguage.Chinese] = chineseSimplified;
                fallback[SystemLanguage.ChineseTraditional] = chineseSimplified;
            }
            if (!string.IsNullOrEmpty(english))
            {
                fallback[SystemLanguage.English] = english;
            }
        }

        public string Key { get; }

        public IReadOnlyDictionary<SystemLanguage, string> Fallback => fallback;

        public static LocalizedText DefaultSort => new LocalizedText("default_sort", "默认整理", "Default Sort");

        public static LocalizedText WeightDescending => new LocalizedText("weight_desc", "重量 ↓", "Weight ↓");

        public static LocalizedText WeightAscending => new LocalizedText("weight_asc", "重量 ↑", "Weight ↑");

        public static LocalizedText ValueDescending => new LocalizedText("value_desc", "价格 ↓", "Value ↓");

        public static LocalizedText ValueAscending => new LocalizedText("value_asc", "价格 ↑", "Value ↑");

        public static LocalizedText ValuePerWeightDescending => new LocalizedText("value_per_weight_desc", "价格/重量 ↓", "Value / Weight ↓");

        public static LocalizedText ValuePerWeightAscending => new LocalizedText("value_per_weight_asc", "价格/重量 ↑", "Value / Weight ↑");

        public static LocalizedText MaxStackValueDescending => new LocalizedText("max_stack_value_desc", "最大堆叠价值 ↓", "Max Stack Value ↓");

        public static LocalizedText MaxStackValueAscending => new LocalizedText("max_stack_value_asc", "最大堆叠价值 ↑", "Max Stack Value ↑");
    }
}
