using System;
using Duckov.UI.Animations;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Duckov.UI;

public class ItemHoveringUI : MonoBehaviour
{
	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private RectTransform layoutParent;

	[SerializeField]
	private RectTransform contents;

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private TextMeshProUGUI itemName;

	[SerializeField]
	private TextMeshProUGUI weightDisplay;

	[SerializeField]
	private TextMeshProUGUI itemDescription;

	[SerializeField]
	private TextMeshProUGUI itemID;

	[SerializeField]
	private ItemPropertiesDisplay itemProperties;

	[SerializeField]
	private BulletTypeDisplay bulletTypeDisplay;

	[SerializeField]
	private UsageUtilitiesDisplay usageUtilitiesDisplay;

	[SerializeField]
	private GameObject interactionIndicatorsContainer;

	[SerializeField]
	private GameObject interactionIndicator_Move;

	[SerializeField]
	private GameObject interactionIndicator_Menu;

	[SerializeField]
	private GameObject interactionIndicator_Drop;

	[SerializeField]
	private GameObject interactionIndicator_Use;

	[SerializeField]
	private GameObject interactionIndicator_Split;

	[SerializeField]
	private GameObject interactionIndicator_LockSort;

	[SerializeField]
	private GameObject interactionIndicator_Shortcut;

	[SerializeField]
	private GameObject wishlistInfoParent;

	[SerializeField]
	private GameObject wishlistIndicator;

	[SerializeField]
	private GameObject buildingIndicator;

	[SerializeField]
	private GameObject questIndicator;

	[SerializeField]
	private GameObject registeredIndicator;

	private MonoBehaviour target;

	public static ItemHoveringUI Instance { get; private set; }

	public RectTransform LayoutParent => layoutParent;

	public static int DisplayingItemID { get; private set; }

	public static bool Shown
	{
		get
		{
			if (Instance == null)
			{
				return false;
			}
			return Instance.fadeGroup.IsShown;
		}
	}

	public static event Action<ItemHoveringUI, ItemMetaData> onSetupMeta;

	public static event Action<ItemHoveringUI, Item> onSetupItem;

	private void Awake()
	{
		Instance = this;
		if (rectTransform == null)
		{
			rectTransform = GetComponent<RectTransform>();
		}
		ItemDisplay.OnPointerEnterItemDisplay += OnPointerEnterItemDisplay;
		ItemDisplay.OnPointerExitItemDisplay += OnPointerExitItemDisplay;
		ItemAmountDisplay.OnMouseEnter += OnMouseEnterItemAmountDisplay;
		ItemAmountDisplay.OnMouseExit += OnMouseExitItemAmountDisplay;
		ItemMetaDisplay.OnMouseEnter += OnMouseEnterMetaDisplay;
		ItemMetaDisplay.OnMouseExit += OnMouseExitMetaDisplay;
	}

	private void OnDestroy()
	{
		ItemDisplay.OnPointerEnterItemDisplay -= OnPointerEnterItemDisplay;
		ItemDisplay.OnPointerExitItemDisplay -= OnPointerExitItemDisplay;
		ItemAmountDisplay.OnMouseEnter -= OnMouseEnterItemAmountDisplay;
		ItemAmountDisplay.OnMouseExit -= OnMouseExitItemAmountDisplay;
		ItemMetaDisplay.OnMouseEnter -= OnMouseEnterMetaDisplay;
		ItemMetaDisplay.OnMouseExit -= OnMouseExitMetaDisplay;
	}

	private void OnMouseExitMetaDisplay(ItemMetaDisplay display)
	{
		if (target == display)
		{
			Hide();
		}
	}

	private void OnMouseEnterMetaDisplay(ItemMetaDisplay display)
	{
		SetupAndShowMeta(display);
	}

	private void OnMouseExitItemAmountDisplay(ItemAmountDisplay display)
	{
		if (target == display)
		{
			Hide();
		}
	}

	private void OnMouseEnterItemAmountDisplay(ItemAmountDisplay display)
	{
		SetupAndShowMeta(display);
	}

	private void OnPointerExitItemDisplay(ItemDisplay display)
	{
		if (target == display)
		{
			Hide();
		}
	}

	private void OnPointerEnterItemDisplay(ItemDisplay display)
	{
		SetupAndShow(display);
	}

	private void SetupAndShow(ItemDisplay display)
	{
		if (display == null)
		{
			return;
		}
		Item item = display.Target;
		if (!(item == null) && !item.NeedInspection)
		{
			registeredIndicator.SetActive(value: false);
			target = display;
			itemName.text = item.DisplayName ?? "";
			itemDescription.text = item.Description ?? "";
			weightDisplay.gameObject.SetActive(value: true);
			weightDisplay.text = $"{item.TotalWeight:0.#} kg";
			itemID.text = $"#{item.TypeID}";
			DisplayingItemID = item.TypeID;
			itemProperties.gameObject.SetActive(value: true);
			itemProperties.Setup(item);
			interactionIndicatorsContainer.SetActive(value: true);
			interactionIndicator_Menu.SetActive(display.ShowOperationButtons);
			interactionIndicator_Move.SetActive(display.Movable);
			interactionIndicator_Drop.SetActive(display.CanDrop);
			interactionIndicator_Use.SetActive(display.CanUse);
			interactionIndicator_Split.SetActive(display.CanSplit);
			interactionIndicator_LockSort.SetActive(display.CanLockSort);
			interactionIndicator_Shortcut.SetActive(display.CanSetShortcut);
			usageUtilitiesDisplay.Setup(item);
			SetupWishlistInfos(item.TypeID);
			SetupBulletDisplay();
			try
			{
				ItemHoveringUI.onSetupItem?.Invoke(this, item);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			RefreshPosition();
			SetupRegisteredInfo(item);
			fadeGroup.Show();
		}
	}

	private void SetupRegisteredInfo(Item item)
	{
		if (!(item == null) && item.IsRegistered())
		{
			registeredIndicator.SetActive(value: true);
		}
	}

	private void SetupAndShowMeta<T>(T dataProvider) where T : MonoBehaviour, IItemMetaDataProvider
	{
		if (!(dataProvider == null))
		{
			registeredIndicator.SetActive(value: false);
			target = dataProvider;
			ItemMetaData metaData = dataProvider.GetMetaData();
			itemName.text = metaData.DisplayName;
			itemID.text = $"{metaData.id}";
			DisplayingItemID = metaData.id;
			itemDescription.text = metaData.Description;
			interactionIndicatorsContainer.SetActive(value: true);
			weightDisplay.gameObject.SetActive(value: false);
			bulletTypeDisplay.gameObject.SetActive(value: false);
			itemProperties.gameObject.SetActive(value: false);
			interactionIndicator_Menu.gameObject.SetActive(value: false);
			interactionIndicator_Move.gameObject.SetActive(value: false);
			interactionIndicator_Drop.gameObject.SetActive(value: false);
			interactionIndicator_Use.gameObject.SetActive(value: false);
			usageUtilitiesDisplay.gameObject.SetActive(value: false);
			interactionIndicator_Split.SetActive(value: false);
			interactionIndicator_Shortcut.SetActive(value: false);
			SetupWishlistInfos(metaData.id);
			try
			{
				ItemHoveringUI.onSetupMeta?.Invoke(this, metaData);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			RefreshPosition();
			fadeGroup.Show();
		}
	}

	private void SetupBulletDisplay()
	{
		ItemDisplay itemDisplay = target as ItemDisplay;
		if (!(itemDisplay == null))
		{
			ItemSetting_Gun component = itemDisplay.Target.GetComponent<ItemSetting_Gun>();
			if (component == null)
			{
				bulletTypeDisplay.gameObject.SetActive(value: false);
				return;
			}
			bulletTypeDisplay.gameObject.SetActive(value: true);
			bulletTypeDisplay.Setup(component.TargetBulletID);
		}
	}

	private void RefreshPosition()
	{
		Vector2 value = Mouse.current.position.value;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, value, null, out var localPoint);
		float xMax = contents.rect.xMax;
		float yMin = contents.rect.yMin;
		float b = rectTransform.rect.xMax - xMax;
		float b2 = rectTransform.rect.yMin - yMin;
		localPoint.x = Mathf.Min(localPoint.x, b);
		localPoint.y = Mathf.Max(localPoint.y, b2);
		contents.anchoredPosition = localPoint;
	}

	private void Hide()
	{
		fadeGroup.Hide();
		DisplayingItemID = -1;
	}

	private void Update()
	{
		if (fadeGroup.IsShown)
		{
			if (target == null || !target.isActiveAndEnabled)
			{
				Hide();
			}
			if (target is ItemDisplay itemDisplay && itemDisplay.Target == null)
			{
				Hide();
			}
		}
		RefreshPosition();
	}

	private void SetupWishlistInfos(int itemTypeID)
	{
		ItemWishlist.WishlistInfo wishlistInfo = ItemWishlist.GetWishlistInfo(itemTypeID);
		bool isManuallyWishlisted = wishlistInfo.isManuallyWishlisted;
		bool isBuildingRequired = wishlistInfo.isBuildingRequired;
		bool isQuestRequired = wishlistInfo.isQuestRequired;
		bool active = isManuallyWishlisted || isBuildingRequired || isQuestRequired;
		wishlistIndicator.SetActive(isManuallyWishlisted);
		buildingIndicator.SetActive(isBuildingRequired);
		questIndicator.SetActive(isQuestRequired);
		wishlistInfoParent.SetActive(active);
	}

	internal static void NotifyRefreshWishlistInfo()
	{
		if (!(Instance == null))
		{
			Instance.SetupWishlistInfos(DisplayingItemID);
		}
	}
}
