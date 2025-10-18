using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ItemStatsSystem;

[CreateAssetMenu(menuName = "Items/Item Asset Collection")]
public class ItemAssetsCollection : ScriptableObject, ISelfValidator
{
	[Serializable]
	public class Entry : ISelfValidator
	{
		public int typeID;

		public Item prefab;

		public ItemMetaData metaData;

		public void RefreshMetaData()
		{
		}

		public void Validate(SelfValidationResult result)
		{
		}
	}

	public class DynamicEntry
	{
		public int typeID;

		public Item prefab;

		private ItemMetaData? _metaData;

		public ItemMetaData MetaData
		{
			get
			{
				if (prefab == null)
				{
					return default(ItemMetaData);
				}
				if (!_metaData.HasValue)
				{
					_metaData = new ItemMetaData(prefab);
				}
				return _metaData.Value;
			}
		}
	}

	private static ItemAssetsCollection instanceCache;

	private bool editNextTypeID;

	[SerializeField]
	private int nextTypeID;

	public List<Entry> entries;

	private Dictionary<int, Entry> dic;

	private static Dictionary<int, DynamicEntry> dynamicDic = new Dictionary<int, DynamicEntry>();

	private static Dictionary<int, int[]> cachedSearchResults = new Dictionary<int, int[]>();

	public static ItemAssetsCollection Instance
	{
		get
		{
			if ((bool)instanceCache)
			{
				return instanceCache;
			}
			instanceCache = Resources.Load<ItemAssetsCollection>("ItemAssetsCollection");
			return instanceCache;
		}
	}

	public int NextTypeID
	{
		get
		{
			int num = entries.Max((Entry e) => e.typeID);
			if (nextTypeID <= num)
			{
				nextTypeID = num + 1;
			}
			return nextTypeID;
		}
	}

	private static bool TryGetDynamicEntry(int typeID, out DynamicEntry entry)
	{
		if (!dynamicDic.TryGetValue(typeID, out entry))
		{
			return false;
		}
		if (entry == null)
		{
			return false;
		}
		return true;
	}

	public static bool AddDynamicEntry(Item prefab)
	{
		if (prefab == null)
		{
			return false;
		}
		if (Instance == null)
		{
			return false;
		}
		int typeID = prefab.TypeID;
		if (Instance.entries.Any((Entry e) => e != null && e.typeID == typeID))
		{
			Debug.LogWarning($"Warning from Dynamic Item:{typeID}\nDynamic Item Type ID collides with the main game. This will override the main game's item. Please make sure this is intentional, or avoid it.");
		}
		if (TryGetDynamicEntry(typeID, out var _))
		{
			Debug.LogWarning($"Warning from Dynamic Item:{typeID}\nDynamic Item Overwrite detected! May cause some of the mod work incorrectly. Please avoid colliding item type ids.");
		}
		DynamicEntry value = new DynamicEntry
		{
			typeID = typeID,
			prefab = prefab
		};
		dynamicDic[typeID] = value;
		return true;
	}

	public static bool RemoveDynamicEntry(Item prefab)
	{
		if (prefab == null)
		{
			return false;
		}
		if (Instance == null)
		{
			return false;
		}
		int typeID = prefab.TypeID;
		if (!TryGetDynamicEntry(typeID, out var entry))
		{
			return false;
		}
		if (entry.prefab != prefab)
		{
			return false;
		}
		return dynamicDic.Remove(typeID);
	}

	private Entry GetEntry(int typeID)
	{
		if (dic == null)
		{
			dic = new Dictionary<int, Entry>();
			foreach (Entry entry in entries)
			{
				dic[entry.typeID] = entry;
			}
		}
		if (dic.TryGetValue(typeID, out var value))
		{
			return value;
		}
		return null;
	}

	public async UniTask<Item> InstantiateAsync_Local(int typeID)
	{
		if (TryGetDynamicEntry(typeID, out var entry))
		{
			return UnityEngine.Object.Instantiate(entry.prefab);
		}
		Entry entry2 = GetEntry(typeID);
		if (entry2 == null)
		{
			Debug.LogWarning($"在 ItemAssetCollection 中找不到 Item ID:{typeID} 的项目。");
			return null;
		}
		if (entry2.prefab == null)
		{
			Debug.LogWarning($"在 ItemAssetCollection 中未配置 Item ID:{typeID} 的 Asset。");
			return null;
		}
		return UnityEngine.Object.Instantiate(entry2.prefab);
	}

	public static async UniTask<Item> InstantiateAsync(int typeID)
	{
		if (Instance == null)
		{
			Debug.LogError("Instance of ItemAssetsCollection not found");
			return null;
		}
		return await Instance.InstantiateAsync_Local(typeID);
	}

	public static ItemMetaData GetMetaData(int typeID)
	{
		if (TryGetDynamicEntry(typeID, out var entry))
		{
			return entry.MetaData;
		}
		return Instance.GetEntry(typeID)?.metaData ?? default(ItemMetaData);
	}

	public static Item GetPrefab(int typeID)
	{
		return Instance.GetEntry(typeID)?.prefab;
	}

	public void Validate(SelfValidationResult result)
	{
	}

	public void Collect()
	{
	}

	private void SetFolderTag(Item item)
	{
	}

	public void RefreshMeta()
	{
		foreach (Entry entry in entries)
		{
			entry.RefreshMetaData();
		}
	}

	public static int[] GetAllTypeIds(ItemFilter filter)
	{
		if (Instance == null)
		{
			return null;
		}
		bool matchCaliber = !string.IsNullOrEmpty(filter.caliber);
		IEnumerable<int> collection = from e in Instance.entries.FindAll(delegate(Entry entry)
			{
				ItemMetaData metaData = entry.metaData;
				return EvaluateFilter(metaData, filter);
			})
			select e.typeID;
		IEnumerable<int> range = from e in dynamicDic.Where(delegate(KeyValuePair<int, DynamicEntry> e)
			{
				DynamicEntry value = e.Value;
				return !(value.prefab == null) && EvaluateFilter(value.MetaData, filter);
			})
			select e.Key;
		HashSet<int> hashSet = new HashSet<int>(collection);
		hashSet.AddRange(range);
		return hashSet.ToArray();
		bool EvaluateFilter(ItemMetaData meta, ItemFilter itemFilter)
		{
			if (meta.quality < itemFilter.minQuality || meta.quality > itemFilter.maxQuality)
			{
				return false;
			}
			if (matchCaliber && meta.caliber != itemFilter.caliber)
			{
				return false;
			}
			if (itemFilter.requireTags != null)
			{
				Tag[] requireTags = itemFilter.requireTags;
				foreach (Tag requiredTag in requireTags)
				{
					if (!(requiredTag == null) && !meta.tags.Any((Tag tag) => tag != null && tag.Hash == requiredTag.Hash))
					{
						return false;
					}
				}
			}
			if (itemFilter.excludeTags != null)
			{
				Tag[] requireTags = itemFilter.excludeTags;
				foreach (Tag excludeTag in requireTags)
				{
					if (!(excludeTag == null) && meta.tags.Any((Tag tag) => tag != null && tag.Hash == excludeTag.Hash))
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public static int[] Search(ItemFilter filter)
	{
		if (cachedSearchResults.TryGetValue(filter.GetHashCode(), out var value))
		{
			return value;
		}
		int[] result = GetAllTypeIds(filter);
		while (result.Length < 1)
		{
			DownGradeSearch();
			if (filter.maxQuality < 0 || filter.minQuality < 0)
			{
				break;
			}
		}
		cachedSearchResults[filter.GetHashCode()] = result;
		return result;
		void DownGradeSearch()
		{
			int num = Mathf.Min(filter.maxQuality, filter.minQuality) - 1;
			filter.maxQuality = num;
			filter.minQuality = num;
			if (num >= 0)
			{
				result = GetAllTypeIds(filter);
			}
		}
	}

	public static int TryGetIDByName(string name)
	{
		if (Instance == null)
		{
			return -1;
		}
		return Instance.entries.Find((Entry e) => e.metaData.Name == name)?.typeID ?? (-1);
	}
}
