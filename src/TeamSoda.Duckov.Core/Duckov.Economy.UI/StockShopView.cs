using System;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.UI.Animations;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.Economy.UI;

public class StockShopView : View, ISingleSelectionMenu<StockShopItemEntry>
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private FadeGroup detailsFadeGroup;

	[SerializeField]
	private ItemDetailsDisplay details;

	[SerializeField]
	private InventoryDisplay playerInventoryDisplay;

	[SerializeField]
	private InventoryDisplay petInventoryDisplay;

	[SerializeField]
	private InventoryDisplay playerStorageDisplay;

	[SerializeField]
	private StockShopItemEntry entryTemplate;

	[SerializeField]
	private TextMeshProUGUI stockText;

	[SerializeField]
	[LocalizationKey("Default")]
	private string stockTextKey = "UI_Stock";

	[SerializeField]
	private string stockTextFormat = "{text} {current}/{max}";

	[SerializeField]
	private TextMeshProUGUI merchantNameText;

	[SerializeField]
	private Button interactionButton;

	[SerializeField]
	private Image interactionButtonImage;

	[SerializeField]
	private Color buttonColor_Interactable;

	[SerializeField]
	private Color buttonColor_NotInteractable;

	[SerializeField]
	private TextMeshProUGUI interactionText;

	[SerializeField]
	private GameObject cashOnlyIndicator;

	[SerializeField]
	private GameObject cannotSellIndicator;

	[LocalizationKey("Default")]
	[SerializeField]
	private string textBuy = "购买";

	[LocalizationKey("Default")]
	[SerializeField]
	private string textSoldOut = "已售罄";

	[LocalizationKey("Default")]
	[SerializeField]
	private string textSell = "出售";

	[LocalizationKey("Default")]
	[SerializeField]
	private string textUnlock = "解锁";

	[LocalizationKey("Default")]
	[SerializeField]
	private string textLocked = "已锁定";

	[SerializeField]
	private GameObject priceDisplay;

	[SerializeField]
	private TextMeshProUGUI priceText;

	[SerializeField]
	private GameObject lockDisplay;

	[SerializeField]
	private FadeGroup clickBlockerFadeGroup;

	[SerializeField]
	private TextMeshProUGUI refreshCountDown;

	private string sfx_Buy = "UI/buy";

	private string sfx_Sell = "UI/sell";

	private PrefabPool<StockShopItemEntry> _entryPool;

	private StockShop target;

	private StockShopItemEntry selectedItem;

	public Action onSelectionChanged;

	public static StockShopView Instance => View.GetViewInstance<StockShopView>();

	private string TextBuy => textBuy.ToPlainText();

	private string TextSoldOut => textSoldOut.ToPlainText();

	private string TextSell => textSell.ToPlainText();

	private string TextUnlock => textUnlock.ToPlainText();

	private string TextLocked => textLocked.ToPlainText();

	private PrefabPool<StockShopItemEntry> EntryPool
	{
		get
		{
			if (_entryPool == null)
			{
				_entryPool = new PrefabPool<StockShopItemEntry>(entryTemplate, entryTemplate.transform.parent);
				entryTemplate.gameObject.SetActive(value: false);
			}
			return _entryPool;
		}
	}

	private UnityEngine.Object Selection
	{
		get
		{
			if (ItemUIUtilities.SelectedItemDisplay != null)
			{
				return ItemUIUtilities.SelectedItemDisplay;
			}
			if (selectedItem != null)
			{
				return selectedItem;
			}
			return null;
		}
	}

	public StockShop Target => target;

	protected override void Awake()
	{
		base.Awake();
		interactionButton.onClick.AddListener(OnInteractionButtonClicked);
		UIInputManager.OnFastPick += OnFastPick;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UIInputManager.OnFastPick -= OnFastPick;
	}

	private void OnFastPick(UIInputEventData data)
	{
		if (base.isActiveAndEnabled)
		{
			OnInteractionButtonClicked();
		}
	}

	private void FixedUpdate()
	{
		RefreshCountDown();
	}

	private void RefreshCountDown()
	{
		if (target == null)
		{
			refreshCountDown.text = "-";
		}
		TimeSpan nextRefreshETA = target.NextRefreshETA;
		int days = nextRefreshETA.Days;
		int hours = nextRefreshETA.Hours;
		int minutes = nextRefreshETA.Minutes;
		int seconds = nextRefreshETA.Seconds;
		refreshCountDown.text = string.Format("{0}{1:00}:{2:00}:{3:00}", (days > 0) ? (days + " - ") : "", hours, minutes, seconds);
	}

	private void OnInteractionButtonClicked()
	{
		if (Selection == null)
		{
			return;
		}
		if (Selection is ItemDisplay itemDisplay)
		{
			Target.Sell(itemDisplay.Target).Forget();
			AudioManager.Post(sfx_Sell);
			ItemUIUtilities.Select(null);
			OnSelectionChanged();
		}
		else if (Selection is StockShopItemEntry stockShopItemEntry)
		{
			int itemTypeID = stockShopItemEntry.Target.ItemTypeID;
			if (stockShopItemEntry.IsUnlocked())
			{
				BuyTask(itemTypeID).Forget();
			}
			else if (EconomyManager.IsWaitingForUnlockConfirm(itemTypeID))
			{
				EconomyManager.ConfirmUnlock(itemTypeID);
			}
		}
	}

	private async UniTask BuyTask(int itemTypeID)
	{
		if (await Target.Buy(itemTypeID))
		{
			AudioManager.Post(sfx_Buy);
			clickBlockerFadeGroup.SkipShow();
			await UniTask.NextFrame();
			await clickBlockerFadeGroup.HideAndReturnTask();
		}
	}

	private void OnEnable()
	{
		ItemUIUtilities.OnSelectionChanged += OnItemUIUtilitiesSelectionChanged;
		EconomyManager.OnItemUnlockStateChanged += OnItemUnlockStateChanged;
		StockShop.OnAfterItemSold += OnAfterItemSold;
		UIInputManager.OnNextPage += OnNextPage;
		UIInputManager.OnPreviousPage += OnPreviousPage;
	}

	private void OnDisable()
	{
		ItemUIUtilities.OnSelectionChanged -= OnItemUIUtilitiesSelectionChanged;
		EconomyManager.OnItemUnlockStateChanged -= OnItemUnlockStateChanged;
		StockShop.OnAfterItemSold -= OnAfterItemSold;
		UIInputManager.OnNextPage -= OnNextPage;
		UIInputManager.OnPreviousPage -= OnPreviousPage;
	}

	private void OnNextPage(UIInputEventData data)
	{
		playerStorageDisplay.NextPage();
	}

	private void OnPreviousPage(UIInputEventData data)
	{
		playerStorageDisplay.PreviousPage();
	}

	private void OnAfterItemSold(StockShop shop)
	{
		RefreshInteractionButton();
		RefreshStockText();
	}

	private void OnItemUnlockStateChanged(int itemTypeID)
	{
		if (!(details.Target == null) && itemTypeID == details.Target.TypeID)
		{
			RefreshInteractionButton();
			RefreshStockText();
		}
	}

	private void OnItemUIUtilitiesSelectionChanged()
	{
		if (selectedItem != null && ItemUIUtilities.SelectedItemDisplay != null)
		{
			selectedItem = null;
		}
		OnSelectionChanged();
	}

	private void OnSelectionChanged()
	{
		onSelectionChanged?.Invoke();
		if (Selection == null)
		{
			detailsFadeGroup.Hide();
			return;
		}
		Item item = null;
		if (Selection is StockShopItemEntry stockShopItemEntry)
		{
			item = stockShopItemEntry.GetItem();
		}
		else if (Selection is ItemDisplay itemDisplay)
		{
			item = itemDisplay.Target;
		}
		if (item == null)
		{
			detailsFadeGroup.Hide();
			return;
		}
		details.Setup(item);
		RefreshStockText();
		RefreshInteractionButton();
		RefreshCountDown();
		detailsFadeGroup.Show();
	}

	private void RefreshStockText()
	{
		if (Selection is StockShopItemEntry stockShopItemEntry)
		{
			stockText.gameObject.SetActive(value: true);
			stockText.text = stockTextFormat.Format(new
			{
				text = stockTextKey.ToPlainText(),
				current = stockShopItemEntry.Target.CurrentStock,
				max = stockShopItemEntry.Target.MaxStock
			});
		}
		else if (Selection is ItemDisplay)
		{
			stockText.gameObject.SetActive(value: false);
		}
	}

	public StockShopItemEntry GetSelection()
	{
		return Selection as StockShopItemEntry;
	}

	public bool SetSelection(StockShopItemEntry selection)
	{
		if (ItemUIUtilities.SelectedItem != null)
		{
			ItemUIUtilities.Select(null);
		}
		selectedItem = selection;
		OnSelectionChanged();
		return true;
	}

	internal void Setup(StockShop target)
	{
		this.target = target;
		detailsFadeGroup.SkipHide();
		merchantNameText.text = target.DisplayName;
		Inventory inventory = LevelManager.Instance?.MainCharacter?.CharacterItem?.Inventory;
		playerInventoryDisplay.Setup(inventory, null, (Item e) => e == null || e.CanBeSold);
		if (PetProxy.PetInventory != null)
		{
			petInventoryDisplay.Setup(PetProxy.PetInventory, null, (Item e) => e == null || e.CanBeSold);
			petInventoryDisplay.gameObject.SetActive(value: true);
		}
		else
		{
			petInventoryDisplay.gameObject.SetActive(value: false);
		}
		Inventory inventory2 = PlayerStorage.Inventory;
		if (inventory2 != null)
		{
			playerStorageDisplay.gameObject.SetActive(value: true);
			playerStorageDisplay.Setup(inventory2, null, (Item e) => e == null || e.CanBeSold);
		}
		else
		{
			playerStorageDisplay.gameObject.SetActive(value: false);
		}
		EntryPool.ReleaseAll();
		Transform setParent = entryTemplate.transform.parent;
		foreach (StockShop.Entry entry in target.entries)
		{
			if (entry.Show)
			{
				StockShopItemEntry stockShopItemEntry = EntryPool.Get(setParent);
				stockShopItemEntry.Setup(this, entry);
				stockShopItemEntry.transform.SetAsLastSibling();
			}
		}
		TradingUIUtilities.ActiveMerchant = target;
	}

	private void RefreshInteractionButton()
	{
		cannotSellIndicator.SetActive(value: false);
		cashOnlyIndicator.SetActive(!Target.AccountAvaliable);
		if (Selection is ItemDisplay itemDisplay)
		{
			bool canBeSold = itemDisplay.Target.CanBeSold;
			interactionButton.interactable = canBeSold;
			priceDisplay.gameObject.SetActive(value: true);
			lockDisplay.gameObject.SetActive(value: false);
			interactionText.text = TextSell;
			interactionButtonImage.color = buttonColor_Interactable;
			priceText.text = GetPriceText(itemDisplay.Target, selling: true);
			cannotSellIndicator.SetActive(!itemDisplay.Target.CanBeSold);
		}
		else
		{
			if (!(Selection is StockShopItemEntry stockShopItemEntry))
			{
				return;
			}
			bool flag = stockShopItemEntry.IsUnlocked();
			bool flag2 = EconomyManager.IsWaitingForUnlockConfirm(stockShopItemEntry.Target.ItemTypeID);
			interactionButton.interactable = flag || flag2;
			priceDisplay.gameObject.SetActive(flag);
			lockDisplay.gameObject.SetActive(!flag);
			cannotSellIndicator.SetActive(value: false);
			if (flag)
			{
				Item item = stockShopItemEntry.GetItem();
				int num = GetPrice(item, selling: false);
				bool enough = new Cost(num).Enough;
				priceText.text = num.ToString("n0");
				if (stockShopItemEntry.Target.CurrentStock > 0)
				{
					interactionText.text = TextBuy;
					interactionButtonImage.color = (enough ? buttonColor_Interactable : buttonColor_NotInteractable);
				}
				else
				{
					interactionButton.interactable = false;
					interactionText.text = TextSoldOut;
					interactionButtonImage.color = buttonColor_NotInteractable;
				}
			}
			else if (flag2)
			{
				interactionText.text = TextUnlock;
				interactionButtonImage.color = buttonColor_Interactable;
			}
			else
			{
				interactionText.text = TextLocked;
				interactionButtonImage.color = buttonColor_NotInteractable;
			}
		}
		int GetPrice(Item item2, bool selling)
		{
			return Target.ConvertPrice(item2, selling);
		}
		string GetPriceText(Item item2, bool selling)
		{
			return GetPrice(item2, selling).ToString("n0");
		}
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		fadeGroup.Show();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	internal void SetupAndShow(StockShop stockShop)
	{
		ItemUIUtilities.Select(null);
		SetSelection(null);
		Setup(stockShop);
		Open();
	}
}
