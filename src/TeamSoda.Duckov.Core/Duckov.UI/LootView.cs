using System.Collections.Generic;
using Duckov.UI.Animations;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using SodaCraft.StringUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI;

public class LootView : View
{
	[SerializeField]
	private ItemSlotCollectionDisplay characterSlotCollectionDisplay;

	[SerializeField]
	private InventoryDisplay characterInventoryDisplay;

	[SerializeField]
	private InventoryDisplay petInventoryDisplay;

	[SerializeField]
	private InventoryDisplay lootTargetInventoryDisplay;

	[SerializeField]
	private InventoryFilterDisplay lootTargetFilterDisplay;

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private Button pickAllButton;

	[SerializeField]
	private TextMeshProUGUI lootTargetDisplayName;

	[SerializeField]
	private TextMeshProUGUI lootTargetCapacityText;

	[SerializeField]
	private string lootTargetCapacityTextFormat = "({itemCount}/{capacity})";

	[SerializeField]
	private Button storeAllButton;

	[SerializeField]
	private FadeGroup lootTargetFadeGroup;

	[SerializeField]
	private ItemDetailsDisplay detailsDisplay;

	[SerializeField]
	private FadeGroup detailsFadeGroup;

	[SerializeField]
	private InteractableLootbox targetLootBox;

	private Inventory targetInventory;

	private HashSet<Inventory> lootedInventories = new HashSet<Inventory>();

	public static LootView Instance => View.GetViewInstance<LootView>();

	private CharacterMainControl Character => LevelManager.Instance.MainCharacter;

	private Item CharacterItem
	{
		get
		{
			if (Character == null)
			{
				return null;
			}
			return Character.CharacterItem;
		}
	}

	public Inventory TargetInventory
	{
		get
		{
			if (targetLootBox != null)
			{
				return targetLootBox.Inventory;
			}
			if ((bool)targetInventory)
			{
				return targetInventory;
			}
			return null;
		}
	}

	public static bool HasInventoryEverBeenLooted(Inventory inventory)
	{
		if (Instance == null)
		{
			return false;
		}
		if (Instance.lootedInventories == null)
		{
			return false;
		}
		if (inventory == null)
		{
			return false;
		}
		return Instance.lootedInventories.Contains(inventory);
	}

	protected override void Awake()
	{
		base.Awake();
		InteractableLootbox.OnStartLoot += OnStartLoot;
		pickAllButton.onClick.AddListener(OnPickAllButtonClicked);
		CharacterMainControl.OnMainCharacterStartUseItem += OnMainCharacterStartUseItem;
		LevelManager.OnMainCharacterDead += OnMainCharacterDead;
		storeAllButton.onClick.AddListener(OnStoreAllButtonClicked);
	}

	private void OnStoreAllButtonClicked()
	{
		if (TargetInventory == null || TargetInventory != PlayerStorage.Inventory || CharacterItem == null)
		{
			return;
		}
		Inventory inventory = CharacterItem.Inventory;
		if (inventory == null)
		{
			return;
		}
		int lastItemPosition = inventory.GetLastItemPosition();
		for (int i = 0; i <= lastItemPosition; i++)
		{
			if (inventory.lockedIndexes.Contains(i))
			{
				continue;
			}
			Item itemAt = inventory.GetItemAt(i);
			if (!(itemAt == null))
			{
				if (!TargetInventory.AddAndMerge(itemAt))
				{
					break;
				}
				if (i == 0)
				{
					AudioManager.PlayPutItemSFX(itemAt);
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		UnregisterEvents();
		InteractableLootbox.OnStartLoot -= OnStartLoot;
		LevelManager.OnMainCharacterDead -= OnMainCharacterDead;
		base.OnDestroy();
	}

	private void OnMainCharacterStartUseItem(Item _item)
	{
		if (base.open)
		{
			Close();
		}
	}

	private void OnMainCharacterDead(DamageInfo dmgInfo)
	{
		if (base.open)
		{
			Close();
		}
	}

	private void OnEnable()
	{
		RegisterEvents();
	}

	private void OnDisable()
	{
		UnregisterEvents();
		targetLootBox?.StopInteract();
		targetLootBox = null;
	}

	public void Show()
	{
		Open();
	}

	private void OnStartLoot(InteractableLootbox lootbox)
	{
		targetLootBox = lootbox;
		if (targetLootBox == null || targetLootBox.Inventory == null)
		{
			Debug.LogError("Target loot box could not be found");
			return;
		}
		Open();
		if (TargetInventory != null)
		{
			lootedInventories.Add(TargetInventory);
		}
	}

	private void OnStopLoot(InteractableLootbox lootbox)
	{
		if (lootbox == targetLootBox)
		{
			targetLootBox = null;
			Close();
		}
	}

	public static void LootItem(Item item)
	{
		if (!(item == null) && !(Instance == null))
		{
			Instance.targetInventory = item.Inventory;
			Instance.Open();
		}
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		UnregisterEvents();
		base.gameObject.SetActive(value: true);
		characterSlotCollectionDisplay.Setup(CharacterItem, movable: true);
		if ((bool)PetProxy.PetInventory)
		{
			petInventoryDisplay.gameObject.SetActive(value: true);
			petInventoryDisplay.Setup(PetProxy.PetInventory);
		}
		else
		{
			petInventoryDisplay.gameObject.SetActive(value: false);
		}
		characterInventoryDisplay.Setup(CharacterItem.Inventory, null, null, movable: true);
		if (targetLootBox != null)
		{
			lootTargetInventoryDisplay.ShowSortButton = targetLootBox.ShowSortButton;
			lootTargetInventoryDisplay.Setup(TargetInventory, null, null, movable: true);
			lootTargetDisplayName.text = TargetInventory.DisplayName;
			if ((bool)TargetInventory.GetComponent<InventoryFilterProvider>())
			{
				lootTargetFilterDisplay.gameObject.SetActive(value: true);
				lootTargetFilterDisplay.Setup(lootTargetInventoryDisplay);
				lootTargetFilterDisplay.Select(0);
			}
			else
			{
				lootTargetFilterDisplay.gameObject.SetActive(value: false);
			}
			lootTargetFadeGroup.Show();
		}
		else if (targetInventory != null)
		{
			lootTargetInventoryDisplay.ShowSortButton = false;
			lootTargetInventoryDisplay.Setup(TargetInventory, null, null, movable: true);
			lootTargetFadeGroup.Show();
			lootTargetFilterDisplay.gameObject.SetActive(value: false);
		}
		else
		{
			lootTargetFadeGroup.SkipHide();
		}
		bool active = TargetInventory != null && TargetInventory == PlayerStorage.Inventory;
		storeAllButton.gameObject.SetActive(active);
		fadeGroup.Show();
		RefreshDetails();
		RefreshPickAllButton();
		RegisterEvents();
		RefreshCapacityText();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
		detailsFadeGroup.Hide();
		targetLootBox?.StopInteract();
		targetLootBox = null;
		targetInventory = null;
		if ((bool)SplitDialogue.Instance && SplitDialogue.Instance.isActiveAndEnabled)
		{
			SplitDialogue.Instance.Cancel();
		}
		UnregisterEvents();
	}

	private void OnTargetInventoryContentChanged(Inventory inventory, int arg2)
	{
		RefreshPickAllButton();
		RefreshCapacityText();
	}

	private void RefreshCapacityText()
	{
		if (targetLootBox != null)
		{
			lootTargetCapacityText.text = lootTargetCapacityTextFormat.Format(new
			{
				itemCount = TargetInventory.GetItemCount(),
				capacity = TargetInventory.Capacity
			});
		}
	}

	private void RegisterEvents()
	{
		UnregisterEvents();
		ItemUIUtilities.OnSelectionChanged += OnSelectionChanged;
		lootTargetInventoryDisplay.onDisplayDoubleClicked += OnLootTargetItemDoubleClicked;
		characterInventoryDisplay.onDisplayDoubleClicked += OnCharacterInventoryItemDoubleClicked;
		petInventoryDisplay.onDisplayDoubleClicked += OnCharacterInventoryItemDoubleClicked;
		characterSlotCollectionDisplay.onElementDoubleClicked += OnCharacterSlotItemDoubleClicked;
		if ((bool)TargetInventory)
		{
			TargetInventory.onContentChanged += OnTargetInventoryContentChanged;
		}
		UIInputManager.OnNextPage += OnNextPage;
		UIInputManager.OnPreviousPage += OnPreviousPage;
	}

	private void OnPreviousPage(UIInputEventData data)
	{
		if (!(TargetInventory == null) && lootTargetInventoryDisplay.UsePages)
		{
			lootTargetInventoryDisplay.PreviousPage();
		}
	}

	private void OnNextPage(UIInputEventData data)
	{
		if (!(TargetInventory == null) && lootTargetInventoryDisplay.UsePages)
		{
			lootTargetInventoryDisplay.NextPage();
		}
	}

	private void UnregisterEvents()
	{
		ItemUIUtilities.OnSelectionChanged -= OnSelectionChanged;
		if ((bool)lootTargetInventoryDisplay)
		{
			lootTargetInventoryDisplay.onDisplayDoubleClicked -= OnLootTargetItemDoubleClicked;
		}
		if ((bool)characterInventoryDisplay)
		{
			characterInventoryDisplay.onDisplayDoubleClicked -= OnCharacterInventoryItemDoubleClicked;
		}
		if ((bool)petInventoryDisplay)
		{
			petInventoryDisplay.onDisplayDoubleClicked -= OnCharacterInventoryItemDoubleClicked;
		}
		if ((bool)characterSlotCollectionDisplay)
		{
			characterSlotCollectionDisplay.onElementDoubleClicked -= OnCharacterSlotItemDoubleClicked;
		}
		if ((bool)TargetInventory)
		{
			TargetInventory.onContentChanged -= OnTargetInventoryContentChanged;
		}
		UIInputManager.OnNextPage -= OnNextPage;
		UIInputManager.OnPreviousPage -= OnPreviousPage;
	}

	private void OnCharacterSlotItemDoubleClicked(ItemSlotCollectionDisplay collectionDisplay, SlotDisplay slotDisplay)
	{
		if (slotDisplay == null)
		{
			return;
		}
		Slot target = slotDisplay.Target;
		if (target == null)
		{
			return;
		}
		Item content = target.Content;
		if (content == null || TargetInventory == null || (content.Sticky && !TargetInventory.AcceptSticky))
		{
			return;
		}
		AudioManager.PlayPutItemSFX(content);
		content.Detach();
		if (TargetInventory.AddAndMerge(content))
		{
			RefreshDetails();
			return;
		}
		if (!target.Plug(content, out var unpluggedItem))
		{
			Debug.LogError("Failed plugging back!");
		}
		if (unpluggedItem != null)
		{
			Debug.Log("Unplugged item should be null!");
		}
		RefreshDetails();
	}

	private void OnCharacterInventoryItemDoubleClicked(InventoryDisplay display, InventoryEntry entry, PointerEventData data)
	{
		Item content = entry.Content;
		if (content == null)
		{
			return;
		}
		Inventory inInventory = content.InInventory;
		if (TargetInventory == null || (content.Sticky && !TargetInventory.AcceptSticky))
		{
			return;
		}
		AudioManager.PlayPutItemSFX(content);
		content.Detach();
		if (TargetInventory.AddAndMerge(content))
		{
			RefreshDetails();
			return;
		}
		if (!inInventory.AddAndMerge(content))
		{
			Debug.LogError("Failed sending back item");
		}
		RefreshDetails();
	}

	private void OnSelectionChanged()
	{
		RefreshDetails();
	}

	private void RefreshDetails()
	{
		if (ItemUIUtilities.SelectedItem != null)
		{
			detailsFadeGroup.Show();
			detailsDisplay.Setup(ItemUIUtilities.SelectedItem);
		}
		else
		{
			detailsFadeGroup.Hide();
		}
	}

	private void OnLootTargetItemDoubleClicked(InventoryDisplay display, InventoryEntry entry, PointerEventData data)
	{
		Item item = entry.Item;
		if (item == null || item.IsInPlayerCharacter())
		{
			return;
		}
		if (targetLootBox != null && targetLootBox.needInspect && !item.Inspected)
		{
			data.Use();
			return;
		}
		data.Use();
		bool flag = false;
		bool? flag2 = LevelManager.Instance?.MainCharacter?.CharacterItem?.TryPlug(item, emptyOnly: true);
		flag |= flag2.Value;
		if (!flag2.HasValue || !flag2.Value)
		{
			flag |= ItemUtilities.SendToPlayerCharacterInventory(item);
		}
		if (flag)
		{
			AudioManager.PlayPutItemSFX(item);
			RefreshDetails();
		}
	}

	private void RefreshPickAllButton()
	{
		if (!(TargetInventory == null))
		{
			pickAllButton.gameObject.SetActive(value: false);
			bool interactable = TargetInventory.GetItemCount() > 0;
			pickAllButton.interactable = interactable;
		}
	}

	private void OnPickAllButtonClicked()
	{
		if (TargetInventory == null)
		{
			return;
		}
		List<Item> list = new List<Item>();
		list.AddRange(TargetInventory);
		foreach (Item item in list)
		{
			if (!(item == null) && (!targetLootBox.needInspect || item.Inspected))
			{
				bool? flag = LevelManager.Instance?.MainCharacter?.CharacterItem?.TryPlug(item, emptyOnly: true);
				if (!flag.HasValue || !flag.Value)
				{
					ItemUtilities.SendToPlayerCharacterInventory(item);
				}
			}
		}
		AudioManager.Post("UI/confirm");
	}
}
