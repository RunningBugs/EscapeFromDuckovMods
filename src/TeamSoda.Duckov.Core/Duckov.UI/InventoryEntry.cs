using System;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Duckov.UI;

public class InventoryEntry : MonoBehaviour, IPoolable, IPointerClickHandler, IEventSystemHandler, IDropHandler, IItemDragSource, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private ItemDisplay itemDisplay;

	[SerializeField]
	private GameObject shortcutIndicator;

	[SerializeField]
	private GameObject disabledIndicator;

	[SerializeField]
	private GameObject hoveringIndicator;

	[SerializeField]
	private GameObject highlightIndicator;

	[SerializeField]
	private GameObject lockIndicator;

	[SerializeField]
	private int index;

	[SerializeField]
	private bool disabled;

	private bool cacheContentIsGun;

	private ItemMetaData cachedMeta;

	public const float doubleClickTimeThreshold = 0.3f;

	private float lastClickTime;

	private bool hovering;

	public InventoryDisplay Master { get; private set; }

	public int Index => index;

	public bool Disabled
	{
		get
		{
			return disabled;
		}
		set
		{
			disabled = value;
			Refresh();
		}
	}

	public Item Content
	{
		get
		{
			Inventory inventory = Master?.Target;
			if (inventory == null)
			{
				return null;
			}
			if (index < inventory.Capacity)
			{
				return Master?.Target?.GetItemAt(index);
			}
			return null;
		}
	}

	public bool ShouldHighlight
	{
		get
		{
			if (Master == null)
			{
				return false;
			}
			if (Content == null)
			{
				return false;
			}
			if (Master.EvaluateShouldHighlight(Content))
			{
				return true;
			}
			if (Editable && ItemUIUtilities.IsGunSelected && !cacheContentIsGun)
			{
				return IsCaliberMatchItemSelected();
			}
			return false;
		}
	}

	public bool CanOperate
	{
		get
		{
			if (Master == null)
			{
				return false;
			}
			return Master.Func_CanOperate(Content);
		}
	}

	public bool Editable
	{
		get
		{
			if (Master == null)
			{
				return false;
			}
			if (!Master.Editable)
			{
				return false;
			}
			return CanOperate;
		}
	}

	public bool Movable
	{
		get
		{
			if (Master == null)
			{
				return false;
			}
			return Master.Movable;
		}
	}

	public static PrefabPool<InventoryEntry> Pool => GameplayUIManager.Instance.InventoryEntryPool;

	public Item Item
	{
		get
		{
			if (itemDisplay != null && itemDisplay.isActiveAndEnabled)
			{
				return itemDisplay.Target;
			}
			return null;
		}
	}

	public event Action<InventoryEntry> onRefresh;

	private bool IsCaliberMatchItemSelected()
	{
		if (Content == null)
		{
			return false;
		}
		return ItemUIUtilities.SelectedItemCaliber == cachedMeta.caliber;
	}

	private void Awake()
	{
		itemDisplay.onPointerClick += OnItemDisplayPointerClicked;
		itemDisplay.onDoubleClicked += OnDisplayDoubleClicked;
		itemDisplay.onReceiveDrop += OnDrop;
		hoveringIndicator?.SetActive(value: false);
		UIInputManager.OnFastPick += OnFastPick;
		UIInputManager.OnDropItem += OnDropItemButton;
		UIInputManager.OnUseItem += OnUseItemButton;
	}

	private void OnEnable()
	{
		ItemUIUtilities.OnSelectionChanged += OnSelectionChanged;
		UIInputManager.OnLockInventoryIndex += OnInputLockInventoryIndex;
		UIInputManager.OnShortcutInput += OnShortcutInput;
	}

	private void OnDisable()
	{
		hovering = false;
		hoveringIndicator?.SetActive(value: false);
		ItemUIUtilities.OnSelectionChanged -= OnSelectionChanged;
		UIInputManager.OnLockInventoryIndex -= OnInputLockInventoryIndex;
		UIInputManager.OnShortcutInput -= OnShortcutInput;
	}

	private void OnShortcutInput(UIInputEventData data, int shortcutIndex)
	{
		if (hovering && !(Item == null))
		{
			ItemShortcut.Set(shortcutIndex, Item);
			ItemUIUtilities.NotifyPutItem(Item);
		}
	}

	private void OnInputLockInventoryIndex(UIInputEventData data)
	{
		if (hovering)
		{
			ToggleLock();
		}
	}

	private void OnSelectionChanged()
	{
		highlightIndicator.SetActive(ShouldHighlight);
		if (ItemUIUtilities.SelectedItemDisplay == itemDisplay)
		{
			Refresh();
		}
	}

	private void OnDestroy()
	{
		UIInputManager.OnFastPick -= OnFastPick;
		UIInputManager.OnDropItem -= OnDropItemButton;
		UIInputManager.OnUseItem -= OnUseItemButton;
		if (itemDisplay != null)
		{
			itemDisplay.onPointerClick -= OnItemDisplayPointerClicked;
			itemDisplay.onDoubleClicked -= OnDisplayDoubleClicked;
			itemDisplay.onReceiveDrop -= OnDrop;
		}
	}

	private void OnFastPick(UIInputEventData data)
	{
		if (!data.Used && base.isActiveAndEnabled && hovering)
		{
			Master.NotifyItemDoubleClicked(this, new PointerEventData(EventSystem.current));
			data.Use();
		}
	}

	private void OnDropItemButton(UIInputEventData data)
	{
		if (base.isActiveAndEnabled && hovering && !(Item == null) && Item.CanDrop && CanOperate)
		{
			Item.Drop(CharacterMainControl.Main, createRigidbody: true);
		}
	}

	private void OnUseItemButton(UIInputEventData data)
	{
		if (base.isActiveAndEnabled && hovering && !(Item == null) && Item.IsUsable(CharacterMainControl.Main) && CanOperate)
		{
			CharacterMainControl.Main.UseItem(Item);
		}
	}

	private void OnItemDisplayPointerClicked(ItemDisplay display, PointerEventData data)
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (disabled || !CanOperate)
		{
			data.Use();
		}
		else
		{
			if (!Editable)
			{
				return;
			}
			if (data.button == PointerEventData.InputButton.Left)
			{
				if (Content == null)
				{
					return;
				}
				if (Keyboard.current != null && Keyboard.current.altKey.isPressed)
				{
					data.Use();
					if (ItemUIUtilities.SelectedItem != null)
					{
						ItemUIUtilities.SelectedItem.TryPlug(Content);
					}
					CharacterMainControl.Main.CharacterItem.TryPlug(Content);
				}
				else if (!(ItemUIUtilities.SelectedItem == null) && Content.Stackable && ItemUIUtilities.SelectedItem != Content && ItemUIUtilities.SelectedItem.TypeID == Content.TypeID)
				{
					ItemUIUtilities.SelectedItem.CombineInto(Content);
				}
			}
			else if (data.button == PointerEventData.InputButton.Right && Editable && Content != null)
			{
				ItemOperationMenu.Show(itemDisplay);
			}
		}
	}

	private void OnDisplayDoubleClicked(ItemDisplay display, PointerEventData data)
	{
		Master.NotifyItemDoubleClicked(this, data);
	}

	public void Setup(InventoryDisplay master, int index, bool disabled = false)
	{
		Master = master;
		this.index = index;
		this.disabled = disabled;
		Refresh();
	}

	internal void Refresh()
	{
		Item content = Content;
		if (content != null)
		{
			cachedMeta = ItemAssetsCollection.GetMetaData(content.TypeID);
			cacheContentIsGun = content.Tags.Contains("Gun");
		}
		else
		{
			cacheContentIsGun = false;
			cachedMeta = default(ItemMetaData);
		}
		itemDisplay.Setup(content);
		itemDisplay.CanDrop = CanOperate;
		itemDisplay.Movable = Movable;
		itemDisplay.Editable = Editable && CanOperate;
		itemDisplay.CanLockSort = true;
		if (!Master.Target.NeedInspection && content != null)
		{
			content.Inspected = true;
		}
		itemDisplay.ShowOperationButtons = Master.ShowOperationButtons;
		shortcutIndicator.gameObject.SetActive(Master.IsShortcut(index));
		disabledIndicator.SetActive(disabled || !CanOperate);
		highlightIndicator.SetActive(ShouldHighlight);
		bool active = Master.Target.IsIndexLocked(Index);
		lockIndicator.SetActive(active);
		this.onRefresh?.Invoke(this);
	}

	public static InventoryEntry Get()
	{
		return Pool.Get();
	}

	public static void Release(InventoryEntry item)
	{
		Pool.Release(item);
	}

	public void NotifyPooled()
	{
	}

	public void NotifyReleased()
	{
		Master = null;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Punch();
		if (eventData.button != PointerEventData.InputButton.Left)
		{
			return;
		}
		lastClickTime = eventData.clickTime;
		if (Editable)
		{
			Item selectedItem = ItemUIUtilities.SelectedItem;
			if (!(selectedItem == null))
			{
				if (Content != null)
				{
					Debug.Log($"{Master.Target.name}(Inventory) 的 {index} 已经有物品。操作已取消。");
				}
				else
				{
					eventData.Use();
					selectedItem.Detach();
					Master.Target.AddAt(selectedItem, index);
					ItemUIUtilities.NotifyPutItem(selectedItem);
				}
			}
		}
		lastClickTime = eventData.clickTime;
	}

	internal void Punch()
	{
		itemDisplay.Punch();
	}

	public void OnDrag(PointerEventData eventData)
	{
	}

	public void OnDrop(PointerEventData eventData)
	{
		if (eventData.used || !Editable || eventData.button != PointerEventData.InputButton.Left)
		{
			return;
		}
		IItemDragSource component = eventData.pointerDrag.gameObject.GetComponent<IItemDragSource>();
		if (component == null || !component.IsEditable())
		{
			return;
		}
		Item item = component.GetItem();
		if (item == null || (item.Sticky && !Master.Target.AcceptSticky))
		{
			return;
		}
		if (Keyboard.current != null && Keyboard.current.ctrlKey.isPressed)
		{
			if (Content != null)
			{
				NotificationText.Push("UI_Inventory_TargetOccupiedCannotSplit".ToPlainText());
				return;
			}
			Debug.Log("SPLIT");
			SplitDialogue.SetupAndShow(item, Master.Target, index);
			return;
		}
		ItemUIUtilities.NotifyPutItem(item);
		if (Content == null)
		{
			item.Detach();
			Master.Target.AddAt(item, index);
			return;
		}
		if (Content.TypeID == item.TypeID && Content.Stackable)
		{
			Content.Combine(item);
			return;
		}
		Inventory inInventory = item.InInventory;
		Inventory target = Master.Target;
		if (inInventory != null)
		{
			int atPosition = inInventory.GetIndex(item);
			int atPosition2 = index;
			Item content = Content;
			if (content != item)
			{
				item.Detach();
				content.Detach();
				inInventory.AddAt(content, atPosition);
				target.AddAt(item, atPosition2);
			}
		}
	}

	public bool IsEditable()
	{
		if (Content == null)
		{
			return false;
		}
		if (Content.NeedInspection)
		{
			return false;
		}
		return Editable;
	}

	public Item GetItem()
	{
		return Content;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hovering = true;
		hoveringIndicator?.SetActive(Editable);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hovering = false;
		hoveringIndicator?.SetActive(value: false);
	}

	public void ToggleLock()
	{
		Master.Target.ToggleLockIndex(Index);
	}
}
