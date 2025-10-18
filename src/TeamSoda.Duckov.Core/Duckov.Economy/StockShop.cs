using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.Economy.UI;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using Saves;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.Economy;

public class StockShop : MonoBehaviour, IMerchant, ISaveDataProvider
{
	public class Entry
	{
		private StockShopDatabase.ItemEntry entry;

		[SerializeField]
		private bool show = true;

		[SerializeField]
		private int currentStock;

		public int MaxStock
		{
			get
			{
				if (entry.maxStock < 1)
				{
					entry.maxStock = 1;
				}
				return entry.maxStock;
			}
		}

		public int ItemTypeID => entry.typeID;

		public bool ForceUnlock
		{
			get
			{
				if (GameMetaData.Instance.IsDemo && entry.lockInDemo)
				{
					return false;
				}
				return entry.forceUnlock;
			}
		}

		public float PriceFactor => entry.priceFactor;

		public float Possibility => entry.possibility;

		public bool Show
		{
			get
			{
				return show;
			}
			set
			{
				show = value;
			}
		}

		public int CurrentStock
		{
			get
			{
				return currentStock;
			}
			set
			{
				currentStock = value;
				this.onStockChanged?.Invoke(this);
			}
		}

		public event Action<Entry> onStockChanged;

		public Entry(StockShopDatabase.ItemEntry cur)
		{
			entry = cur;
		}
	}

	[Serializable]
	public class OverrideSellingPriceEntry
	{
		[ItemTypeID]
		public int typeID;

		public float factor = 0.5f;
	}

	[Serializable]
	private class SaveData
	{
		public class StockCountEntry
		{
			public int itemTypeID;

			public int stock;
		}

		[DateTime]
		public long lastTimeRefreshedStock;

		public List<StockCountEntry> stockCounts = new List<StockCountEntry>();
	}

	[SerializeField]
	private string merchantID = "Albert";

	[LocalizationKey("Default")]
	public string DisplayNameKey;

	[TimeSpan]
	[SerializeField]
	private long refreshAfterTimeSpan;

	[SerializeField]
	private string purchaseNotificationTextFormatKey = "UI_StockShop_PurchasedNotification";

	[SerializeField]
	private bool accountAvaliable;

	[SerializeField]
	private bool returnCash;

	[SerializeField]
	private bool refreshStockOnStart;

	public float sellFactor = 0.5f;

	public List<Entry> entries = new List<Entry>();

	public List<OverrideSellingPriceEntry> overrideSellingPrice = new List<OverrideSellingPriceEntry>();

	[DateTime]
	[SerializeField]
	private long lastTimeRefreshedStock;

	private Dictionary<int, Item> itemInstances = new Dictionary<int, Item>();

	private bool buying;

	private bool selling;

	public string MerchantID => merchantID;

	public string OpinionKey => "Opinion_" + merchantID;

	public string DisplayName => DisplayNameKey.ToPlainText();

	private int Opinion
	{
		get
		{
			return Mathf.Clamp(CommonVariables.GetInt(OpinionKey), -100, 100);
		}
		set
		{
			CommonVariables.SetInt(OpinionKey, value);
		}
	}

	public string PurchaseNotificationTextFormat => purchaseNotificationTextFormatKey.ToPlainText();

	public bool AccountAvaliable => accountAvaliable;

	public TimeSpan TimeSinceLastRefresh
	{
		get
		{
			DateTime dateTime = DateTime.FromBinary(lastTimeRefreshedStock);
			if (dateTime > DateTime.UtcNow)
			{
				dateTime = DateTime.UtcNow;
				lastTimeRefreshedStock = DateTime.UtcNow.ToBinary();
				GameManager.TimeTravelDetected();
			}
			return DateTime.UtcNow - dateTime;
		}
	}

	public TimeSpan NextRefreshETA
	{
		get
		{
			TimeSpan timeSinceLastRefresh = TimeSinceLastRefresh;
			TimeSpan timeSpan = TimeSpan.FromTicks(refreshAfterTimeSpan) - timeSinceLastRefresh;
			if (timeSpan < TimeSpan.Zero)
			{
				timeSpan = TimeSpan.Zero;
			}
			return timeSpan;
		}
	}

	private string SaveKey => "StockShop_" + merchantID;

	public bool Busy
	{
		get
		{
			if (buying)
			{
				return true;
			}
			if (selling)
			{
				return true;
			}
			return false;
		}
	}

	public static event Action<StockShop> OnAfterItemSold;

	public static event Action<StockShop, Item> OnItemPurchased;

	public static event Action<StockShop, Item, int> OnItemSoldByPlayer;

	private async UniTask<Item> GetItemInstance(int typeID)
	{
		if (itemInstances.TryGetValue(typeID, out var value))
		{
			return value;
		}
		Item item = await ItemAssetsCollection.InstantiateAsync(typeID);
		item.transform.SetParent(base.transform);
		item.gameObject.SetActive(value: false);
		itemInstances[typeID] = item;
		return item;
	}

	public Item GetItemInstanceDirect(int typeID)
	{
		if (itemInstances.TryGetValue(typeID, out var value))
		{
			return value;
		}
		return null;
	}

	private void Awake()
	{
		InitializeEntries();
		SavesSystem.OnCollectSaveData += Save;
		SavesSystem.OnSetFile += Load;
		Load();
	}

	private void InitializeEntries()
	{
		StockShopDatabase.MerchantProfile merchantProfile = StockShopDatabase.Instance.GetMerchantProfile(merchantID);
		if (merchantProfile == null)
		{
			Debug.Log("未配置商人 " + merchantID);
			return;
		}
		foreach (StockShopDatabase.ItemEntry entry in merchantProfile.entries)
		{
			entries.Add(new Entry(entry));
		}
	}

	private void Load()
	{
		if (SavesSystem.KeyExisits(SaveKey))
		{
			SaveData dataRaw = SavesSystem.Load<SaveData>(SaveKey);
			SetupSaveData(dataRaw);
		}
	}

	private void Save()
	{
		if (!(GenerateSaveData() is SaveData value))
		{
			Debug.LogError("没法正确生成StockShop的SaveData");
		}
		else
		{
			SavesSystem.Save(SaveKey, value);
		}
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= Save;
		SavesSystem.OnSetFile -= Load;
	}

	private void Start()
	{
		CacheItemInstances().Forget();
		if (refreshStockOnStart)
		{
			DoRefreshStock();
			lastTimeRefreshedStock = DateTime.UtcNow.ToBinary();
		}
	}

	private async UniTask CacheItemInstances()
	{
		List<UniTask> list = new List<UniTask>();
		foreach (Entry entry in entries)
		{
			UniTask<Item> itemInstance = GetItemInstance(entry.ItemTypeID);
			list.Add(itemInstance);
		}
		await UniTask.WhenAll(list);
	}

	internal void RefreshIfNeeded()
	{
		TimeSpan timeSpan = TimeSpan.FromTicks(refreshAfterTimeSpan);
		DateTime dateTime = DateTime.FromBinary(lastTimeRefreshedStock);
		if (dateTime > DateTime.UtcNow)
		{
			dateTime = DateTime.UtcNow;
			lastTimeRefreshedStock = dateTime.ToBinary();
		}
		DateTime dateTime2 = DateTime.UtcNow - TimeSpan.FromDays(2.0);
		if (dateTime < dateTime2)
		{
			lastTimeRefreshedStock = dateTime2.ToBinary();
		}
		if (DateTime.UtcNow - dateTime > timeSpan)
		{
			DoRefreshStock();
			lastTimeRefreshedStock = DateTime.UtcNow.ToBinary();
		}
	}

	private void DoRefreshStock()
	{
		bool advancedDebuffMode = LevelManager.Rule.AdvancedDebuffMode;
		foreach (Entry entry in entries)
		{
			if (entry.Possibility > 0f && entry.Possibility < 1f && UnityEngine.Random.Range(0f, 1f) > entry.Possibility)
			{
				entry.Show = false;
				entry.CurrentStock = 0;
				continue;
			}
			ItemMetaData metaData = ItemAssetsCollection.GetMetaData(entry.ItemTypeID);
			if (!advancedDebuffMode && metaData.tags.Contains(GameplayDataSettings.Tags.AdvancedDebuffMode))
			{
				entry.Show = false;
				entry.CurrentStock = 0;
			}
			else
			{
				entry.Show = true;
				entry.CurrentStock = entry.MaxStock;
			}
		}
	}

	public async UniTask<bool> Buy(int itemTypeID, int amount = 1)
	{
		if (Busy)
		{
			return false;
		}
		buying = true;
		bool result = await BuyTask(itemTypeID, amount);
		buying = false;
		return result;
	}

	private async UniTask<bool> BuyTask(int itemTypeID, int amount = 1)
	{
		Entry found = entries.First((Entry e) => e != null && e.ItemTypeID == itemTypeID);
		if (found == null)
		{
			return false;
		}
		if (found.CurrentStock < 1)
		{
			return false;
		}
		Item itemInstanceDirect = GetItemInstanceDirect(itemTypeID);
		if (!itemInstanceDirect.Stackable)
		{
			amount = 1;
		}
		if (found.CurrentStock < amount)
		{
			return false;
		}
		if (itemInstanceDirect == null)
		{
			return false;
		}
		if (!EconomyManager.Pay(new Cost(ConvertPrice(itemInstanceDirect)), accountAvaliable))
		{
			return false;
		}
		Item item = await ItemAssetsCollection.InstantiateAsync(itemTypeID);
		if (!ItemUtilities.SendToPlayerCharacterInventory(item))
		{
			Debug.Log("玩家身上没地儿了，发送到玩家仓储处");
			ItemUtilities.SendToPlayerStorage(item);
		}
		found.CurrentStock -= amount;
		StockShop.OnAfterItemSold?.Invoke(this);
		StockShop.OnItemPurchased?.Invoke(this, item);
		NotificationText.Push(PurchaseNotificationTextFormat.Format(new
		{
			itemDisplayName = item.DisplayName
		}));
		return true;
	}

	internal async UniTask Sell(Item target)
	{
		if (!Busy && !(target == null) && target.CanBeSold)
		{
			selling = true;
			int sellPrice = ConvertPrice(target, selling: true);
			target.Detach();
			target.DestroyTree();
			if (returnCash)
			{
				await new Cost((451, sellPrice)).Return(directToBuffer: false, toPlayerInventory: true);
			}
			else
			{
				EconomyManager.Add(sellPrice);
			}
			StockShop.OnItemSoldByPlayer?.Invoke(this, target, sellPrice);
			selling = false;
		}
	}

	public void ShowUI()
	{
		if ((bool)StockShopView.Instance)
		{
			RefreshIfNeeded();
			StockShopView.Instance.SetupAndShow(this);
		}
	}

	public int ConvertPrice(Item item, bool selling = false)
	{
		int num = item.GetTotalRawValue();
		if (!selling)
		{
			Entry entry = entries.Find((Entry e) => e != null && e.ItemTypeID == item.TypeID);
			if (entry != null)
			{
				num = Mathf.FloorToInt((float)num * entry.PriceFactor);
			}
		}
		if (selling)
		{
			float factor = sellFactor;
			OverrideSellingPriceEntry overrideSellingPriceEntry = overrideSellingPrice.Find((OverrideSellingPriceEntry e) => e.typeID == item.TypeID);
			if (overrideSellingPriceEntry != null)
			{
				factor = overrideSellingPriceEntry.factor;
			}
			return Mathf.FloorToInt((float)num * factor);
		}
		return num;
	}

	public object GenerateSaveData()
	{
		SaveData saveData = new SaveData();
		saveData.lastTimeRefreshedStock = lastTimeRefreshedStock;
		foreach (Entry entry in entries)
		{
			saveData.stockCounts.Add(new SaveData.StockCountEntry
			{
				itemTypeID = entry.ItemTypeID,
				stock = entry.CurrentStock
			});
		}
		return saveData;
	}

	public void SetupSaveData(object dataRaw)
	{
		if (!(dataRaw is SaveData saveData))
		{
			return;
		}
		lastTimeRefreshedStock = saveData.lastTimeRefreshedStock;
		foreach (Entry cur in entries)
		{
			SaveData.StockCountEntry stockCountEntry = saveData.stockCounts.Find((SaveData.StockCountEntry e) => e != null && e.itemTypeID == cur.ItemTypeID);
			if (stockCountEntry != null)
			{
				cur.Show = stockCountEntry.stock > 0;
				cur.CurrentStock = stockCountEntry.stock;
			}
		}
	}
}
