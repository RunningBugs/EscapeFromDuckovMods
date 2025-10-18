using System.Collections.Generic;
using Duckov.UI.Animations;
using Duckov.Utilities;
using ItemStatsSystem;
using LeTai.TrueShadow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class ItemCustomizeSelectionView : View
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private ItemSlotCollectionDisplay slotDisplay;

	[SerializeField]
	private InventoryDisplay inventoryDisplay;

	[SerializeField]
	private ItemDetailsDisplay detailsDisplay;

	[SerializeField]
	private FadeGroup itemDetailsFadeGroup;

	[SerializeField]
	private FadeGroup customizeButtonFadeGroup;

	[SerializeField]
	private FadeGroup placeHolderFadeGroup;

	[SerializeField]
	private Button beginCustomizeButton;

	[SerializeField]
	private TextMeshProUGUI selectedItemName;

	[SerializeField]
	private Image selectedItemIcon;

	[SerializeField]
	private TrueShadow selectedItemShadow;

	[SerializeField]
	private GameObject customizableIndicator;

	[SerializeField]
	private GameObject uncustomizableIndicator;

	[SerializeField]
	private string noItemSelectedNameText = "-";

	[SerializeField]
	private Sprite noItemSelectedIconSprite;

	private List<Inventory> avaliableInventories = new List<Inventory>();

	public static ItemCustomizeSelectionView Instance => View.GetViewInstance<ItemCustomizeSelectionView>();

	private Item CharacterItem => LevelManager.Instance?.MainCharacter?.CharacterItem;

	private bool CanCustomize
	{
		get
		{
			Item selectedItem = ItemUIUtilities.SelectedItem;
			if (selectedItem == null)
			{
				return false;
			}
			if (selectedItem.Slots == null)
			{
				return false;
			}
			if (selectedItem.Slots.Count < 1)
			{
				return false;
			}
			return true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		beginCustomizeButton.onClick.AddListener(OnBeginCustomizeButtonClicked);
	}

	private void OnBeginCustomizeButtonClicked()
	{
		_ = ItemUIUtilities.SelectedItem;
		ItemCustomizeView instance = ItemCustomizeView.Instance;
		if (!(instance == null))
		{
			instance.Setup(ItemUIUtilities.SelectedItem, GetAvaliableInventories());
			instance.Open();
		}
	}

	private List<Inventory> GetAvaliableInventories()
	{
		avaliableInventories.Clear();
		Inventory inventory = LevelManager.Instance?.MainCharacter?.CharacterItem?.Inventory;
		if (inventory != null)
		{
			avaliableInventories.Add(inventory);
		}
		Inventory inventory2 = PlayerStorage.Inventory;
		if (inventory2 != null)
		{
			avaliableInventories.Add(inventory2);
		}
		return avaliableInventories;
	}

	protected override void OnOpen()
	{
		UnregisterEvents();
		base.OnOpen();
		Item characterItem = CharacterItem;
		if (characterItem == null)
		{
			Debug.LogError("物品栏开启失败，角色物体不存在");
			return;
		}
		base.gameObject.SetActive(value: true);
		slotDisplay.Setup(characterItem);
		inventoryDisplay.Setup(characterItem.Inventory);
		RegisterEvents();
		fadeGroup.Show();
		customizeButtonFadeGroup.SkipHide();
		placeHolderFadeGroup.SkipHide();
		ItemUIUtilities.Select(null);
		RefreshSelectedItemInfo();
	}

	protected override void OnClose()
	{
		UnregisterEvents();
		base.OnClose();
		fadeGroup.Hide();
		itemDetailsFadeGroup.Hide();
	}

	private void RegisterEvents()
	{
		ItemUIUtilities.OnSelectionChanged += OnItemSelectionChanged;
	}

	private void OnItemSelectionChanged()
	{
		if (ItemUIUtilities.SelectedItem != null)
		{
			detailsDisplay.Setup(ItemUIUtilities.SelectedItem);
			itemDetailsFadeGroup.Show();
		}
		else
		{
			itemDetailsFadeGroup.Hide();
		}
		if (CanCustomize)
		{
			placeHolderFadeGroup.Hide();
			customizeButtonFadeGroup.Show();
		}
		else
		{
			customizeButtonFadeGroup.Hide();
			placeHolderFadeGroup.Show();
		}
		RefreshSelectedItemInfo();
	}

	private void UnregisterEvents()
	{
		ItemUIUtilities.OnSelectionChanged -= OnItemSelectionChanged;
	}

	public static void Show()
	{
		if (!(Instance == null))
		{
			Instance.Open();
		}
	}

	public static void Hide()
	{
		if (!(Instance == null))
		{
			Instance.Close();
		}
	}

	private void RefreshSelectedItemInfo()
	{
		Item selectedItem = ItemUIUtilities.SelectedItem;
		if (selectedItem == null)
		{
			selectedItemName.text = noItemSelectedNameText;
			selectedItemIcon.sprite = noItemSelectedIconSprite;
			selectedItemShadow.enabled = false;
			customizableIndicator.SetActive(value: false);
			uncustomizableIndicator.SetActive(value: false);
			selectedItemIcon.color = Color.clear;
		}
		else
		{
			selectedItemShadow.enabled = true;
			selectedItemIcon.color = Color.white;
			selectedItemName.text = selectedItem.DisplayName;
			selectedItemIcon.sprite = selectedItem.Icon;
			GameplayDataSettings.UIStyle.GetDisplayQualityLook(selectedItem.DisplayQuality).Apply(selectedItemShadow);
			customizableIndicator.SetActive(CanCustomize);
			uncustomizableIndicator.SetActive(!CanCustomize);
		}
	}
}
