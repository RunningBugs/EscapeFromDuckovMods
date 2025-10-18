using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.Scenes;
using ItemStatsSystem;
using UnityEngine;

namespace Duckov.Utilities;

public class LootBoxLoader : MonoBehaviour
{
	[Serializable]
	private struct Entry
	{
		[ItemTypeID]
		[SerializeField]
		public int itemTypeID;
	}

	public bool autoSetup = true;

	public bool dropOnSpawnItem;

	[SerializeField]
	[Range(0f, 1f)]
	private float activeChance = 1f;

	[SerializeField]
	private int inventorySize = 8;

	[SerializeField]
	private Vector2Int randomCount = new Vector2Int(1, 1);

	public bool randomFromPool;

	[SerializeField]
	private RandomContainer<Tag> tags;

	[SerializeField]
	private List<Tag> excludeTags;

	[SerializeField]
	private RandomContainer<int> qualities;

	[SerializeField]
	private RandomContainer<Entry> randomPool;

	[Range(0f, 1f)]
	public float GenrateCashChance;

	public int maxRandomCash;

	[ItemTypeID]
	[SerializeField]
	private List<int> fixedItems;

	[Range(0f, 1f)]
	[SerializeField]
	private float fixedItemSpawnChance = 1f;

	[SerializeField]
	private InteractableLootbox _lootBox;

	public List<int> FixedItems => fixedItems;

	[SerializeField]
	private Inventory Inventory
	{
		get
		{
			if (_lootBox == null)
			{
				_lootBox = GetComponent<InteractableLootbox>();
				if (_lootBox == null)
				{
					return null;
				}
			}
			return _lootBox.Inventory;
		}
	}

	public void CalculateChances()
	{
		randomPool.RefreshPercent();
	}

	public static int[] Search(ItemFilter filter)
	{
		return ItemAssetsCollection.Search(filter);
	}

	private void Awake()
	{
		if (_lootBox == null)
		{
			_lootBox = GetComponent<InteractableLootbox>();
		}
		RandomActive();
	}

	private int GetKey()
	{
		Vector3 vector = base.transform.position * 10f;
		int x = Mathf.RoundToInt(vector.x);
		int y = Mathf.RoundToInt(vector.y);
		int z = Mathf.RoundToInt(vector.z);
		Vector3Int vector3Int = new Vector3Int(x, y, z);
		return $"LootBoxLoader_{vector3Int}".GetHashCode();
	}

	private void RandomActive()
	{
		bool flag = false;
		int key = GetKey();
		if (MultiSceneCore.Instance.inLevelData.TryGetValue(key, out var value))
		{
			if (value is bool flag2)
			{
				flag = flag2;
			}
		}
		else
		{
			flag = UnityEngine.Random.Range(0f, 1f) < activeChance;
			MultiSceneCore.Instance.inLevelData.Add(key, flag);
		}
		base.gameObject.SetActive(flag);
	}

	public void StartSetup()
	{
		Setup().Forget();
	}

	public async UniTask Setup()
	{
		if (Inventory == null)
		{
			return;
		}
		if (GameMetaData.Instance.IsDemo)
		{
			excludeTags.Add(GameplayDataSettings.Tags.LockInDemoTag);
		}
		if (!excludeTags.Contains(GameplayDataSettings.Tags.Special))
		{
			excludeTags.Add(GameplayDataSettings.Tags.Special);
		}
		if (!LevelManager.Rule.AdvancedDebuffMode && !excludeTags.Contains(GameplayDataSettings.Tags.AdvancedDebuffMode))
		{
			excludeTags.Add(GameplayDataSettings.Tags.AdvancedDebuffMode);
		}
		int num = Mathf.RoundToInt(UnityEngine.Random.Range((float)randomCount.x - 0.5f, (float)randomCount.y + 0.5f) * LevelConfig.Instance.LootboxItemCountMultiplier);
		if (randomCount.x <= 0 && randomCount.y <= 0)
		{
			num = 0;
		}
		List<int> list = new List<int>();
		if (UnityEngine.Random.Range(0f, 1f) < fixedItemSpawnChance && fixedItems.Count > 0)
		{
			list.AddRange(fixedItems);
		}
		if (randomFromPool)
		{
			for (int i = 0; i < num; i++)
			{
				list.Add(randomPool.GetRandom().itemTypeID);
			}
		}
		else
		{
			float lootBoxQualityLowPercent = LevelConfig.Instance.LootBoxQualityLowPercent;
			for (int j = 0; j < num; j++)
			{
				Tag random = tags.GetRandom();
				int random2 = qualities.GetRandom(lootBoxQualityLowPercent);
				int[] array = Search(new ItemFilter
				{
					requireTags = new Tag[1] { random },
					excludeTags = excludeTags.ToArray(),
					minQuality = random2,
					maxQuality = random2
				});
				if (array.Length >= 1)
				{
					int random3 = array.GetRandom();
					list.Add(random3);
				}
			}
		}
		if (inventorySize < list.Count)
		{
			inventorySize = list.Count;
		}
		Inventory.SetCapacity(inventorySize);
		Inventory.Loading = true;
		foreach (int item2 in list)
		{
			if (item2 > 0)
			{
				Item item = await ItemAssetsCollection.InstantiateAsync(item2);
				if (dropOnSpawnItem || !Inventory.AddItem(item))
				{
					item.Drop(base.transform.position + Vector3.up, createRigidbody: true, (UnityEngine.Random.insideUnitSphere + Vector3.up) * 2f, 45f);
				}
			}
		}
		await CreateCash();
		Inventory.Loading = false;
		_lootBox.CheckHideIfEmpty();
	}

	private async UniTask CreateCash()
	{
		if (!(UnityEngine.Random.Range(0f, 1f) > GenrateCashChance))
		{
			int cashCount = UnityEngine.Random.Range(1, maxRandomCash);
			int firstEmptyPosition = Inventory.GetFirstEmptyPosition();
			int capacity = Inventory.Capacity;
			if (firstEmptyPosition >= capacity)
			{
				Inventory.SetCapacity(capacity + 1);
			}
			Item item = await ItemAssetsCollection.InstantiateAsync(GameplayDataSettings.ItemAssets.CashItemTypeID);
			item.StackCount = cashCount;
			if (dropOnSpawnItem || !Inventory.AddItem(item))
			{
				item.Drop(base.transform.position + Vector3.up, createRigidbody: true, (UnityEngine.Random.insideUnitSphere + Vector3.up) * 2f, 45f);
			}
		}
	}

	private void OnValidate()
	{
		tags.RefreshPercent();
		qualities.RefreshPercent();
	}
}
