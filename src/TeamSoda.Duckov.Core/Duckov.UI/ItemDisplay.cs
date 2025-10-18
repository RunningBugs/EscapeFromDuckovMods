using System;
using DG.Tweening;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using LeTai.TrueShadow;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI;

public class ItemDisplay : MonoBehaviour, IPoolable, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TrueShadow displayQualityShadow;

	[SerializeField]
	private GameObject countGameObject;

	[SerializeField]
	private TextMeshProUGUI countText;

	[SerializeField]
	private GameObject selectionIndicator;

	[SerializeField]
	private Graphic interactionEventReceiver;

	[SerializeField]
	private GameObject backgroundRing;

	[SerializeField]
	private GameObject inspectionElementRoot;

	[SerializeField]
	private GameObject inspectingElement;

	[SerializeField]
	private GameObject notInspectingElement;

	[SerializeField]
	private GameObject nameContainer;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private GameObject durabilityGameObject;

	[SerializeField]
	private Image durabilityFill;

	[SerializeField]
	private Gradient durabilityFillColorOverT;

	[SerializeField]
	private GameObject durabilityZeroIndicator;

	[SerializeField]
	private Image durabilityLoss;

	[SerializeField]
	private GameObject slotIndicatorContainer;

	[SerializeField]
	private SlotIndicator slotIndicatorTemplate;

	[SerializeField]
	private GameObject wishlistedIndicator;

	[SerializeField]
	private GameObject questRequiredIndicator;

	[SerializeField]
	private GameObject buildingRequiredIndicator;

	[SerializeField]
	[Range(0f, 1f)]
	private float punchDuration = 0.2f;

	[SerializeField]
	[Range(-1f, 1f)]
	private float selectionRingPunchScale = 0.1f;

	[SerializeField]
	[Range(-1f, 1f)]
	private float backgroundRingPunchScale = 0.2f;

	[SerializeField]
	[Range(-1f, 1f)]
	private float iconPunchScale = 0.1f;

	public const float doubleClickTimeThreshold = 0.3f;

	private PrefabPool<SlotIndicator> _slotIndicatorPool;

	private bool mainContentShown = true;

	private bool isBeingDestroyed;

	[SerializeField]
	private bool showOperationButtons = true;

	private float lastClickTime;

	private bool doubleClickInvoked;

	private Sprite FallbackIcon => GameplayDataSettings.UIStyle.FallbackItemIcon;

	public Item Target { get; private set; }

	internal Action releaseAction { get; set; }

	public bool Selected => ItemUIUtilities.SelectedItemDisplay == this;

	private PrefabPool<SlotIndicator> SlotIndicatorPool
	{
		get
		{
			if (_slotIndicatorPool == null)
			{
				if (slotIndicatorTemplate == null)
				{
					Debug.LogError("SI is null", base.gameObject);
				}
				_slotIndicatorPool = new PrefabPool<SlotIndicator>(slotIndicatorTemplate);
			}
			return _slotIndicatorPool;
		}
	}

	public static PrefabPool<ItemDisplay> Pool => GameplayUIManager.Instance.ItemDisplayPool;

	public bool ShowOperationButtons
	{
		get
		{
			return showOperationButtons;
		}
		internal set
		{
			showOperationButtons = value;
		}
	}

	public bool Editable { get; set; }

	public bool Movable { get; set; }

	public bool CanDrop { get; set; }

	public bool IsStockshopSample { get; set; }

	public bool CanUse
	{
		get
		{
			if (Target == null)
			{
				return false;
			}
			if (!Editable)
			{
				return false;
			}
			if (Target.IsUsable(CharacterMainControl.Main))
			{
				return true;
			}
			return false;
		}
	}

	public bool CanSplit
	{
		get
		{
			if (Target == null)
			{
				return false;
			}
			if (!Editable)
			{
				return false;
			}
			if (Movable && Target.StackCount > 1)
			{
				return true;
			}
			return false;
		}
	}

	public bool CanLockSort { get; internal set; }

	public bool CanSetShortcut
	{
		get
		{
			if (Target == null)
			{
				return false;
			}
			if (!showOperationButtons)
			{
				return false;
			}
			if (!ItemShortcut.IsItemValid(Target))
			{
				return false;
			}
			return true;
		}
	}

	internal event Action<ItemDisplay, PointerEventData> onDoubleClicked;

	public event Action<PointerEventData> onReceiveDrop;

	public static event Action<ItemDisplay> OnPointerEnterItemDisplay;

	public static event Action<ItemDisplay> OnPointerExitItemDisplay;

	public event Action<ItemDisplay, PointerEventData> onPointerClick;

	public void Setup(Item target)
	{
		UnregisterEvents();
		Target = target;
		Clear();
		slotIndicatorTemplate.gameObject.SetActive(value: false);
		if (target == null)
		{
			SetupEmpty();
		}
		else
		{
			icon.color = Color.white;
			icon.sprite = target.Icon;
			if (icon.sprite == null)
			{
				icon.sprite = FallbackIcon;
			}
			icon.gameObject.SetActive(value: true);
			(float, Color, bool) shadowOffsetAndColorOfQuality = GameplayDataSettings.UIStyle.GetShadowOffsetAndColorOfQuality(target.DisplayQuality);
			displayQualityShadow.OffsetDistance = shadowOffsetAndColorOfQuality.Item1;
			displayQualityShadow.Color = shadowOffsetAndColorOfQuality.Item2;
			displayQualityShadow.Inset = shadowOffsetAndColorOfQuality.Item3;
			bool stackable = Target.Stackable;
			countGameObject.SetActive(stackable);
			nameText.text = Target.DisplayName;
			if (target.Slots != null)
			{
				foreach (Slot slot in target.Slots)
				{
					SlotIndicatorPool.Get().Setup(slot);
				}
			}
		}
		Refresh();
		if (base.isActiveAndEnabled)
		{
			RegisterEvents();
		}
	}

	private void RegisterEvents()
	{
		UnregisterEvents();
		ItemUIUtilities.OnSelectionChanged += OnItemUtilitiesSelectionChanged;
		ItemWishlist.OnWishlistChanged += OnWishlistChanged;
		if (!(Target == null))
		{
			Target.onDestroy += OnTargetDestroy;
			Target.onSetStackCount += OnTargetSetStackCount;
			Target.onInspectionStateChanged += OnTargetInspectionStateChanged;
			Target.onDurabilityChanged += OnTargetDurabilityChanged;
		}
	}

	private void UnregisterEvents()
	{
		ItemUIUtilities.OnSelectionChanged -= OnItemUtilitiesSelectionChanged;
		ItemWishlist.OnWishlistChanged -= OnWishlistChanged;
		if (!(Target == null))
		{
			Target.onDestroy -= OnTargetDestroy;
			Target.onSetStackCount -= OnTargetSetStackCount;
			Target.onInspectionStateChanged -= OnTargetInspectionStateChanged;
			Target.onDurabilityChanged -= OnTargetDurabilityChanged;
		}
	}

	private void OnWishlistChanged(int type)
	{
		if (!(Target == null) && Target.TypeID == type)
		{
			RefreshWishlistInfo();
		}
	}

	private void OnTargetDurabilityChanged(Item item)
	{
		Refresh();
	}

	private void OnTargetDestroy(Item item)
	{
	}

	private void OnTargetSetStackCount(Item item)
	{
		if (item != Target)
		{
			Debug.LogError("触发事件的Item不匹配!");
		}
		Refresh();
	}

	private void OnItemUtilitiesSelectionChanged()
	{
		Refresh();
	}

	private void OnTargetInspectionStateChanged(Item item)
	{
		Refresh();
		Punch();
	}

	private void Clear()
	{
		SlotIndicatorPool.ReleaseAll();
	}

	private void SetupEmpty()
	{
		icon.sprite = EmptySprite.Get();
		icon.color = Color.clear;
		countText.text = string.Empty;
		nameText.text = string.Empty;
		durabilityFill.fillAmount = 0f;
		durabilityLoss.fillAmount = 0f;
		durabilityZeroIndicator.gameObject.SetActive(value: false);
	}

	private void Refresh()
	{
		if (this == null)
		{
			Debug.Log("NULL");
		}
		else
		{
			if (isBeingDestroyed)
			{
				return;
			}
			if (Target == null)
			{
				HideMainContentAndDisableControl();
				HideInspectionElements();
				if (ItemUIUtilities.SelectedItemDisplayRaw == this)
				{
					ItemUIUtilities.Select(null);
				}
			}
			else if (Target.NeedInspection)
			{
				HideMainContentAndDisableControl();
				ShowInspectionElements();
			}
			else
			{
				HideInspectionElements();
				ShowMainContentAndEnableControl();
			}
			selectionIndicator.gameObject.SetActive(Selected);
			RefreshWishlistInfo();
		}
	}

	private void RefreshWishlistInfo()
	{
		if (Target == null || Target.NeedInspection)
		{
			wishlistedIndicator.SetActive(value: false);
			questRequiredIndicator.SetActive(value: false);
			buildingRequiredIndicator.SetActive(value: false);
		}
		else
		{
			ItemWishlist.WishlistInfo wishlistInfo = ItemWishlist.GetWishlistInfo(Target.TypeID);
			wishlistedIndicator.SetActive(wishlistInfo.isManuallyWishlisted);
			questRequiredIndicator.SetActive(wishlistInfo.isQuestRequired);
			buildingRequiredIndicator.SetActive(wishlistInfo.isBuildingRequired);
		}
	}

	private void HideMainContentAndDisableControl()
	{
		mainContentShown = false;
		if (mainContentShown && ItemUIUtilities.SelectedItemDisplay == this)
		{
			ItemUIUtilities.Select(null);
		}
		interactionEventReceiver.raycastTarget = false;
		icon.gameObject.SetActive(value: false);
		countGameObject.SetActive(value: false);
		durabilityGameObject.SetActive(value: false);
		durabilityZeroIndicator.gameObject.SetActive(value: false);
		nameContainer.SetActive(value: false);
		slotIndicatorContainer.SetActive(value: false);
	}

	private void ShowMainContentAndEnableControl()
	{
		mainContentShown = true;
		interactionEventReceiver.raycastTarget = true;
		icon.gameObject.SetActive(value: true);
		nameContainer.SetActive(value: true);
		countText.text = (Target.Stackable ? Target.StackCount.ToString() : string.Empty);
		bool useDurability = Target.UseDurability;
		if (useDurability)
		{
			float num = Target.Durability / Target.MaxDurability;
			durabilityFill.fillAmount = num;
			durabilityFill.color = durabilityFillColorOverT.Evaluate(num);
			durabilityZeroIndicator.SetActive(Target.Durability <= 0f);
			durabilityLoss.fillAmount = Target.DurabilityLoss;
		}
		else
		{
			durabilityZeroIndicator.gameObject.SetActive(value: false);
		}
		countGameObject.SetActive(Target.Stackable);
		durabilityGameObject.SetActive(useDurability);
		slotIndicatorContainer.SetActive(value: true);
	}

	private void ShowInspectionElements()
	{
		inspectionElementRoot.gameObject.SetActive(value: true);
		bool inspecting = Target.Inspecting;
		if ((bool)inspectingElement)
		{
			inspectingElement.SetActive(inspecting);
		}
		if ((bool)notInspectingElement)
		{
			notInspectingElement.SetActive(!inspecting);
		}
	}

	private void HideInspectionElements()
	{
		inspectionElementRoot.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		RegisterEvents();
	}

	private void OnDisable()
	{
		ItemUIUtilities.OnSelectionChanged -= OnItemUtilitiesSelectionChanged;
		if (Selected)
		{
			ItemUIUtilities.Select(null);
		}
		UnregisterEvents();
	}

	private void OnDestroy()
	{
		UnregisterEvents();
		ItemUIUtilities.OnSelectionChanged -= OnItemUtilitiesSelectionChanged;
		isBeingDestroyed = true;
	}

	public static ItemDisplay Get()
	{
		return Pool.Get();
	}

	public static void Release(ItemDisplay item)
	{
		Pool.Release(item);
	}

	public void NotifyPooled()
	{
	}

	public void NotifyReleased()
	{
		UnregisterEvents();
		Target = null;
		SetupEmpty();
	}

	[ContextMenu("Select")]
	private void Select()
	{
		ItemUIUtilities.Select(this);
	}

	public void NotifySelected()
	{
	}

	public void NotifyUnselected()
	{
		KontextMenu.Hide(this);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		this.onPointerClick?.Invoke(this, eventData);
		if (!eventData.used && eventData.button == PointerEventData.InputButton.Left)
		{
			if (eventData.clickTime - lastClickTime <= 0.3f && !doubleClickInvoked)
			{
				doubleClickInvoked = true;
				this.onDoubleClicked?.Invoke(this, eventData);
			}
			if (!eventData.used && (!Target || !Target.NeedInspection))
			{
				if (ItemUIUtilities.SelectedItemDisplay != this)
				{
					Select();
					eventData.Use();
				}
				else
				{
					ItemUIUtilities.Select(null);
					eventData.Use();
				}
			}
		}
		if (eventData.clickTime - lastClickTime > 0.3f)
		{
			doubleClickInvoked = false;
		}
		lastClickTime = eventData.clickTime;
		Punch();
	}

	public void Punch()
	{
		selectionIndicator.transform.DOKill();
		icon.transform.DOKill();
		backgroundRing.transform.DOKill();
		selectionIndicator.transform.localScale = Vector3.one;
		icon.transform.localScale = Vector3.one;
		backgroundRing.transform.localScale = Vector3.one;
		selectionIndicator.transform.DOPunchScale(Vector3.one * selectionRingPunchScale, punchDuration);
		icon.transform.DOPunchScale(Vector3.one * iconPunchScale, punchDuration);
		backgroundRing.transform.DOPunchScale(Vector3.one * backgroundRingPunchScale, punchDuration);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	public void OnPointerUp(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!(Target == null))
		{
			ItemDisplay.OnPointerExitItemDisplay?.Invoke(this);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!(Target == null))
		{
			ItemDisplay.OnPointerEnterItemDisplay?.Invoke(this);
		}
	}

	public void OnDrop(PointerEventData eventData)
	{
		HandleDirectDrop(eventData);
		if (!eventData.used)
		{
			this.onReceiveDrop?.Invoke(eventData);
		}
	}

	private void HandleDirectDrop(PointerEventData eventData)
	{
		if (Target == null || eventData.button != PointerEventData.InputButton.Left || IsStockshopSample)
		{
			return;
		}
		IItemDragSource component = eventData.pointerDrag.gameObject.GetComponent<IItemDragSource>();
		if (component != null && component.IsEditable())
		{
			Item item = component.GetItem();
			if (Target.TryPlug(item))
			{
				ItemUIUtilities.NotifyPutItem(item);
				eventData.Use();
			}
		}
	}
}
