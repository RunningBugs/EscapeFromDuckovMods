using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Buildings;
using Duckov.Buildings.UI;
using Duckov.Economy;
using Duckov.Quests;
using Duckov.UI;
using Saves;
using UnityEngine;

public class ItemWishlist : MonoBehaviour
{
	public struct WishlistInfo
	{
		public int itemTypeID;

		public bool isManuallyWishlisted;

		public bool isQuestRequired;

		public bool isBuildingRequired;
	}

	private List<int> manualWishList = new List<int>();

	private HashSet<int> _questRequiredItems = new HashSet<int>();

	private HashSet<int> _buildingRequiredItems = new HashSet<int>();

	private const string SaveKey = "ItemWishlist_Manual";

	public static ItemWishlist Instance { get; private set; }

	public static event Action<int> OnWishlistChanged;

	private void Awake()
	{
		Instance = this;
		QuestManager.onQuestListsChanged += OnQuestListChanged;
		BuildingManager.OnBuildingListChanged += OnBuildingListChanged;
		SavesSystem.OnCollectSaveData += Save;
		UIInputManager.OnWishlistHoveringItem += OnWishlistHoveringItem;
		Load();
	}

	private void OnDestroy()
	{
		QuestManager.onQuestListsChanged -= OnQuestListChanged;
		SavesSystem.OnCollectSaveData -= Save;
		UIInputManager.OnWishlistHoveringItem -= OnWishlistHoveringItem;
	}

	private void OnWishlistHoveringItem(UIInputEventData data)
	{
		if (ItemHoveringUI.Shown)
		{
			int displayingItemID = ItemHoveringUI.DisplayingItemID;
			if (IsManuallyWishlisted(displayingItemID))
			{
				RemoveFromWishlist(displayingItemID);
			}
			else
			{
				AddToWishList(displayingItemID);
			}
			ItemHoveringUI.NotifyRefreshWishlistInfo();
		}
	}

	private void Load()
	{
		manualWishList.Clear();
		List<int> list = SavesSystem.Load<List<int>>("ItemWishlist_Manual");
		if (list != null)
		{
			manualWishList.AddRange(list);
		}
	}

	private void Save()
	{
		SavesSystem.Save("ItemWishlist_Manual", manualWishList);
	}

	private void Start()
	{
		CacheQuestItems();
		CacheBuildingItems();
	}

	private void OnQuestListChanged(QuestManager obj)
	{
		CacheQuestItems();
	}

	private void OnBuildingListChanged()
	{
		CacheBuildingItems();
	}

	private void CacheQuestItems()
	{
		_questRequiredItems = QuestManager.GetAllRequiredItems().ToHashSet();
	}

	private void CacheBuildingItems()
	{
		_buildingRequiredItems.Clear();
		BuildingInfo[] buildingsToDisplay = BuildingSelectionPanel.GetBuildingsToDisplay();
		for (int i = 0; i < buildingsToDisplay.Length; i++)
		{
			BuildingInfo buildingInfo = buildingsToDisplay[i];
			if (buildingInfo.RequirementsSatisfied() && buildingInfo.TokenAmount + buildingInfo.CurrentAmount < buildingInfo.maxAmount)
			{
				Cost.ItemEntry[] items = buildingInfo.cost.items;
				for (int j = 0; j < items.Length; j++)
				{
					Cost.ItemEntry itemEntry = items[j];
					_buildingRequiredItems.Add(itemEntry.id);
				}
			}
		}
	}

	private IEnumerable<int> IterateAll()
	{
		foreach (int manualWish in manualWishList)
		{
			yield return manualWish;
		}
		IEnumerable<int> allRequiredItems = QuestManager.GetAllRequiredItems();
		foreach (int item in allRequiredItems)
		{
			yield return item;
		}
	}

	public bool IsQuestRequired(int itemTypeID)
	{
		return _questRequiredItems.Contains(itemTypeID);
	}

	public bool IsManuallyWishlisted(int itemTypeID)
	{
		return manualWishList.Contains(itemTypeID);
	}

	public bool IsBuildingRequired(int itemTypeID)
	{
		return _buildingRequiredItems.Contains(itemTypeID);
	}

	public static void AddToWishList(int itemTypeID)
	{
		if (!(Instance == null) && !Instance.manualWishList.Contains(itemTypeID))
		{
			Instance.manualWishList.Add(itemTypeID);
			ItemWishlist.OnWishlistChanged?.Invoke(itemTypeID);
		}
	}

	public static bool RemoveFromWishlist(int itemTypeID)
	{
		if (Instance == null)
		{
			return false;
		}
		bool num = Instance.manualWishList.Remove(itemTypeID);
		if (num)
		{
			Action<int> onWishlistChanged = ItemWishlist.OnWishlistChanged;
			if (onWishlistChanged == null)
			{
				return num;
			}
			onWishlistChanged(itemTypeID);
		}
		return num;
	}

	public static WishlistInfo GetWishlistInfo(int itemTypeID)
	{
		if (Instance == null)
		{
			return default(WishlistInfo);
		}
		bool isManuallyWishlisted = Instance.IsManuallyWishlisted(itemTypeID);
		bool isQuestRequired = Instance.IsQuestRequired(itemTypeID);
		bool isBuildingRequired = Instance.IsBuildingRequired(itemTypeID);
		return new WishlistInfo
		{
			itemTypeID = itemTypeID,
			isManuallyWishlisted = isManuallyWishlisted,
			isQuestRequired = isQuestRequired,
			isBuildingRequired = isBuildingRequired
		};
	}
}
