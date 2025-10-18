using System;
using Duckov.Bitcoins;
using Duckov.UI;
using Duckov.UI.Animations;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BitcoinMinerView : View
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private InventoryDisplay inventoryDisplay;

	[SerializeField]
	private InventoryDisplay storageDisplay;

	[SerializeField]
	private ItemSlotCollectionDisplay minerSlotsDisplay;

	[SerializeField]
	private InventoryDisplay minerInventoryDisplay;

	[SerializeField]
	private TextMeshProUGUI commentText;

	[SerializeField]
	private TextMeshProUGUI remainingTimeText;

	[SerializeField]
	private TextMeshProUGUI timeEachCoinText;

	[SerializeField]
	private TextMeshProUGUI performanceText;

	[SerializeField]
	private Image fill;

	public static BitcoinMinerView Instance => View.GetViewInstance<BitcoinMinerView>();

	[LocalizationKey("Default")]
	private string ActiveCommentKey
	{
		get
		{
			return "UI_BitcoinMiner_Active";
		}
		set
		{
		}
	}

	[LocalizationKey("Default")]
	private string StoppedCommentKey
	{
		get
		{
			return "UI_BitcoinMiner_Stopped";
		}
		set
		{
		}
	}

	protected override void Awake()
	{
		base.Awake();
		minerInventoryDisplay.onDisplayDoubleClicked += OnMinerInventoryEntryDoubleClicked;
		inventoryDisplay.onDisplayDoubleClicked += OnPlayerItemsDoubleClicked;
		storageDisplay.onDisplayDoubleClicked += OnPlayerItemsDoubleClicked;
		minerSlotsDisplay.onElementDoubleClicked += OnMinerSlotEntryDoubleClicked;
	}

	private void OnMinerSlotEntryDoubleClicked(ItemSlotCollectionDisplay display1, SlotDisplay slotDisplay)
	{
		Slot target = slotDisplay.Target;
		if (target != null)
		{
			Item content = target.Content;
			if (!(content == null))
			{
				ItemUtilities.SendToPlayer(content, dontMerge: false, PlayerStorage.Instance != null);
			}
		}
	}

	private void OnPlayerItemsDoubleClicked(InventoryDisplay display, InventoryEntry entry, PointerEventData data)
	{
		Item content = entry.Content;
		if (!(content == null))
		{
			Item item = BitcoinMiner.Instance.Item;
			if (!(item == null))
			{
				item.TryPlug(content, emptyOnly: true, content.InInventory);
			}
		}
	}

	private void OnMinerInventoryEntryDoubleClicked(InventoryDisplay display, InventoryEntry entry, PointerEventData data)
	{
		Item content = entry.Content;
		if (!(content == null) && data.button == PointerEventData.InputButton.Left)
		{
			ItemUtilities.SendToPlayer(content);
		}
	}

	public static void Show()
	{
		if (!(Instance == null) && !(BitcoinMiner.Instance == null))
		{
			Instance.Open();
		}
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		CharacterMainControl main = CharacterMainControl.Main;
		if (!(main == null))
		{
			Item characterItem = main.CharacterItem;
			if (!(characterItem == null))
			{
				BitcoinMiner instance = BitcoinMiner.Instance;
				if (!instance.Loading)
				{
					Item item = instance.Item;
					if (!(item == null))
					{
						inventoryDisplay.Setup(characterItem.Inventory);
						if (PlayerStorage.Inventory != null)
						{
							storageDisplay.gameObject.SetActive(value: true);
							storageDisplay.Setup(PlayerStorage.Inventory);
						}
						else
						{
							storageDisplay.gameObject.SetActive(value: false);
						}
						minerSlotsDisplay.Setup(item);
						minerInventoryDisplay.Setup(item.Inventory);
						fadeGroup.Show();
						return;
					}
				}
			}
		}
		Debug.Log("Failed");
		Close();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	private void FixedUpdate()
	{
		RefreshStatus();
	}

	private void RefreshStatus()
	{
		if (BitcoinMiner.Instance.WorkPerSecond > 0.0)
		{
			TimeSpan remainingTime = BitcoinMiner.Instance.RemainingTime;
			TimeSpan timePerCoin = BitcoinMiner.Instance.TimePerCoin;
			remainingTimeText.text = $"{Mathf.FloorToInt((float)remainingTime.TotalHours):00}:{remainingTime.Minutes:00}:{remainingTime.Seconds:00}";
			timeEachCoinText.text = $"{Mathf.FloorToInt((float)timePerCoin.TotalHours):00}:{timePerCoin.Minutes:00}:{timePerCoin.Seconds:00}";
			performanceText.text = $"{BitcoinMiner.Instance.Performance:0.#}";
			commentText.text = ActiveCommentKey.ToPlainText();
		}
		else
		{
			remainingTimeText.text = "--:--:--";
			timeEachCoinText.text = "--:--:--";
			commentText.text = StoppedCommentKey.ToPlainText();
			performanceText.text = $"{BitcoinMiner.Instance.Performance:0.#}";
		}
		fill.fillAmount = BitcoinMiner.Instance.NormalizedProgress;
	}
}
