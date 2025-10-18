using System;
using System.Collections.Generic;
using Duckov.Economy;
using Duckov.UI.Animations;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using LeTai.TrueShadow;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class ItemRepairView : View
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
	private ItemRepair_RepairAllPanel repairAllPanel;

	[SerializeField]
	private FadeGroup repairButtonFadeGroup;

	[SerializeField]
	private FadeGroup placeHolderFadeGroup;

	[SerializeField]
	private Button repairButton;

	[SerializeField]
	private TextMeshProUGUI repairPriceText;

	[SerializeField]
	private TextMeshProUGUI selectedItemName;

	[SerializeField]
	private Image selectedItemIcon;

	[SerializeField]
	private TrueShadow selectedItemShadow;

	[SerializeField]
	private string noItemSelectedNameText = "-";

	[SerializeField]
	private Sprite noItemSelectedIconSprite;

	[SerializeField]
	private GameObject noNeedToRepairIndicator;

	[SerializeField]
	private GameObject brokenIndicator;

	[SerializeField]
	private GameObject cannotRepairIndicator;

	[SerializeField]
	private TextMeshProUGUI durabilityText;

	[SerializeField]
	private TextMeshProUGUI willLoseDurabilityText;

	[SerializeField]
	private Image barFill;

	[SerializeField]
	private Image lossBarFill;

	[SerializeField]
	private Gradient barFillColorOverT;

	private List<Inventory> avaliableInventories = new List<Inventory>();

	public static ItemRepairView Instance => View.GetViewInstance<ItemRepairView>();

	private Item CharacterItem => LevelManager.Instance?.MainCharacter?.CharacterItem;

	private bool CanRepair
	{
		get
		{
			Item selectedItem = ItemUIUtilities.SelectedItem;
			if (selectedItem == null)
			{
				return false;
			}
			if (!selectedItem.UseDurability)
			{
				return false;
			}
			if (selectedItem.MaxDurabilityWithLoss < 1f)
			{
				return false;
			}
			if (!selectedItem.Tags.Contains("Repairable"))
			{
				Debug.Log(selectedItem.DisplayName + " 不包含tag Repairable");
				return false;
			}
			return selectedItem.Durability < selectedItem.MaxDurabilityWithLoss;
		}
	}

	private bool NoNeedToRepair
	{
		get
		{
			Item selectedItem = ItemUIUtilities.SelectedItem;
			if (selectedItem == null)
			{
				return false;
			}
			if (!selectedItem.UseDurability)
			{
				return false;
			}
			return selectedItem.Durability >= selectedItem.MaxDurabilityWithLoss;
		}
	}

	private bool Broken
	{
		get
		{
			Item selectedItem = ItemUIUtilities.SelectedItem;
			if (selectedItem == null)
			{
				return false;
			}
			if (!selectedItem.UseDurability)
			{
				return false;
			}
			return selectedItem.MaxDurabilityWithLoss < 1f;
		}
	}

	public static event Action OnRepaireOptionDone;

	protected override void Awake()
	{
		base.Awake();
		repairButton.onClick.AddListener(OnRepairButtonClicked);
		itemDetailsFadeGroup.SkipHide();
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
		repairButtonFadeGroup.SkipHide();
		placeHolderFadeGroup.SkipHide();
		ItemUIUtilities.Select(null);
		RefreshSelectedItemInfo();
		repairAllPanel.Setup(this);
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
		if (ItemUIUtilities.SelectedItem != null)
		{
			detailsDisplay.Setup(ItemUIUtilities.SelectedItem);
			itemDetailsFadeGroup.Show();
		}
		else
		{
			itemDetailsFadeGroup.Hide();
		}
		if (CanRepair)
		{
			placeHolderFadeGroup.Hide();
			repairButtonFadeGroup.Show();
		}
		else
		{
			repairButtonFadeGroup.Hide();
			placeHolderFadeGroup.Show();
		}
		Item selectedItem = ItemUIUtilities.SelectedItem;
		willLoseDurabilityText.text = "";
		if (selectedItem == null)
		{
			selectedItemName.text = noItemSelectedNameText;
			selectedItemIcon.sprite = noItemSelectedIconSprite;
			selectedItemShadow.enabled = false;
			noNeedToRepairIndicator.SetActive(value: false);
			brokenIndicator.SetActive(value: false);
			cannotRepairIndicator.SetActive(value: false);
			selectedItemIcon.color = Color.clear;
			barFill.fillAmount = 0f;
			lossBarFill.fillAmount = 0f;
			durabilityText.text = "-";
			return;
		}
		selectedItemShadow.enabled = true;
		selectedItemIcon.color = Color.white;
		selectedItemName.text = selectedItem.DisplayName;
		selectedItemIcon.sprite = selectedItem.Icon;
		GameplayDataSettings.UIStyle.GetDisplayQualityLook(selectedItem.DisplayQuality).Apply(selectedItemShadow);
		noNeedToRepairIndicator.SetActive(!Broken && NoNeedToRepair && selectedItem.Repairable);
		cannotRepairIndicator.SetActive(selectedItem.UseDurability && !selectedItem.Repairable && !Broken);
		brokenIndicator.SetActive(Broken);
		if (CanRepair)
		{
			float repairAmount;
			float lostAmount;
			float lostPercentage;
			int num = CalculateRepairPrice(selectedItem, out repairAmount, out lostAmount, out lostPercentage);
			repairPriceText.text = num.ToString();
			willLoseDurabilityText.text = "UI_MaxDurability".ToPlainText() + " -" + lostAmount.ToString("0.#");
			repairButton.interactable = EconomyManager.Money >= num;
		}
		if (selectedItem.UseDurability)
		{
			float durability = selectedItem.Durability;
			float maxDurability = selectedItem.MaxDurability;
			float maxDurabilityWithLoss = selectedItem.MaxDurabilityWithLoss;
			float num2 = durability / maxDurability;
			barFill.fillAmount = num2;
			lossBarFill.fillAmount = selectedItem.DurabilityLoss;
			durabilityText.text = string.Format("{0:0.#} / {1} ", durability, maxDurabilityWithLoss.ToString("0.#"));
			barFill.color = barFillColorOverT.Evaluate(num2);
		}
		else
		{
			barFill.fillAmount = 0f;
			lossBarFill.fillAmount = 0f;
			durabilityText.text = "-";
		}
	}

	private void OnRepairButtonClicked()
	{
		Item selectedItem = ItemUIUtilities.SelectedItem;
		if (!(selectedItem == null) && selectedItem.UseDurability)
		{
			Repair(selectedItem);
			RefreshSelectedItemInfo();
		}
	}

	private void Repair(Item item, bool prepaied = false)
	{
		float repairAmount;
		float lostAmount;
		float lostPercentage;
		int num = CalculateRepairPrice(item, out repairAmount, out lostAmount, out lostPercentage);
		if (prepaied || EconomyManager.Pay(new Cost(num)))
		{
			item.DurabilityLoss += lostPercentage;
			item.Durability = item.MaxDurability * (1f - item.DurabilityLoss);
			ItemRepairView.OnRepaireOptionDone?.Invoke();
		}
	}

	private int CalculateRepairPrice(Item item, out float repairAmount, out float lostAmount, out float lostPercentage)
	{
		repairAmount = 0f;
		lostAmount = 0f;
		lostPercentage = 0f;
		if (item == null)
		{
			return 0;
		}
		if (!item.UseDurability)
		{
			return 0;
		}
		float maxDurability = item.MaxDurability;
		float durabilityLoss = item.DurabilityLoss;
		float num = maxDurability * (1f - durabilityLoss);
		float durability = item.Durability;
		repairAmount = num - durability;
		float repairLossRatio = item.GetRepairLossRatio();
		lostAmount = repairAmount * repairLossRatio;
		repairAmount -= lostAmount;
		if (repairAmount <= 0f)
		{
			return 0;
		}
		lostPercentage = lostAmount / maxDurability;
		float num2 = repairAmount / maxDurability;
		return Mathf.CeilToInt((float)item.Value * num2 * 0.5f);
	}

	public List<Item> GetAllEquippedItems()
	{
		CharacterMainControl main = CharacterMainControl.Main;
		if (main == null)
		{
			return null;
		}
		Item characterItem = main.CharacterItem;
		if (characterItem == null)
		{
			return null;
		}
		SlotCollection slots = characterItem.Slots;
		if (slots == null)
		{
			return null;
		}
		List<Item> list = new List<Item>();
		foreach (Slot item in slots)
		{
			if (item != null)
			{
				Item content = item.Content;
				if (!(content == null))
				{
					list.Add(content);
				}
			}
		}
		return list;
	}

	public int CalculateRepairPrice(List<Item> itemsToRepair)
	{
		int num = 0;
		foreach (Item item in itemsToRepair)
		{
			num += CalculateRepairPrice(item, out var _, out var _, out var _);
		}
		return num;
	}

	public void RepairItems(List<Item> itemsToRepair)
	{
		if (!EconomyManager.Pay(new Cost(CalculateRepairPrice(itemsToRepair))))
		{
			return;
		}
		foreach (Item item in itemsToRepair)
		{
			Repair(item, prepaied: true);
		}
	}
}
