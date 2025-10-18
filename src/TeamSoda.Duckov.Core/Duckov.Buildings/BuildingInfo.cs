using System;
using Duckov.Economy;
using Duckov.Quests;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.Buildings;

[Serializable]
public struct BuildingInfo
{
	public string id;

	public string prefabName;

	public int maxAmount;

	public Cost cost;

	public string[] requireBuildings;

	public string[] alternativeFor;

	public int[] requireQuests;

	public Sprite iconReference;

	public bool Valid => !string.IsNullOrEmpty(id);

	public Building Prefab => BuildingDataCollection.GetPrefab(prefabName);

	public Vector2Int Dimensions
	{
		get
		{
			if (!Prefab)
			{
				return default(Vector2Int);
			}
			return Prefab.Dimensions;
		}
	}

	[LocalizationKey("Default")]
	public string DisplayNameKey => "Building_" + id;

	public string DisplayName => DisplayNameKey.ToPlainText();

	[LocalizationKey("Default")]
	public string DescriptionKey => "Building_" + id + "_Desc";

	public string Description => DescriptionKey.ToPlainText();

	public int CurrentAmount
	{
		get
		{
			if (BuildingManager.Instance == null)
			{
				return 0;
			}
			return BuildingManager.GetBuildingAmount(id);
		}
	}

	public bool ReachedAmountLimit
	{
		get
		{
			if (maxAmount <= 0)
			{
				return false;
			}
			return CurrentAmount >= maxAmount;
		}
	}

	public int TokenAmount
	{
		get
		{
			if (BuildingManager.Instance == null)
			{
				return 0;
			}
			return BuildingManager.Instance.GetTokenAmount(id);
		}
	}

	public static string GetDisplayName(string id)
	{
		return ("Building_" + id).ToPlainText();
	}

	internal bool RequirementsSatisfied()
	{
		string[] array = requireBuildings;
		for (int i = 0; i < array.Length; i++)
		{
			if (!BuildingManager.Any(array[i]))
			{
				return false;
			}
		}
		if (!QuestManager.AreQuestFinished(requireQuests))
		{
			return false;
		}
		return true;
	}
}
