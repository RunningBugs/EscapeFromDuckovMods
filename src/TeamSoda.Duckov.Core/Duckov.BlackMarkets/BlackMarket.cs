using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.Economy;
using Duckov.Utilities;
using ItemStatsSystem;
using Saves;
using UnityEngine;

namespace Duckov.BlackMarkets;

public class BlackMarket : MonoBehaviour
{
	public class OnRequestMaxRefreshChanceEventContext
	{
		private int value;

		public int Value => value;

		public void Add(int count = 1)
		{
			value += count;
		}
	}

	public class OnRequestRefreshTimeFactorEventContext
	{
		private float value = 1f;

		public float Value => value;

		public void Add(float count = -0.1f)
		{
			value += count;
		}
	}

	[Serializable]
	public class DemandSupplyEntry
	{
		[SerializeField]
		[ItemTypeID]
		internal int itemID;

		[SerializeField]
		internal int remaining;

		[SerializeField]
		internal float priceFactor;

		[SerializeField]
		internal int batchCount;

		public int ItemID => itemID;

		internal ItemMetaData ItemMetaData => ItemAssetsCollection.GetMetaData(itemID);

		public int Remaining => remaining;

		public int TotalPrice => Mathf.FloorToInt((float)ItemMetaData.priceEach * priceFactor * (float)ItemMetaData.defaultStackCount * (float)batchCount);

		public Cost BuyCost => new Cost(TotalPrice);

		public Cost SellCost => new Cost((ItemMetaData.id, ItemMetaData.defaultStackCount * batchCount));

		public string ItemDisplayName => ItemMetaData.DisplayName;

		public event Action<DemandSupplyEntry> onChanged;

		internal void NotifyChange()
		{
			this.onChanged?.Invoke(this);
		}
	}

	[Serializable]
	public struct SaveData
	{
		public bool valid;

		public long lastRefreshedTimeRaw;

		public int refreshChance;

		public DemandSupplyEntry[] demands;

		public DemandSupplyEntry[] supplies;

		public SaveData(BlackMarket blackMarket)
		{
			valid = true;
			lastRefreshedTimeRaw = blackMarket.lastRefreshedTimeRaw;
			demands = blackMarket.demands.ToArray();
			supplies = blackMarket.supplies.ToArray();
			refreshChance = blackMarket.refreshChance;
		}
	}

	[SerializeField]
	private int demandsCount = 3;

	[SerializeField]
	private int suppliesCount = 3;

	[SerializeField]
	private List<Tag> excludeTags;

	[SerializeField]
	private RandomContainer<Tag> tags;

	[SerializeField]
	private RandomContainer<int> qualities;

	[SerializeField]
	private RandomContainer<int> demandAmountRand;

	[SerializeField]
	private RandomContainer<float> demandFactorRand;

	[SerializeField]
	private RandomContainer<int> demandBatchCountRand;

	[SerializeField]
	private RandomContainer<int> supplyAmountRand;

	[SerializeField]
	private RandomContainer<float> supplyFactorRand;

	[SerializeField]
	private RandomContainer<int> supplyBatchCountRand;

	[SerializeField]
	[TimeSpan]
	private long timeToRefresh;

	[SerializeField]
	private int refreshChance;

	private static bool dirty = true;

	private int cachedMaxRefreshChance = -1;

	[DateTime]
	private long lastRefreshedTimeRaw;

	private List<DemandSupplyEntry> demands = new List<DemandSupplyEntry>();

	private List<DemandSupplyEntry> supplies = new List<DemandSupplyEntry>();

	private ReadOnlyCollection<DemandSupplyEntry> _demands_readonly;

	private ReadOnlyCollection<DemandSupplyEntry> _supplies_readonly;

	private const string SaveKey = "BlackMarket_Data";

	public static BlackMarket Instance { get; private set; }

	public int RefreshChance
	{
		get
		{
			return Mathf.Min(refreshChance, MaxRefreshChance);
		}
		set
		{
			refreshChance = value;
			BlackMarket.onRefreshChanceChanged?.Invoke(this);
		}
	}

	public int MaxRefreshChance
	{
		get
		{
			if (dirty)
			{
				OnRequestMaxRefreshChanceEventContext onRequestMaxRefreshChanceEventContext = new OnRequestMaxRefreshChanceEventContext();
				onRequestMaxRefreshChanceEventContext.Add();
				BlackMarket.onRequestMaxRefreshChance?.Invoke(onRequestMaxRefreshChanceEventContext);
				cachedMaxRefreshChance = onRequestMaxRefreshChanceEventContext.Value;
			}
			return cachedMaxRefreshChance;
		}
	}

	private TimeSpan TimeToRefresh
	{
		get
		{
			OnRequestRefreshTimeFactorEventContext onRequestRefreshTimeFactorEventContext = new OnRequestRefreshTimeFactorEventContext();
			BlackMarket.onRequestRefreshTime?.Invoke(onRequestRefreshTimeFactorEventContext);
			float num = Mathf.Max(onRequestRefreshTimeFactorEventContext.Value, 0.01f);
			return TimeSpan.FromTicks((long)((float)timeToRefresh * num));
		}
	}

	private DateTime LastRefreshedTime
	{
		get
		{
			return DateTime.FromBinary(lastRefreshedTimeRaw);
		}
		set
		{
			lastRefreshedTimeRaw = value.ToBinary();
		}
	}

	private TimeSpan TimeSinceLastRefreshedTime
	{
		get
		{
			if (DateTime.UtcNow < LastRefreshedTime)
			{
				LastRefreshedTime = DateTime.UtcNow;
			}
			return DateTime.UtcNow - LastRefreshedTime;
		}
	}

	public TimeSpan RemainingTimeBeforeRefresh => TimeToRefresh - TimeSinceLastRefreshedTime;

	public ReadOnlyCollection<DemandSupplyEntry> Demands
	{
		get
		{
			if (_demands_readonly == null)
			{
				_demands_readonly = new ReadOnlyCollection<DemandSupplyEntry>(demands);
			}
			return _demands_readonly;
		}
	}

	public ReadOnlyCollection<DemandSupplyEntry> Supplies
	{
		get
		{
			if (_supplies_readonly == null)
			{
				_supplies_readonly = new ReadOnlyCollection<DemandSupplyEntry>(supplies);
			}
			return _supplies_readonly;
		}
	}

	public static event Action<BlackMarket> onRefreshChanceChanged;

	public static event Action<OnRequestMaxRefreshChanceEventContext> onRequestMaxRefreshChance;

	public static event Action<OnRequestRefreshTimeFactorEventContext> onRequestRefreshTime;

	public event Action onAfterGenerateEntries;

	public static void NotifyMaxRefreshChanceChanged()
	{
		dirty = true;
	}

	private ItemFilter ContructRandomFilter()
	{
		Tag random = tags.GetRandom();
		int random2 = qualities.GetRandom();
		if (GameMetaData.Instance.IsDemo)
		{
			excludeTags.Add(GameplayDataSettings.Tags.LockInDemoTag);
		}
		return new ItemFilter
		{
			requireTags = new Tag[1] { random },
			excludeTags = excludeTags.ToArray(),
			minQuality = random2,
			maxQuality = random2
		};
	}

	public async UniTask<bool> Buy(DemandSupplyEntry entry)
	{
		if (entry == null)
		{
			return false;
		}
		if (entry.remaining <= 0)
		{
			return false;
		}
		if (!supplies.Contains(entry))
		{
			return false;
		}
		if (!entry.BuyCost.Pay())
		{
			return false;
		}
		await entry.SellCost.Return(directToBuffer: true);
		entry.remaining--;
		entry.NotifyChange();
		return true;
	}

	public async UniTask<bool> Sell(DemandSupplyEntry entry)
	{
		if (entry == null)
		{
			return false;
		}
		if (entry.remaining <= 0)
		{
			return false;
		}
		if (!demands.Contains(entry))
		{
			return false;
		}
		if (!entry.SellCost.Pay())
		{
			return false;
		}
		await entry.BuyCost.Return();
		entry.remaining--;
		entry.NotifyChange();
		return true;
	}

	private void GenerateDemandsAndSupplies()
	{
		demands.Clear();
		supplies.Clear();
		int num = 0;
		for (int i = 0; i < demandsCount; i++)
		{
			num++;
			if (num > 100)
			{
				Debug.LogError("黑市构建需求失败。尝试次数超过100次。");
				break;
			}
			int[] array = ItemAssetsCollection.Search(ContructRandomFilter());
			if (array.Length == 0)
			{
				i--;
				continue;
			}
			int random = array.GetRandom();
			ItemAssetsCollection.GetMetaData(random);
			int random2 = demandAmountRand.GetRandom();
			float random3 = demandFactorRand.GetRandom();
			int random4 = demandBatchCountRand.GetRandom();
			DemandSupplyEntry item = new DemandSupplyEntry
			{
				itemID = random,
				remaining = random2,
				priceFactor = random3,
				batchCount = random4
			};
			demands.Add(item);
		}
		num = 0;
		for (int j = 0; j < suppliesCount; j++)
		{
			num++;
			if (num > 100)
			{
				Debug.LogError("黑市构建供应失败。尝试次数超过100次。");
				break;
			}
			int[] array2 = ItemAssetsCollection.Search(ContructRandomFilter());
			if (array2.Length == 0)
			{
				j--;
				continue;
			}
			int candidate = array2.GetRandom();
			if (demands.Any((DemandSupplyEntry e) => e.ItemID == candidate))
			{
				j--;
				continue;
			}
			ItemAssetsCollection.GetMetaData(candidate);
			int random5 = supplyAmountRand.GetRandom();
			float random6 = supplyFactorRand.GetRandom();
			int random7 = supplyBatchCountRand.GetRandom();
			DemandSupplyEntry item2 = new DemandSupplyEntry
			{
				itemID = candidate,
				remaining = random5,
				priceFactor = random6,
				batchCount = random7
			};
			supplies.Add(item2);
		}
		this.onAfterGenerateEntries?.Invoke();
		if (LevelManager.LevelInited)
		{
			LevelManager.Instance.SaveMainCharacter();
			SavesSystem.CollectSaveData();
			SavesSystem.SaveFile();
		}
	}

	public void PayAndRegenerate()
	{
		if (RefreshChance > 0)
		{
			RefreshChance--;
			GenerateDemandsAndSupplies();
		}
	}

	private void FixedUpdate()
	{
		if (RefreshChance >= MaxRefreshChance)
		{
			LastRefreshedTime = DateTime.UtcNow;
			return;
		}
		TimeSpan timeSinceLastRefreshedTime = TimeSinceLastRefreshedTime;
		if (!(timeSinceLastRefreshedTime > TimeToRefresh))
		{
			return;
		}
		while (timeSinceLastRefreshedTime > TimeToRefresh)
		{
			timeSinceLastRefreshedTime -= TimeToRefresh;
			RefreshChance++;
			if (RefreshChance >= MaxRefreshChance)
			{
				break;
			}
		}
		if (timeSinceLastRefreshedTime > TimeSpan.Zero)
		{
			LastRefreshedTime = DateTime.UtcNow - timeSinceLastRefreshedTime;
		}
	}

	private void Awake()
	{
		Instance = this;
		SavesSystem.OnCollectSaveData += Save;
	}

	private void Start()
	{
		Load();
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= Save;
	}

	private void Save()
	{
		SaveData value = new SaveData(this);
		SavesSystem.Save("BlackMarket_Data", value);
	}

	private void Load()
	{
		SaveData saveData = SavesSystem.Load<SaveData>("BlackMarket_Data");
		if (!saveData.valid)
		{
			GenerateDemandsAndSupplies();
			return;
		}
		demands.Clear();
		demands.AddRange(saveData.demands);
		supplies.Clear();
		supplies.AddRange(saveData.supplies);
		lastRefreshedTimeRaw = saveData.lastRefreshedTimeRaw;
		refreshChance = saveData.refreshChance;
	}
}
