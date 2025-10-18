using System;
using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using ItemStatsSystem.Data;
using Saves;
using UnityEngine;

namespace Duckov.Bitcoins;

public class BitcoinMiner : MonoBehaviour
{
	[Serializable]
	private struct SaveData
	{
		public ItemTreeData itemData;

		public double work;

		public float cachedPerformance;

		public long lastUpdateDateTimeRaw;
	}

	[SerializeField]
	[ItemTypeID]
	private int minerItemID = 397;

	[SerializeField]
	[ItemTypeID]
	private int coinItemID = 388;

	[SerializeField]
	private double workPerCoin = 1.0;

	private Item item;

	private double work;

	private static readonly double wps_1 = 2.3148148148148147E-05;

	private static readonly double wps_12 = 5.555555555555556E-05;

	private static double? _cached_k;

	[DateTime]
	private long lastUpdateDateTimeRaw;

	private float cachedPerformance;

	public const string SaveKey = "BitcoinMiner_Data";

	private const string PerformaceStatKey = "Performance";

	public static BitcoinMiner Instance { get; private set; }

	private double Progress => work;

	private static double K_1_12
	{
		get
		{
			if (!_cached_k.HasValue)
			{
				_cached_k = (wps_12 - wps_1) / 11.0;
			}
			return _cached_k.Value;
		}
	}

	public double WorkPerSecond
	{
		get
		{
			if (IsInventoryFull)
			{
				return 0.0;
			}
			if (cachedPerformance < 1f)
			{
				return (double)cachedPerformance * wps_1;
			}
			return wps_1 + (double)(cachedPerformance - 1f) * K_1_12;
		}
	}

	public double HoursPerCoin => workPerCoin / 3600.0 / WorkPerSecond;

	public bool IsInventoryFull
	{
		get
		{
			if (item == null)
			{
				return false;
			}
			return item.Inventory.GetFirstEmptyPosition() < 0;
		}
	}

	public TimeSpan TimePerCoin
	{
		get
		{
			if (WorkPerSecond > 0.0)
			{
				return TimeSpan.FromSeconds(workPerCoin / WorkPerSecond);
			}
			return TimeSpan.MaxValue;
		}
	}

	public TimeSpan RemainingTime
	{
		get
		{
			if (WorkPerSecond > 0.0)
			{
				return TimeSpan.FromSeconds((workPerCoin - work) / WorkPerSecond);
			}
			return TimeSpan.MaxValue;
		}
	}

	private DateTime LastUpdateDateTime
	{
		get
		{
			DateTime dateTime = DateTime.FromBinary(lastUpdateDateTimeRaw);
			if (dateTime > DateTime.UtcNow)
			{
				lastUpdateDateTimeRaw = DateTime.UtcNow.ToBinary();
				dateTime = DateTime.UtcNow;
				GameManager.TimeTravelDetected();
			}
			return dateTime;
		}
		set
		{
			lastUpdateDateTimeRaw = value.ToBinary();
		}
	}

	public bool Loading { get; private set; }

	public bool Initialized { get; private set; }

	public bool CreatingCoin { get; private set; }

	public Item Item => item;

	public float NormalizedProgress => (float)(work / workPerCoin);

	public double Performance
	{
		get
		{
			if (Item == null)
			{
				return 0.0;
			}
			return Item.GetStatValue("Performance".GetHashCode());
		}
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("存在多个BitcoinMiner");
			return;
		}
		Instance = this;
		SavesSystem.OnCollectSaveData += Save;
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= Save;
	}

	private void Start()
	{
		Load();
	}

	private async UniTask Setup(SaveData data)
	{
		if (Loading)
		{
			Debug.LogError("已经在加载中");
			return;
		}
		Loading = true;
		item = await ItemTreeData.InstantiateAsync(data.itemData);
		item.transform.SetParent(base.transform);
		work = data.work;
		lastUpdateDateTimeRaw = data.lastUpdateDateTimeRaw;
		cachedPerformance = data.cachedPerformance;
		Loading = false;
		Initialized = true;
	}

	private async UniTask Initialize()
	{
		if (Loading)
		{
			Debug.LogError("已经在加载中");
			return;
		}
		Loading = true;
		item = await ItemAssetsCollection.InstantiateAsync(minerItemID);
		item.transform.SetParent(base.transform);
		work = 0.0;
		cachedPerformance = 0f;
		LastUpdateDateTime = DateTime.UtcNow;
		Loading = false;
		Initialized = true;
	}

	private void Load()
	{
		if (SavesSystem.KeyExisits("BitcoinMiner_Data"))
		{
			SaveData data = SavesSystem.Load<SaveData>("BitcoinMiner_Data");
			Setup(data).Forget();
		}
		else
		{
			Initialize().Forget();
		}
	}

	private void Save()
	{
		if (!Loading && Initialized)
		{
			SaveData value = new SaveData
			{
				itemData = ItemTreeData.FromItem(item),
				work = work,
				lastUpdateDateTimeRaw = lastUpdateDateTimeRaw,
				cachedPerformance = cachedPerformance
			};
			SavesSystem.Save("BitcoinMiner_Data", value);
		}
	}

	private void UpdateWork()
	{
		if (Loading || !Initialized)
		{
			return;
		}
		double totalSeconds = (DateTime.UtcNow - LastUpdateDateTime).TotalSeconds;
		double num = WorkPerSecond * totalSeconds;
		bool isInventoryFull = IsInventoryFull;
		if (work < 0.0)
		{
			work = 0.0;
		}
		work += num;
		if (work >= workPerCoin && !CreatingCoin)
		{
			if (!isInventoryFull)
			{
				CreateCoin().Forget();
			}
			else
			{
				work = workPerCoin;
			}
		}
		cachedPerformance = item.GetStatValue("Performance".GetHashCode());
		LastUpdateDateTime = DateTime.UtcNow;
	}

	private async UniTask CreateCoin()
	{
		if (!CreatingCoin)
		{
			CreatingCoin = true;
			Item item = await ItemAssetsCollection.InstantiateAsync(coinItemID);
			this.item.Inventory.AddAndMerge(item);
			work -= workPerCoin;
			CreatingCoin = false;
		}
	}

	private void FixedUpdate()
	{
		UpdateWork();
	}
}
