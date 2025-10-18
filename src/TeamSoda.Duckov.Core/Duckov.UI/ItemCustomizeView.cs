using System.Collections.Generic;
using Duckov.UI.Animations;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI;

public class ItemCustomizeView : View, ISingleSelectionMenu<SlotDisplay>
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private Button equipButton;

	[SerializeField]
	private Button unequipButton;

	[SerializeField]
	private ItemDetailsDisplay customizingTargetDisplay;

	[SerializeField]
	private ItemDetailsDisplay selectedItemDisplay;

	[SerializeField]
	private FadeGroup selectedItemDisplayFadeGroup;

	[SerializeField]
	private RectTransform slotSelectionIndicator;

	[SerializeField]
	private GameObject selectSlotPlaceHolder;

	[SerializeField]
	private GameObject avaliableItemsContainer;

	[SerializeField]
	private GameObject noAvaliableItemPlaceHolder;

	[SerializeField]
	private ItemDisplay itemDisplayTemplate;

	private PrefabPool<ItemDisplay> _itemDisplayPool;

	private Item target;

	private SlotDisplay selectedSlotDisplay;

	private List<Inventory> avaliableInventories = new List<Inventory>();

	private List<Item> avaliableItems = new List<Item>();

	public static ItemCustomizeView Instance => View.GetViewInstance<ItemCustomizeView>();

	private PrefabPool<ItemDisplay> ItemDisplayPool
	{
		get
		{
			if (_itemDisplayPool == null)
			{
				itemDisplayTemplate.gameObject.SetActive(value: false);
				_itemDisplayPool = new PrefabPool<ItemDisplay>(itemDisplayTemplate, itemDisplayTemplate.transform.parent);
			}
			return _itemDisplayPool;
		}
	}

	public Item Target => target;

	private void OnGetInventoryDisplay(InventoryDisplay display)
	{
		display.onDisplayDoubleClicked += OnInventoryDoubleClicked;
		display.ShowOperationButtons = false;
	}

	private void OnReleaseInventoryDisplay(InventoryDisplay display)
	{
		display.onDisplayDoubleClicked -= OnInventoryDoubleClicked;
	}

	private void OnInventoryDoubleClicked(InventoryDisplay display, InventoryEntry entry, PointerEventData data)
	{
		if (entry.Item != null)
		{
			target.TryPlug(entry.Item, emptyOnly: false, entry.Master.Target);
			data.Use();
		}
	}

	public void Setup(Item target, List<Inventory> avaliableInventories)
	{
		this.target = target;
		customizingTargetDisplay.Setup(target);
		this.avaliableInventories.Clear();
		this.avaliableInventories.AddRange(avaliableInventories);
	}

	public void DebugSetup(Item target, Inventory inventory1, Inventory inventory2)
	{
		Setup(target, new List<Inventory> { inventory1, inventory2 });
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		ItemUIUtilities.Select(null);
		ItemUIUtilities.OnSelectionChanged += OnItemSelectionChanged;
		fadeGroup.Show();
		SetSelection(null);
		RefreshDetails();
	}

	protected override void OnClose()
	{
		ItemUIUtilities.OnSelectionChanged -= OnItemSelectionChanged;
		base.OnClose();
		fadeGroup.Hide();
		selectedItemDisplayFadeGroup.Hide();
	}

	private void OnItemSelectionChanged()
	{
		RefreshDetails();
	}

	private void RefreshDetails()
	{
		if (ItemUIUtilities.SelectedItem != null)
		{
			selectedItemDisplayFadeGroup.Show();
			selectedItemDisplay.Setup(ItemUIUtilities.SelectedItem);
			Item item = selectedItemDisplay.Target;
			bool flag = selectedSlotDisplay.Target.Content != item;
			equipButton.gameObject.SetActive(flag);
			unequipButton.gameObject.SetActive(!flag);
		}
		else
		{
			selectedItemDisplayFadeGroup.Hide();
			equipButton.gameObject.SetActive(value: false);
			unequipButton.gameObject.SetActive(value: false);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		equipButton.onClick.AddListener(OnEquipButtonClicked);
		unequipButton.onClick.AddListener(OnUnequipButtonClicked);
		customizingTargetDisplay.SlotCollectionDisplay.onElementClicked += OnSlotElementClicked;
	}

	private void OnUnequipButtonClicked()
	{
		if (!(selectedSlotDisplay == null) && !(selectedItemDisplay == null))
		{
			Slot slot = selectedSlotDisplay.Target;
			if (slot.Content != null)
			{
				Item item = slot.Unplug();
				HandleUnpluggledItem(item);
			}
			RefreshAvaliableItems();
		}
	}

	private void OnEquipButtonClicked()
	{
		if (selectedSlotDisplay == null || selectedItemDisplay == null)
		{
			return;
		}
		Slot slot = selectedSlotDisplay.Target;
		Item item = selectedItemDisplay.Target;
		if (slot != null && !(item == null))
		{
			if (slot.Content != null)
			{
				Item item2 = slot.Unplug();
				HandleUnpluggledItem(item2);
			}
			item.Detach();
			if (!slot.Plug(item, out var _))
			{
				Debug.LogError("装备失败！");
				HandleUnpluggledItem(item);
			}
			RefreshAvaliableItems();
		}
	}

	private void HandleUnpluggledItem(Item item)
	{
		if ((bool)PlayerStorage.Inventory)
		{
			ItemUtilities.SendToPlayerStorage(item);
		}
		else if (!ItemUtilities.SendToPlayerCharacterInventory(item))
		{
			ItemUtilities.SendToPlayerStorage(item);
		}
	}

	private void OnSlotElementClicked(ItemSlotCollectionDisplay collection, SlotDisplay slot)
	{
		SetSelection(slot);
	}

	public SlotDisplay GetSelection()
	{
		return selectedSlotDisplay;
	}

	public bool SetSelection(SlotDisplay selection)
	{
		selectedSlotDisplay = selection;
		RefreshSelectionIndicator();
		OnSlotSelectionChanged();
		return true;
	}

	private void RefreshSelectionIndicator()
	{
		slotSelectionIndicator.gameObject.SetActive(selectedSlotDisplay);
		if (selectedSlotDisplay != null)
		{
			slotSelectionIndicator.position = selectedSlotDisplay.transform.position;
		}
	}

	private void OnSlotSelectionChanged()
	{
		ItemUIUtilities.Select(null);
		RefreshAvaliableItems();
	}

	private void RefreshAvaliableItems()
	{
		avaliableItems.Clear();
		if (!(selectedSlotDisplay == null))
		{
			Slot slot = selectedSlotDisplay.Target;
			if (!(selectedSlotDisplay == null))
			{
				foreach (Inventory avaliableInventory in avaliableInventories)
				{
					foreach (Item item in avaliableInventory)
					{
						if (!(item == null) && slot.CanPlug(item))
						{
							avaliableItems.Add(item);
						}
					}
				}
			}
		}
		RefreshItemListGraphics();
	}

	private void RefreshItemListGraphics()
	{
		Debug.Log("Refreshing Item List Graphics");
		bool flag = selectedSlotDisplay != null;
		bool flag2 = avaliableItems.Count > 0;
		selectSlotPlaceHolder.SetActive(!flag);
		noAvaliableItemPlaceHolder.SetActive(flag && !flag2);
		avaliableItemsContainer.SetActive(flag2);
		ItemDisplayPool.ReleaseAll();
		if (!flag2)
		{
			return;
		}
		foreach (Item avaliableItem in avaliableItems)
		{
			if (!(avaliableItem == null))
			{
				ItemDisplay itemDisplay = ItemDisplayPool.Get();
				itemDisplay.ShowOperationButtons = false;
				itemDisplay.Setup(avaliableItem);
			}
		}
	}
}
