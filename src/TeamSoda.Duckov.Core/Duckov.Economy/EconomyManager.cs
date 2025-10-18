using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using Saves;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.Economy;

public class EconomyManager : MonoBehaviour, ISaveDataProvider
{
	[Serializable]
	public struct SaveData
	{
		public long money;

		public int[] unlockedItems;

		public int[] unlockesWaitingForConfirm;
	}

	[SerializeField]
	private string itemUnlockNotificationTextMainFormat = "物品 {itemDisplayName} 已解锁";

	[SerializeField]
	private string itemUnlockNotificationTextSubFormat = "请在对应商店中查看";

	private const string saveKey = "EconomyData";

	private long money;

	[SerializeField]
	private List<int> unlockedItemIds;

	[SerializeField]
	private List<int> unlockesWaitingForConfirm;

	public const int CashItemID = 451;

	public static string ItemUnlockNotificationTextMainFormat => Instance?.itemUnlockNotificationTextMainFormat;

	public static string ItemUnlockNotificationTextSubFormat => Instance?.itemUnlockNotificationTextSubFormat;

	public static EconomyManager Instance { get; private set; }

	public static long Money
	{
		get
		{
			if (Instance == null)
			{
				return 0L;
			}
			return Instance.money;
		}
		private set
		{
			long arg = Money;
			if (!(Instance == null))
			{
				Instance.money = value;
				EconomyManager.OnMoneyChanged?.Invoke(arg, value);
			}
		}
	}

	public static long Cash => ItemUtilities.GetItemCount(451);

	public ReadOnlyCollection<int> UnlockedItemIds => unlockedItemIds.AsReadOnly();

	public static event Action OnEconomyManagerLoaded;

	public static event Action<long, long> OnMoneyChanged;

	public static event Action<int> OnItemUnlockStateChanged;

	public static event Action<long> OnMoneyPaid;

	public static event Action<Cost> OnCostPaid;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		SavesSystem.OnCollectSaveData += OnCollectSaveData;
		SavesSystem.OnSetFile += OnSetSaveFile;
		Load();
	}

	private void OnCollectSaveData()
	{
		Save();
	}

	private void OnSetSaveFile()
	{
		Load();
	}

	private void Load()
	{
		if (SavesSystem.KeyExisits("EconomyData"))
		{
			SetupSaveData(SavesSystem.Load<SaveData>("EconomyData"));
		}
		try
		{
			EconomyManager.OnEconomyManagerLoaded?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void Save()
	{
		SavesSystem.Save("EconomyData", (SaveData)GenerateSaveData());
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= OnCollectSaveData;
		SavesSystem.OnSetFile -= OnSetSaveFile;
	}

	private static bool Pay(long amount, bool accountAvaliable = true, bool cashAvaliale = true)
	{
		long num = (accountAvaliable ? Money : 0);
		long num2 = (cashAvaliale ? Cash : 0);
		if (num + num2 < amount)
		{
			return false;
		}
		long num3 = amount;
		if (accountAvaliable)
		{
			if (num > amount)
			{
				num3 = 0L;
				Money -= amount;
			}
			else
			{
				num3 -= num;
				Money = 0L;
			}
		}
		if (cashAvaliale && num3 > 0)
		{
			ItemUtilities.ConsumeItems(451, num3);
			num3 = 0L;
		}
		if (amount > 0)
		{
			EconomyManager.OnMoneyPaid?.Invoke(amount);
		}
		return true;
	}

	public static bool Pay(Cost cost, bool accountAvaliable = true, bool cashAvaliale = true)
	{
		if (!IsEnough(cost, accountAvaliable))
		{
			return false;
		}
		if (!Pay(cost.money, accountAvaliable, cashAvaliale))
		{
			return false;
		}
		if (!ItemUtilities.ConsumeItems(cost))
		{
			return false;
		}
		EconomyManager.OnCostPaid?.Invoke(cost);
		return true;
	}

	public static bool IsEnough(Cost cost, bool accountAvaliable = true, bool cashAvaliale = true)
	{
		long num = (accountAvaliable ? Money : 0);
		long num2 = (cashAvaliale ? Cash : 0);
		if (num + num2 < cost.money)
		{
			return false;
		}
		if (cost.items != null)
		{
			Cost.ItemEntry[] items = cost.items;
			for (int i = 0; i < items.Length; i++)
			{
				Cost.ItemEntry itemEntry = items[i];
				if (ItemUtilities.GetItemCount(itemEntry.id) < itemEntry.amount)
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool Add(long amount)
	{
		if (Instance == null)
		{
			return false;
		}
		Money += amount;
		return true;
	}

	public static bool IsWaitingForUnlockConfirm(int itemTypeID)
	{
		if (GameplayDataSettings.Economy.UnlockedItemByDefault.Contains(itemTypeID))
		{
			return false;
		}
		if (Instance == null)
		{
			return false;
		}
		return Instance.unlockesWaitingForConfirm.Contains(itemTypeID);
	}

	public static bool IsUnlocked(int itemTypeID)
	{
		if (GameplayDataSettings.Economy.UnlockedItemByDefault.Contains(itemTypeID))
		{
			return true;
		}
		if (Instance == null)
		{
			return false;
		}
		return Instance.UnlockedItemIds.Contains(itemTypeID);
	}

	public static void Unlock(int itemTypeID, bool needConfirm = true, bool showUI = true)
	{
		if (!(Instance == null) && !Instance.unlockedItemIds.Contains(itemTypeID) && !Instance.unlockesWaitingForConfirm.Contains(itemTypeID))
		{
			if (needConfirm)
			{
				Instance.unlockesWaitingForConfirm.Add(itemTypeID);
			}
			else
			{
				Instance.unlockedItemIds.Add(itemTypeID);
			}
			EconomyManager.OnItemUnlockStateChanged?.Invoke(itemTypeID);
			ItemMetaData metaData = ItemAssetsCollection.GetMetaData(itemTypeID);
			Debug.Log(ItemUnlockNotificationTextMainFormat);
			Debug.Log(metaData.DisplayName);
			if (showUI)
			{
				NotificationText.Push("Notification_StockShoopItemUnlockFormat".ToPlainText().Format(new
				{
					displayName = metaData.DisplayName
				}));
			}
		}
	}

	public static void ConfirmUnlock(int itemTypeID)
	{
		if (!(Instance == null))
		{
			Instance.unlockesWaitingForConfirm.Remove(itemTypeID);
			Instance.unlockedItemIds.Add(itemTypeID);
			EconomyManager.OnItemUnlockStateChanged?.Invoke(itemTypeID);
		}
	}

	public object GenerateSaveData()
	{
		return new SaveData
		{
			money = Money,
			unlockedItems = unlockedItemIds.ToArray(),
			unlockesWaitingForConfirm = unlockesWaitingForConfirm.ToArray()
		};
	}

	public void SetupSaveData(object rawData)
	{
		if (rawData is SaveData saveData)
		{
			money = saveData.money;
			unlockedItemIds.Clear();
			if (saveData.unlockedItems != null)
			{
				unlockedItemIds.AddRange(saveData.unlockedItems);
			}
			unlockesWaitingForConfirm.Clear();
			if (saveData.unlockesWaitingForConfirm != null)
			{
				unlockesWaitingForConfirm.AddRange(saveData.unlockesWaitingForConfirm);
			}
		}
	}
}
