using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Duckov.Utilities;
using UnityEngine;

namespace Duckov.Buildings;

[CreateAssetMenu]
public class BuildingDataCollection : ScriptableObject
{
	[SerializeField]
	private List<BuildingInfo> infos = new List<BuildingInfo>();

	[SerializeField]
	private List<Building> prefabs;

	public ReadOnlyCollection<BuildingInfo> readonlyInfos;

	public static BuildingDataCollection Instance => GameplayDataSettings.BuildingDataCollection;

	public ReadOnlyCollection<BuildingInfo> Infos
	{
		get
		{
			if (readonlyInfos == null)
			{
				readonlyInfos = new ReadOnlyCollection<BuildingInfo>(infos);
			}
			return readonlyInfos;
		}
	}

	internal static BuildingInfo GetInfo(string id)
	{
		if (Instance == null)
		{
			return default(BuildingInfo);
		}
		return Instance.infos.FirstOrDefault((BuildingInfo e) => e.id == id);
	}

	internal static Building GetPrefab(string prefabName)
	{
		if (Instance == null)
		{
			return null;
		}
		return Instance.prefabs.FirstOrDefault((Building e) => e != null && e.name == prefabName);
	}
}
