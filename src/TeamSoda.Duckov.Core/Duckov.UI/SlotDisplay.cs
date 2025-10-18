using System;
using DG.Tweening;
using Duckov.UI.Animations;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Duckov.UI;

public class SlotDisplay : MonoBehaviour, IPoolable, IPointerClickHandler, IEventSystemHandler, IItemDragSource, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private Sprite defaultSlotIcon;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private ItemDisplay itemDisplay;

	[SerializeField]
	private Image slotIcon;

	[SerializeField]
	private FadeGroup pluggableIndicator;

	[SerializeField]
	private GameObject hoveringIndicator;

	[SerializeField]
	private bool editable = true;

	[SerializeField]
	private bool showOperationMenu = true;

	[SerializeField]
	private bool contentSelectable = true;

	[SerializeField]
	[Range(0f, 1f)]
	private float punchDuration = 0.1f;

	[SerializeField]
	[Range(-1f, 1f)]
	private float slotIconPunchScale = -0.1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float denialPunchDuration = 0.2f;

	[SerializeField]
	private Color slotIconDenialColor = Color.red;

	private Color iconInitialColor;

	private bool hovering;

	public bool Editable
	{
		get
		{
			return editable;
		}
		internal set
		{
			editable = value;
		}
	}

	public bool ContentSelectable
	{
		get
		{
			return contentSelectable;
		}
		internal set
		{
			contentSelectable = value;
		}
	}

	public bool ShowOperationMenu
	{
		get
		{
			return showOperationMenu;
		}
		internal set
		{
			showOperationMenu = value;
		}
	}

	public Slot Target { get; private set; }

	public static PrefabPool<SlotDisplay> Pool => GameplayUIManager.Instance.SlotDisplayPool;

	public bool Movable
	{
		get
		{
			return itemDisplay.Movable;
		}
		set
		{
			itemDisplay.Movable = value;
		}
	}

	internal event Action<SlotDisplay> onSlotDisplayClicked;

	internal event Action<SlotDisplay> onSlotDisplayDoubleClicked;

	public static event Action<SlotDisplayOperationContext> onOperation;

	private void RegisterEvents()
	{
		UnregisterEvents();
		if (base.isActiveAndEnabled)
		{
			if (Target != null)
			{
				Target.onSlotContentChanged += OnTargetContentChanged;
			}
			ItemUIUtilities.OnSelectionChanged += OnItemSelectionChanged;
			itemDisplay.onPointerClick += OnItemDisplayClicked;
			itemDisplay.onDoubleClicked += OnItemDisplayDoubleClicked;
			IItemDragSource.OnStartDragItem += OnStartDragItem;
			IItemDragSource.OnEndDragItem += OnEndDragItem;
			UIInputManager.OnFastPick += OnFastPick;
			UIInputManager.OnDropItem += OnFastDrop;
			UIInputManager.OnUseItem += OnFastUse;
		}
	}

	private void UnregisterEvents()
	{
		if (Target != null)
		{
			Target.onSlotContentChanged -= OnTargetContentChanged;
		}
		ItemUIUtilities.OnSelectionChanged -= OnItemSelectionChanged;
		itemDisplay.onPointerClick -= OnItemDisplayClicked;
		itemDisplay.onDoubleClicked -= OnItemDisplayDoubleClicked;
		IItemDragSource.OnStartDragItem -= OnStartDragItem;
		IItemDragSource.OnEndDragItem -= OnEndDragItem;
		UIInputManager.OnFastPick -= OnFastPick;
		UIInputManager.OnDropItem -= OnFastDrop;
		UIInputManager.OnUseItem -= OnFastUse;
	}

	private void OnFastDrop(UIInputEventData data)
	{
		if (base.isActiveAndEnabled && hovering && Target != null && !(Target.Content == null) && Target.Content.CanDrop && Editable)
		{
			Target.Content.Drop(CharacterMainControl.Main, createRigidbody: true);
		}
	}

	private void OnFastUse(UIInputEventData data)
	{
		if (base.isActiveAndEnabled && hovering && Target != null && !(Target.Content == null) && Target.Content.IsUsable(CharacterMainControl.Main))
		{
			CharacterMainControl.Main.UseItem(Target.Content);
		}
	}

	private void OnFastPick(UIInputEventData data)
	{
		if (base.isActiveAndEnabled && hovering)
		{
			OnItemDisplayDoubleClicked(itemDisplay, new PointerEventData(EventSystem.current));
		}
	}

	private void OnEndDragItem(Item item)
	{
		pluggableIndicator.Hide();
	}

	private void OnStartDragItem(Item item)
	{
		if (base.isActiveAndEnabled && Editable)
		{
			if (item != Target.Content && Target.CanPlug(item))
			{
				pluggableIndicator.Show();
			}
			else
			{
				pluggableIndicator.Hide();
			}
		}
	}

	private void OnItemDisplayDoubleClicked(ItemDisplay arg1, PointerEventData arg2)
	{
		this.onSlotDisplayDoubleClicked?.Invoke(this);
		if (!ContentSelectable)
		{
			arg2.Use();
		}
	}

	private void OnItemDisplayClicked(ItemDisplay display, PointerEventData data)
	{
		this.onSlotDisplayClicked?.Invoke(this);
		if (data.button == PointerEventData.InputButton.Left)
		{
			if (Keyboard.current != null && Keyboard.current.altKey.isPressed)
			{
				if (!Editable || !(Target.Content != null))
				{
					return;
				}
				Item content = Target.Content;
				content.Detach();
				if (!ItemUtilities.SendToPlayerCharacterInventory(content))
				{
					if (PlayerStorage.IsAccessableAndNotFull())
					{
						ItemUtilities.SendToPlayerStorage(content);
					}
					else
					{
						ItemUtilities.SendToPlayer(content, dontMerge: false, sendToStorage: false);
					}
				}
				data.Use();
			}
			else if (!ContentSelectable)
			{
				data.Use();
			}
		}
		else if (data.button == PointerEventData.InputButton.Right && Editable && Target?.Content != null)
		{
			ItemOperationMenu.Show(itemDisplay);
		}
	}

	private void OnTargetContentChanged(Slot slot)
	{
		Refresh();
		Punch();
	}

	private void OnItemSelectionChanged()
	{
	}

	public void Setup(Slot target)
	{
		UnregisterEvents();
		Target = target;
		label.text = target.DisplayName;
		Refresh();
		RegisterEvents();
		pluggableIndicator.Hide();
	}

	private void Refresh()
	{
		if (Target.Content == null)
		{
			slotIcon.gameObject.SetActive(value: true);
			if (Target.SlotIcon != null)
			{
				slotIcon.sprite = Target.SlotIcon;
			}
			else
			{
				slotIcon.sprite = defaultSlotIcon;
			}
		}
		else
		{
			slotIcon.gameObject.SetActive(value: false);
		}
		itemDisplay.ShowOperationButtons = showOperationMenu;
		itemDisplay.Setup(Target.Content);
	}

	public static SlotDisplay Get()
	{
		return Pool.Get();
	}

	public static void Release(SlotDisplay item)
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
	}

	private void Awake()
	{
		itemDisplay.onReceiveDrop += OnDrop;
	}

	private void OnEnable()
	{
		RegisterEvents();
		iconInitialColor = slotIcon.color;
		hoveringIndicator?.SetActive(value: false);
	}

	private void OnDisable()
	{
		UnregisterEvents();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		this.onSlotDisplayClicked?.Invoke(this);
		if (!Editable)
		{
			Punch();
			eventData.Use();
		}
		else
		{
			if (eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}
			Item selectedItem = ItemUIUtilities.SelectedItem;
			if (selectedItem == null)
			{
				Punch();
				return;
			}
			if (Target.Content != null)
			{
				Debug.Log("槽位 " + Target.DisplayName + " 中已经有物品。操作已取消。");
				DenialPunch();
				return;
			}
			if (!Target.CanPlug(selectedItem))
			{
				Debug.Log("物品 " + selectedItem.DisplayName + " 未通过槽位 " + Target.DisplayName + " 安装检测。操作已取消。");
				DenialPunch();
				return;
			}
			eventData.Use();
			selectedItem.Detach();
			Target.Plug(selectedItem, out var unpluggedItem);
			ItemUIUtilities.NotifyPutItem(selectedItem);
			if (unpluggedItem != null)
			{
				ItemUIUtilities.RaiseOrphan(unpluggedItem);
			}
			Punch();
		}
	}

	public void Punch()
	{
		if (slotIcon != null)
		{
			slotIcon.transform.DOKill();
			slotIcon.color = iconInitialColor;
			slotIcon.transform.localScale = Vector3.one;
			slotIcon.transform.DOPunchScale(Vector3.one * slotIconPunchScale, punchDuration);
		}
		if (itemDisplay != null)
		{
			itemDisplay.Punch();
		}
	}

	public void DenialPunch()
	{
		if (!(slotIcon == null))
		{
			slotIcon.transform.DOKill();
			slotIcon.color = iconInitialColor;
			slotIcon.DOColor(slotIconDenialColor, denialPunchDuration).From();
			SlotDisplay.onOperation?.Invoke(new SlotDisplayOperationContext(this, SlotDisplayOperationContext.Operation.Deny, succeed: false));
		}
	}

	public bool IsEditable()
	{
		return Editable;
	}

	public Item GetItem()
	{
		return Target?.Content;
	}

	public void OnDrop(PointerEventData eventData)
	{
		if (!Editable || eventData.used || eventData.button != PointerEventData.InputButton.Left)
		{
			return;
		}
		IItemDragSource component = eventData.pointerDrag.gameObject.GetComponent<IItemDragSource>();
		if (component == null || !component.IsEditable())
		{
			return;
		}
		Item item = component.GetItem();
		if (item == null || SetAmmo(item))
		{
			return;
		}
		if (!Target.CanPlug(item))
		{
			Debug.Log("物品 " + item.DisplayName + " 未通过槽位 " + Target.DisplayName + " 安装检测。操作已取消。");
			DenialPunch();
			return;
		}
		Inventory inInventory = item.InInventory;
		Slot pluggedIntoSlot = item.PluggedIntoSlot;
		if (pluggedIntoSlot == Target)
		{
			return;
		}
		ItemUIUtilities.NotifyPutItem(item);
		bool flag = false;
		flag = Target.Plug(item, out var unpluggedItem);
		if (unpluggedItem != null && (!(inInventory != null) || !inInventory.AddAndMerge(unpluggedItem)))
		{
			if (pluggedIntoSlot != null && pluggedIntoSlot.CanPlug(unpluggedItem) && pluggedIntoSlot.Plug(unpluggedItem, out var unpluggedItem2))
			{
				if ((bool)unpluggedItem2)
				{
					Debug.LogError("Source slot spit out an unplugged item! " + unpluggedItem2.DisplayName);
				}
			}
			else if (!ItemUtilities.SendToPlayerCharacter(unpluggedItem) && (!(View.ActiveView is LootView lootView) || !(lootView.TargetInventory != null) || !lootView.TargetInventory.AddAndMerge(unpluggedItem)))
			{
				if (PlayerStorage.IsAccessableAndNotFull())
				{
					ItemUtilities.SendToPlayerStorage(unpluggedItem);
				}
				else
				{
					unpluggedItem.Drop(CharacterMainControl.Main, createRigidbody: true);
				}
			}
		}
		SlotDisplay.onOperation?.Invoke(new SlotDisplayOperationContext(this, SlotDisplayOperationContext.Operation.Equip, flag));
	}

	public void OnDrag(PointerEventData eventData)
	{
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

	private bool SetAmmo(Item incomming)
	{
		ItemSetting_Gun itemSetting_Gun = Target?.Content?.GetComponent<ItemSetting_Gun>();
		if (itemSetting_Gun == null)
		{
			return false;
		}
		if (!itemSetting_Gun.IsValidBullet(incomming))
		{
			return false;
		}
		if (View.ActiveView is InventoryView || View.ActiveView is LootView)
		{
			View.ActiveView.Close();
		}
		return itemSetting_Gun.LoadSpecificBullet(incomming);
	}
}
