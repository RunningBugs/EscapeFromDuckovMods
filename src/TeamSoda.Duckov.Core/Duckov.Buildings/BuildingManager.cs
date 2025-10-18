using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Saves;
using Sirenix.Utilities;
using UnityEngine;

namespace Duckov.Buildings;

public class BuildingManager : MonoBehaviour
{
	[Serializable]
	public class BuildingTokenAmountEntry
	{
		public string id;

		public int amount;
	}

	[Serializable]
	public class BuildingAreaData
	{
		[SerializeField]
		private string areaID;

		[SerializeField]
		private List<BuildingData> buildings = new List<BuildingData>();

		public string AreaID => areaID;

		public List<BuildingData> Buildings => buildings;

		public bool Any(string buildingID)
		{
			foreach (BuildingData building in buildings)
			{
				if (building != null)
				{
					if (building.ID == buildingID)
					{
						return true;
					}
					if (building.Info.alternativeFor.Contains(buildingID))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool Add(string buildingID, BuildingRotation rotation, Vector2Int coord, int guid = -1)
		{
			GetBuildingInfo(buildingID);
			if (guid < 0)
			{
				guid = GenerateBuildingGUID(buildingID);
			}
			buildings.Add(new BuildingData(guid, buildingID, rotation, coord));
			return true;
		}

		public bool Remove(int buildingGUID)
		{
			BuildingData buildingData = buildings.Find((BuildingData e) => e != null && e.GUID == buildingGUID);
			if (buildingData != null)
			{
				return buildings.Remove(buildingData);
			}
			return false;
		}

		public bool Remove(BuildingData building)
		{
			return buildings.Remove(building);
		}

		public BuildingData GetBuildingAt(Vector2Int coord)
		{
			foreach (BuildingData building in buildings)
			{
				if (GetOccupyingCoords(building.Dimensions, building.Rotation, building.Coord).Contains(coord))
				{
					return building;
				}
			}
			return null;
		}

		public HashSet<Vector2Int> GetAllOccupiedCoords()
		{
			HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
			foreach (BuildingData building in buildings)
			{
				Vector2Int[] occupyingCoords = GetOccupyingCoords(building.Dimensions, building.Rotation, building.Coord);
				hashSet.AddRange(occupyingCoords);
			}
			return hashSet;
		}

		public bool Collide(Vector2Int dimensions, BuildingRotation rotation, Vector2Int coord)
		{
			HashSet<Vector2Int> allOccupiedCoords = GetAllOccupiedCoords();
			Vector2Int[] occupyingCoords = GetOccupyingCoords(dimensions, rotation, coord);
			foreach (Vector2Int item in occupyingCoords)
			{
				if (allOccupiedCoords.Contains(item))
				{
					return true;
				}
			}
			return false;
		}

		internal bool Any(Func<BuildingData, bool> predicate)
		{
			return buildings.Any(predicate);
		}

		public BuildingAreaData()
		{
		}

		public BuildingAreaData(string areaID)
		{
			this.areaID = areaID;
		}
	}

	[Serializable]
	public class BuildingData
	{
		[SerializeField]
		private int guid;

		[SerializeField]
		private string id;

		[SerializeField]
		private Vector2Int coord;

		[SerializeField]
		private BuildingRotation rotation;

		public int GUID => guid;

		public string ID => id;

		public Vector2Int Dimensions => Info.Dimensions;

		public Vector2Int Coord => coord;

		public BuildingRotation Rotation => rotation;

		public BuildingInfo Info => BuildingDataCollection.GetInfo(id);

		public BuildingData(int guid, string id, BuildingRotation rotation, Vector2Int coord)
		{
			this.guid = guid;
			this.id = id;
			this.coord = coord;
			this.rotation = rotation;
		}

		internal Vector3 GetTransformPosition()
		{
			Vector2Int vector2Int = Dimensions;
			if ((int)rotation % 2 > 0)
			{
				vector2Int = new Vector2Int(vector2Int.y, vector2Int.x);
			}
			return new Vector3((float)coord.x - 0.5f + (float)vector2Int.x / 2f, 0f, (float)coord.y - 0.5f + (float)vector2Int.y / 2f);
		}
	}

	[Serializable]
	private struct SaveData
	{
		[SerializeField]
		public List<BuildingAreaData> data;

		[SerializeField]
		public List<BuildingTokenAmountEntry> tokenAmounts;
	}

	private List<BuildingTokenAmountEntry> tokens = new List<BuildingTokenAmountEntry>();

	[SerializeField]
	private List<BuildingAreaData> areas = new List<BuildingAreaData>();

	private const string SaveKey = "BuildingData";

	private static bool returningBuilding;

	public static BuildingManager Instance { get; private set; }

	public List<BuildingAreaData> Areas => areas;

	public static event Action OnBuildingListChanged;

	public static event Action<int> OnBuildingBuilt;

	public static event Action<int> OnBuildingDestroyed;

	public static event Action<int, BuildingInfo> OnBuildingBuiltComplex;

	public static event Action<int, BuildingInfo> OnBuildingDestroyedComplex;

	private static int GenerateBuildingGUID(string buildingID)
	{
		int result = default(int);
		Regenerate();
		while (Any((BuildingData e) => e != null && e.GUID == result))
		{
			Regenerate();
		}
		return result;
		void Regenerate()
		{
			result = UnityEngine.Random.Range(0, int.MaxValue);
		}
	}

	public int GetTokenAmount(string id)
	{
		return tokens.Find((BuildingTokenAmountEntry e) => e.id == id)?.amount ?? 0;
	}

	private void SetTokenAmount(string id, int amount)
	{
		BuildingTokenAmountEntry buildingTokenAmountEntry = tokens.Find((BuildingTokenAmountEntry e) => e.id == id);
		if (buildingTokenAmountEntry != null)
		{
			buildingTokenAmountEntry.amount = amount;
			return;
		}
		buildingTokenAmountEntry = new BuildingTokenAmountEntry
		{
			id = id,
			amount = amount
		};
		tokens.Add(buildingTokenAmountEntry);
	}

	private void AddToken(string id, int amount = 1)
	{
		BuildingTokenAmountEntry buildingTokenAmountEntry = tokens.Find((BuildingTokenAmountEntry e) => e.id == id);
		if (buildingTokenAmountEntry == null)
		{
			buildingTokenAmountEntry = new BuildingTokenAmountEntry
			{
				id = id,
				amount = 0
			};
			tokens.Add(buildingTokenAmountEntry);
		}
		buildingTokenAmountEntry.amount += amount;
	}

	private bool PayToken(string id)
	{
		BuildingTokenAmountEntry buildingTokenAmountEntry = tokens.Find((BuildingTokenAmountEntry e) => e.id == id);
		if (buildingTokenAmountEntry == null)
		{
			return false;
		}
		if (buildingTokenAmountEntry.amount <= 0)
		{
			return false;
		}
		buildingTokenAmountEntry.amount--;
		return true;
	}

	public static Vector2Int[] GetOccupyingCoords(Vector2Int dimensions, BuildingRotation rotations, Vector2Int coord)
	{
		if ((int)rotations % 2 != 0)
		{
			dimensions = new Vector2Int(dimensions.y, dimensions.x);
		}
		Vector2Int[] array = new Vector2Int[dimensions.x * dimensions.y];
		for (int i = 0; i < dimensions.y; i++)
		{
			for (int j = 0; j < dimensions.x; j++)
			{
				int num = j + dimensions.x * i;
				array[num] = coord + new Vector2Int(j, i);
			}
		}
		return array;
	}

	public BuildingAreaData GetOrCreateArea(string id)
	{
		BuildingAreaData buildingAreaData = areas.Find((BuildingAreaData e) => e != null && e.AreaID == id);
		if (buildingAreaData != null)
		{
			return buildingAreaData;
		}
		BuildingAreaData buildingAreaData2 = new BuildingAreaData(id);
		areas.Add(buildingAreaData2);
		return buildingAreaData2;
	}

	public BuildingAreaData GetArea(string id)
	{
		return areas.Find((BuildingAreaData e) => e != null && e.AreaID == id);
	}

	private void CleanupAndSort()
	{
	}

	public static BuildingInfo GetBuildingInfo(string id)
	{
		return BuildingDataCollection.GetInfo(id);
	}

	public static bool Any(string id, bool includeTokens = false)
	{
		if (Instance == null)
		{
			return false;
		}
		if (includeTokens && Instance.GetTokenAmount(id) > 0)
		{
			return true;
		}
		foreach (BuildingAreaData area in Instance.Areas)
		{
			if (area.Any(id))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Any(Func<BuildingData, bool> predicate)
	{
		if (Instance == null)
		{
			return false;
		}
		foreach (BuildingAreaData area in Instance.Areas)
		{
			if (area.Any(predicate))
			{
				return true;
			}
		}
		return false;
	}

	public static int GetBuildingAmount(string id)
	{
		if (Instance == null)
		{
			return 0;
		}
		int num = 0;
		foreach (BuildingAreaData area in Instance.Areas)
		{
			foreach (BuildingData building in area.Buildings)
			{
				if (building.ID == id)
				{
					num++;
				}
			}
		}
		return num;
	}

	private void Awake()
	{
		Instance = this;
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

	private void Load()
	{
		SaveData saveData = SavesSystem.Load<SaveData>("BuildingData");
		areas.Clear();
		if (saveData.data != null)
		{
			areas.AddRange(saveData.data);
		}
		tokens.Clear();
		if (saveData.tokenAmounts != null)
		{
			tokens.AddRange(saveData.tokenAmounts);
		}
	}

	private void Save()
	{
		SaveData value = new SaveData
		{
			data = new List<BuildingAreaData>(areas),
			tokenAmounts = new List<BuildingTokenAmountEntry>(tokens)
		};
		SavesSystem.Save("BuildingData", value);
	}

	internal static BuildingAreaData GetAreaData(string areaID)
	{
		if (Instance == null)
		{
			return null;
		}
		return Instance.Areas.Find((BuildingAreaData e) => e != null && e.AreaID == areaID);
	}

	internal static BuildingAreaData GetOrCreateAreaData(string areaID)
	{
		if (Instance == null)
		{
			return null;
		}
		return Instance.GetOrCreateArea(areaID);
	}

	internal static BuildingData GetBuildingData(int guid, string areaID = null)
	{
		if (areaID == null)
		{
			foreach (BuildingAreaData area in Instance.Areas)
			{
				BuildingData buildingData = area.Buildings.Find((BuildingData e) => e != null && e.GUID == guid);
				if (buildingData != null)
				{
					return buildingData;
				}
			}
			return null;
		}
		return GetAreaData(areaID)?.Buildings.Find((BuildingData e) => e != null && e.GUID == guid);
	}

	internal static BuildingBuyAndPlaceResults BuyAndPlace(string areaID, string id, Vector2Int coord, BuildingRotation rotation)
	{
		if (Instance == null)
		{
			return BuildingBuyAndPlaceResults.NoReferences;
		}
		BuildingInfo buildingInfo = GetBuildingInfo(id);
		if (!buildingInfo.Valid)
		{
			return BuildingBuyAndPlaceResults.InvalidBuildingInfo;
		}
		GetBuildingAmount(id);
		if (buildingInfo.ReachedAmountLimit)
		{
			return BuildingBuyAndPlaceResults.ReachedAmountLimit;
		}
		Instance.GetTokenAmount(id);
		if (!Instance.PayToken(id) && !buildingInfo.cost.Pay())
		{
			return BuildingBuyAndPlaceResults.PaymentFailure;
		}
		BuildingAreaData orCreateArea = Instance.GetOrCreateArea(areaID);
		int num = GenerateBuildingGUID(id);
		orCreateArea.Add(id, rotation, coord, num);
		BuildingManager.OnBuildingListChanged?.Invoke();
		BuildingManager.OnBuildingBuilt?.Invoke(num);
		BuildingManager.OnBuildingBuiltComplex?.Invoke(num, buildingInfo);
		AudioManager.Post("UI/building_up");
		return BuildingBuyAndPlaceResults.Succeed;
	}

	internal static bool DestroyBuilding(int guid, string areaID = null)
	{
		if (!TryGetBuildingDataAndAreaData(guid, out var buildingData, out var areaData, areaID))
		{
			return false;
		}
		areaData.Remove(buildingData);
		BuildingManager.OnBuildingListChanged?.Invoke();
		BuildingManager.OnBuildingDestroyed?.Invoke(guid);
		BuildingManager.OnBuildingDestroyedComplex?.Invoke(guid, buildingData.Info);
		return true;
	}

	internal static bool TryGetBuildingDataAndAreaData(int guid, out BuildingData buildingData, out BuildingAreaData areaData, string areaID = null)
	{
		buildingData = null;
		areaData = null;
		if (Instance == null)
		{
			return false;
		}
		if (areaID == null)
		{
			foreach (BuildingAreaData area2 in Instance.areas)
			{
				BuildingData buildingData2 = area2.Buildings.Find((BuildingData e) => e != null && e.GUID == guid);
				if (buildingData2 != null)
				{
					areaData = area2;
					buildingData = buildingData2;
					return true;
				}
			}
		}
		else
		{
			BuildingAreaData area = Instance.GetArea(areaID);
			if (area == null)
			{
				return false;
			}
			BuildingData buildingData3 = area.Buildings.Find((BuildingData e) => e != null && e.GUID == guid);
			if (buildingData3 != null)
			{
				areaData = area;
				buildingData = buildingData3;
			}
		}
		return false;
	}

	internal static async UniTask<bool> ReturnBuilding(int guid, string areaID = null)
	{
		if (returningBuilding)
		{
			return false;
		}
		returningBuilding = true;
		if (!TryGetBuildingDataAndAreaData(guid, out var buildingData, out var areaData, areaID))
		{
			return false;
		}
		Instance.AddToken(buildingData.ID);
		areaData.Remove(buildingData);
		BuildingManager.OnBuildingListChanged?.Invoke();
		BuildingManager.OnBuildingDestroyed?.Invoke(guid);
		BuildingManager.OnBuildingDestroyedComplex?.Invoke(guid, buildingData.Info);
		returningBuilding = false;
		return true;
	}

	internal static async UniTask<int> ReturnBuildings(string areaID = null, params int[] buildings)
	{
		int count = 0;
		for (int i = 0; i < buildings.Length; i++)
		{
			if (await ReturnBuilding(buildings[i], areaID))
			{
				count++;
			}
		}
		return count;
	}

	internal static async UniTask<int> ReturnBuildingsOfType(string buildingID, string areaID = null)
	{
		if (Instance == null)
		{
			return 0;
		}
		List<BuildingAreaData> list = new List<BuildingAreaData>();
		if (areaID != null)
		{
			BuildingAreaData area = Instance.GetArea(areaID);
			if (area == null)
			{
				return 0;
			}
			list.Add(area);
		}
		else
		{
			list.AddRange(Instance.Areas);
		}
		returningBuilding = true;
		int num = 0;
		foreach (BuildingAreaData item in list)
		{
			foreach (BuildingData item2 in item.Buildings.FindAll((BuildingData e) => e != null && e.ID == buildingID))
			{
				Instance.AddToken(item2.ID);
				item.Remove(item2);
				BuildingManager.OnBuildingDestroyed?.Invoke(item2.GUID);
				BuildingManager.OnBuildingDestroyedComplex?.Invoke(item2.GUID, item2.Info);
				num++;
			}
		}
		BuildingManager.OnBuildingListChanged?.Invoke();
		returningBuilding = false;
		return num;
	}
}
