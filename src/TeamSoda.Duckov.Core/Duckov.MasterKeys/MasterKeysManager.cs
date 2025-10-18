using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Utilities;
using ItemStatsSystem;
using Saves;
using UnityEngine;

namespace Duckov.MasterKeys;

public class MasterKeysManager : MonoBehaviour
{
	[Serializable]
	public class Status
	{
		[ItemTypeID]
		public int id;

		public bool active;
	}

	[SerializeField]
	private List<Status> keysStatus = new List<Status>();

	private static List<int> _cachedKeyItemIds;

	private static string[] excludeTags = new string[1] { "SpecialKey" };

	private const string SaveKey = "MasterKeys";

	public static MasterKeysManager Instance { get; private set; }

	public int Count => keysStatus.Count;

	public static List<int> AllPossibleKeys
	{
		get
		{
			if (_cachedKeyItemIds == null)
			{
				_cachedKeyItemIds = new List<int>();
				foreach (ItemAssetsCollection.Entry entry in ItemAssetsCollection.Instance.entries)
				{
					Tag[] tags = entry.metaData.tags;
					if (tags.Any((Tag e) => Tag.Match(e, "Key")) && (!GameMetaData.Instance.IsDemo || !tags.Any((Tag e) => e.name == GameplayDataSettings.Tags.LockInDemoTag.name)) && !tags.Any((Tag e) => excludeTags.Contains(e.name)))
					{
						_cachedKeyItemIds.Add(entry.typeID);
					}
				}
			}
			return _cachedKeyItemIds;
		}
	}

	public static event Action<int> OnMasterKeyUnlocked;

	public static bool SubmitAndActivate(Item item)
	{
		if (Instance == null)
		{
			return false;
		}
		if (item == null)
		{
			return false;
		}
		int typeID = item.TypeID;
		if (IsActive(typeID))
		{
			return false;
		}
		if (item.StackCount > 1)
		{
			item.StackCount--;
		}
		else
		{
			item.Detach();
			item.DestroyTree();
		}
		Activate(typeID);
		return true;
	}

	public static bool IsActive(int id)
	{
		if (Instance == null)
		{
			return false;
		}
		return Instance.IsActive_Local(id);
	}

	internal static void Activate(int id)
	{
		if (!(Instance == null))
		{
			Instance.Activate_Local(id);
		}
	}

	internal static Status GetStatus(int id)
	{
		if (Instance == null)
		{
			return null;
		}
		return Instance.GetStatus_Local(id);
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		SavesSystem.OnCollectSaveData += OnCollectSaveData;
		Load();
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= OnCollectSaveData;
	}

	private void OnCollectSaveData()
	{
		Save();
	}

	public bool IsActive_Local(int id)
	{
		return GetStatus(id)?.active ?? false;
	}

	private void Activate_Local(int id)
	{
		if (id >= 0 && AllPossibleKeys.Contains(id))
		{
			GetOrCreateStatus(id).active = true;
			MasterKeysManager.OnMasterKeyUnlocked?.Invoke(id);
		}
	}

	public Status GetStatus_Local(int id)
	{
		return keysStatus.Find((Status e) => e.id == id);
	}

	public Status GetOrCreateStatus(int id)
	{
		Status status_Local = GetStatus_Local(id);
		if (status_Local != null)
		{
			return status_Local;
		}
		Status status = new Status();
		status.id = id;
		keysStatus.Add(status);
		return status;
	}

	private void Save()
	{
		SavesSystem.Save("MasterKeys", keysStatus);
	}

	private void Load()
	{
		if (SavesSystem.KeyExisits("MasterKeys"))
		{
			keysStatus = SavesSystem.Load<List<Status>>("MasterKeys");
		}
		else
		{
			keysStatus = new List<Status>();
		}
	}
}
