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
    private RectTransform _containerRect;
    private RectTransform _menuRect;
    private RectTransform _overlayRect;
    private SortMenuOverlay _overlayComponent;
    private Canvas _rootCanvas;
    private bool _uiInitialized;

    private static readonly Vector3[] WorldCornersBuffer = new Vector3[4];

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

    private void Update()
    {
        SyncVisibility();
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
        GameObject containerObject = new GameObject("BetterSortButtonContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        _containerRect = containerObject.GetComponent<RectTransform>();
        _containerRect.SetParent(parentRect, worldPositionStays: false);
        CopyRectTransform(originalRect, _containerRect);
        _containerRect.SetSiblingIndex(siblingIndex);
        HorizontalLayoutGroup layout = containerObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = true;
        ContentSizeFitter fitter = containerObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        CopyLayoutElement(originalRect, _containerRect);

        _metricsButton = CreateButtonFromSource("BetterSortMetricButton", _containerRect, originalButton);
        SetButtonLabel(_metricsButton, ">");
        _metricsButton.onClick.RemoveAllListeners();
        _metricsButton.onClick.AddListener(ToggleMenu);
        LayoutElement metricsLayout = RequireLayoutElement(_metricsButton.gameObject);
        metricsLayout.flexibleWidth = 0f;
        metricsLayout.preferredWidth = Mathf.Max(metricsLayout.preferredWidth, 56f);
        metricsLayout.minWidth = Mathf.Max(metricsLayout.minWidth, 48f);
        RectTransform metricsRect = _metricsButton.GetComponent<RectTransform>();
        metricsRect.SetSiblingIndex(0);

        originalRect.SetParent(_containerRect, worldPositionStays: false);
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
        actionLayout.flexibleWidth = Mathf.Max(actionLayout.flexibleWidth, 1f);
    }

    private void EnsureOverlayAndMenu()
    {
        if (_overlayRect != null && _menuRect != null)
        {
            return;
        }
        _rootCanvas = GetComponentInParent<Canvas>();
        if (_rootCanvas == null)
        {
            Debug.LogWarning("BetterSortingMod: Unable to find parent canvas for inventory display.", this);
            return;
        }

        GameObject overlayObject = new GameObject("BetterSortOverlay", typeof(RectTransform), typeof(Image), typeof(SortMenuOverlay));
        _overlayRect = overlayObject.GetComponent<RectTransform>();
        _overlayRect.SetParent(_rootCanvas.transform, worldPositionStays: false);
        _overlayRect.anchorMin = Vector2.zero;
        _overlayRect.anchorMax = Vector2.one;
        _overlayRect.offsetMin = Vector2.zero;
        _overlayRect.offsetMax = Vector2.zero;
        _overlayRect.pivot = new Vector2(0.5f, 0.5f);
        Image overlayImage = overlayObject.GetComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0f);
        overlayImage.raycastTarget = true;
        overlayObject.SetActive(false);
        _overlayComponent = overlayObject.GetComponent<SortMenuOverlay>();
        _overlayComponent.Initialize(HideMenu);

        GameObject menuObject = new GameObject("BetterSortMenu", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        _menuRect = menuObject.GetComponent<RectTransform>();
        _menuRect.SetParent(_overlayRect, worldPositionStays: false);
        _menuRect.anchorMin = new Vector2(0f, 1f);
        _menuRect.anchorMax = new Vector2(0f, 1f);
        _menuRect.pivot = new Vector2(0f, 1f);
        _menuRect.anchoredPosition = Vector2.zero;
        Image menuBackground = menuObject.GetComponent<Image>();
        menuBackground.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);
        menuBackground.raycastTarget = true;
        VerticalLayoutGroup menuLayout = menuObject.GetComponent<VerticalLayoutGroup>();
        menuLayout.spacing = 4f;
        menuLayout.padding = new RectOffset(10, 10, 8, 8);
        menuLayout.childAlignment = TextAnchor.UpperLeft;
        menuLayout.childControlWidth = true;
        menuLayout.childControlHeight = true;
        menuLayout.childForceExpandWidth = true;
        menuLayout.childForceExpandHeight = false;
        ContentSizeFitter menuFitter = menuObject.GetComponent<ContentSizeFitter>();
        menuFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        menuFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
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

        AddMenuOption("默认整理", () =>
        {
            HideMenu();
            RunVanillaSort();
        });

        AddMenuOption("重量 ↓", () =>
        {
            HideMenu();
            ApplyMetricSort("WeightDesc", WeightKey, descending: true);
        });
        AddMenuOption("重量 ↑", () =>
        {
            HideMenu();
            ApplyMetricSort("WeightAsc", WeightKey, descending: false);
        });

        AddMenuOption("价格 ↓", () =>
        {
            HideMenu();
            ApplyMetricSort("ValueDesc", StackValueKey, descending: true);
        });
        AddMenuOption("价格 ↑", () =>
        {
            HideMenu();
            ApplyMetricSort("ValueAsc", StackValueKey, descending: false);
        });

        AddMenuOption("价格/重量 ↓", () =>
        {
            HideMenu();
            ApplyMetricSort("ValuePerWeightDesc", ValuePerWeightKey, descending: true);
        });
        AddMenuOption("价格/重量 ↑", () =>
        {
            HideMenu();
            ApplyMetricSort("ValuePerWeightAsc", ValuePerWeightKey, descending: false);
        });

        AddMenuOption("最大堆叠价值 ↓", () =>
        {
            HideMenu();
            ApplyMetricSort("MaxStackValueDesc", MaxStackValueKey, descending: true);
        });
        AddMenuOption("最大堆叠价值 ↑", () =>
        {
            HideMenu();
            ApplyMetricSort("MaxStackValueAsc", MaxStackValueKey, descending: false);
        });
    }

    private void AddMenuOption(string label, Action onClick)
    {
        if (_menuRect == null || _actionButton == null)
        {
            return;
        }
        Button optionButton = CreateButtonFromSource($"BetterSortOption_{label}", _menuRect, _actionButton);
        optionButton.onClick.RemoveAllListeners();
        optionButton.onClick.AddListener(() => onClick());
        SetButtonLabel(optionButton, label);
        LayoutElement optionLayout = RequireLayoutElement(optionButton.gameObject);
        optionLayout.flexibleWidth = 1f;
        optionLayout.minWidth = Mathf.Max(optionLayout.minWidth, 220f);
        optionLayout.preferredWidth = Mathf.Max(optionLayout.preferredWidth, 220f);
        _menuButtons.Add(optionButton);
    }

    private Button CreateButtonFromSource(string name, Transform parent, Button sourceButton)
    {
        if (sourceButton == null)
        {
            throw new ArgumentNullException(nameof(sourceButton));
        }

        Button clone = Instantiate(sourceButton, parent);
        clone.gameObject.name = name;
        RectTransform rect = clone.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.anchoredPosition3D = Vector3.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        foreach (MonoBehaviour localizer in clone.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
        {
            if (localizer == null)
            {
                continue;
            }
            Type type = localizer.GetType();
            if (type.FullName == "SodaCraft.Localizations.TextLocalizor")
            {
                Destroy(localizer);
            }
        }

        TextMeshProUGUI label = clone.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
        if (label != null)
        {
            label.text = string.Empty;
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
        UpdateMenuPosition();
        SetButtonLabel(_metricsButton, "v");
    }

    private void HideMenu()
    {
        if (_overlayRect == null)
        {
            return;
        }
        _overlayRect.gameObject.SetActive(false);
        if (_metricsButton != null)
        {
            SetButtonLabel(_metricsButton, ">");
        }
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
        Canvas canvas = _rootCanvas;
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }
            _rootCanvas = canvas;
        }
        metricsRect.GetWorldCorners(WorldCornersBuffer);
        Vector3 bottomLeft = WorldCornersBuffer[0];
        Vector3 offset = new Vector3(0f, -4f, 0f);
        _menuRect.position = bottomLeft + offset;
        ClampMenuWithinCanvas(canvas);
    }

    private void ClampMenuWithinCanvas(Canvas canvas)
    {
        if (_menuRect == null || canvas == null)
        {
            return;
        }
        RectTransform canvasRect = canvas.transform as RectTransform;
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
        bool visible = _actionButton.gameObject.activeSelf;
        if (_containerRect.gameObject.activeSelf != visible)
        {
            _containerRect.gameObject.SetActive(visible);
        }
        if (_metricsButton != null && _metricsButton.gameObject.activeSelf != visible)
        {
            _metricsButton.gameObject.SetActive(visible);
        }
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

    private static void SetButtonLabel(Button button, string text)
    {
        if (button == null)
        {
            return;
        }
        TextMeshProUGUI tmp = button.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = text;
        }
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
}
