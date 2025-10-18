using System.Collections.Generic;
using System.Linq;
using Duckov.Utilities;
using UnityEngine;

namespace Duckov.Crops;

[CreateAssetMenu]
public class CropDatabase : ScriptableObject
{
	[SerializeField]
	public List<CropInfo> entries = new List<CropInfo>();

	[SerializeField]
	public List<SeedInfo> seedInfos = new List<SeedInfo>();

	public static CropDatabase Instance => GameplayDataSettings.CropDatabase;

	public static CropInfo? GetCropInfo(string id)
	{
		CropDatabase instance = Instance;
		for (int i = 0; i < instance.entries.Count; i++)
		{
			CropInfo value = instance.entries[i];
			if (value.id == id)
			{
				return value;
			}
		}
		return null;
	}

	internal static bool IsIdValid(string id)
	{
		if (Instance == null)
		{
			return false;
		}
		return Instance.entries.Any((CropInfo e) => e.id == id);
	}

	internal static bool IsSeed(int itemTypeID)
	{
		if (Instance == null)
		{
			return false;
		}
		return Instance.seedInfos.Any((SeedInfo e) => e.itemTypeID == itemTypeID);
	}

	internal static SeedInfo GetSeedInfo(int seedItemTypeID)
	{
		if (Instance == null)
		{
			return default(SeedInfo);
		}
		return Instance.seedInfos.FirstOrDefault((SeedInfo e) => e.itemTypeID == seedItemTypeID);
	}
}
