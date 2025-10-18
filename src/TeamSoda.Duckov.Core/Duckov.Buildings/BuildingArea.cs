using System.Collections.Generic;
using Drawing;
using Duckov.Achievements;
using UnityEngine;

namespace Duckov.Buildings;

public class BuildingArea : MonoBehaviour, IDrawGizmos
{
	[SerializeField]
	private string areaID;

	[SerializeField]
	private Vector2Int size;

	[SerializeField]
	private LayerMask physicsCollisionLayers = -1;

	private List<Building> activeBuildings = new List<Building>();

	private int raycastHitCount;

	private RaycastHit[] raycastHitBuffer = new RaycastHit[5];

	public string AreaID => areaID;

	public Vector2Int Size => size;

	public Vector2Int LowerLeftCorner => CenterCoord - (size - Vector2Int.one);

	private Vector2Int CenterCoord => new Vector2Int(Mathf.RoundToInt(base.transform.position.x), Mathf.RoundToInt(base.transform.position.z));

	private int Width => size.x;

	private int Height => size.y;

	public BuildingManager.BuildingAreaData AreaData => BuildingManager.GetOrCreateAreaData(AreaID);

	public Plane Plane => new Plane(base.transform.up, base.transform.position);

	private void Awake()
	{
		BuildingManager.OnBuildingBuilt += OnBuildingBuilt;
	}

	private void OnDestroy()
	{
		BuildingManager.OnBuildingBuilt -= OnBuildingBuilt;
	}

	private void OnBuildingBuilt(int guid)
	{
		BuildingManager.BuildingData buildingData = BuildingManager.GetBuildingData(guid);
		if (buildingData != null)
		{
			Display(buildingData);
		}
	}

	private void Start()
	{
		RepaintAll();
	}

	public void DrawGizmos()
	{
		if (GizmoContext.InSelection(this))
		{
			int num = CenterCoord.x - (size.x - 1);
			int num2 = CenterCoord.x + (size.x - 1) + 1;
			int num3 = CenterCoord.y - (size.y - 1);
			int num4 = CenterCoord.y + (size.y - 1) + 1;
			Vector3 vector = new Vector3(-0.5f, 0f, -0.5f);
			for (int i = num; i <= num2; i++)
			{
				Draw.Line(new Vector3(i, 0f, num3) + vector, new Vector3(i, 0f, num4) + vector);
			}
			for (int j = num3; j <= num4; j++)
			{
				Draw.Line(new Vector3(num, 0f, j) + vector, new Vector3(num2, 0f, j) + vector);
			}
		}
	}

	public bool IsPlacementWithinRange(Vector2Int dimensions, BuildingRotation rotation, Vector2Int coord)
	{
		if ((int)rotation % 2 > 0)
		{
			dimensions = new Vector2Int(dimensions.y, dimensions.x);
		}
		coord -= CenterCoord;
		if (coord.x > -size.x && coord.y > -size.y && coord.x + dimensions.x <= size.x)
		{
			return coord.y + dimensions.y <= size.y;
		}
		return false;
	}

	public Vector2Int CursorToCoord(Vector3 point, Vector2Int dimensions, BuildingRotation rotation)
	{
		if ((int)rotation % 2 > 0)
		{
			dimensions = new Vector2Int(dimensions.y, dimensions.x);
		}
		int x = Mathf.RoundToInt(point.x) - dimensions.x / 2;
		int y = Mathf.RoundToInt(point.z) - dimensions.y / 2;
		return new Vector2Int(x, y);
	}

	private void ReleaseAllBuildings()
	{
		for (int num = activeBuildings.Count - 1; num >= 0; num--)
		{
			Building building = activeBuildings[num];
			if (!(building == null))
			{
				Object.Destroy(building.gameObject);
			}
		}
		activeBuildings.Clear();
	}

	public void RepaintAll()
	{
		ReleaseAllBuildings();
		BuildingManager.BuildingAreaData areaData = AreaData;
		if (areaData == null)
		{
			return;
		}
		foreach (BuildingManager.BuildingData building in areaData.Buildings)
		{
			Display(building);
		}
	}

	private void Display(BuildingManager.BuildingData building)
	{
		if (building == null)
		{
			return;
		}
		Building prefab = building.Info.Prefab;
		if (prefab == null)
		{
			Debug.LogError("No prefab for building " + building.ID);
			return;
		}
		for (int num = activeBuildings.Count - 1; num >= 0; num--)
		{
			Building building2 = activeBuildings[num];
			if (building2 == null)
			{
				activeBuildings.RemoveAt(num);
			}
			else if (building2.GUID == building.GUID)
			{
				Debug.LogError($"重复显示建筑{building.Info.DisplayName}({building.GUID})");
				return;
			}
		}
		Building building3 = Object.Instantiate(prefab, base.transform);
		building3.Setup(building);
		building3.transform.position = building.GetTransformPosition();
		activeBuildings.Add(building3);
		if (building3.unlockAchievement && (bool)AchievementManager.Instance)
		{
			AchievementManager.Instance.Unlock("Building_" + building3.ID.Trim());
		}
	}

	internal Vector3 CoordToWorldPosition(Vector2Int coord, Vector2Int dimensions, BuildingRotation rotation)
	{
		if ((int)rotation % 2 > 0)
		{
			dimensions = new Vector2Int(dimensions.y, dimensions.x);
		}
		return new Vector3((float)coord.x - 0.5f + (float)dimensions.x / 2f, 0f, (float)coord.y - 0.5f + (float)dimensions.y / 2f);
	}

	internal bool PhysicsCollide(Vector2Int dimensions, BuildingRotation rotation, Vector2Int coord, float castBeginHeight = 0f, float castHeight = 2f)
	{
		if ((int)rotation % 2 != 0)
		{
			dimensions = new Vector2Int(dimensions.y, dimensions.x);
		}
		raycastHitCount = 0;
		for (int i = coord.y; i < coord.y + dimensions.y; i++)
		{
			for (int j = coord.x; j < coord.x + dimensions.x; j++)
			{
				Vector3 vector = new Vector3(j, castBeginHeight, i);
				raycastHitCount += Physics.RaycastNonAlloc(vector, Vector3.up, raycastHitBuffer, castHeight, physicsCollisionLayers);
				raycastHitCount += Physics.RaycastNonAlloc(vector + Vector3.up * castHeight, Vector3.down, raycastHitBuffer, castHeight, physicsCollisionLayers);
				if (raycastHitCount > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal Building GetBuildingInstanceAt(Vector2Int coord)
	{
		BuildingManager.BuildingData buildingData = AreaData.GetBuildingAt(coord);
		if (buildingData == null)
		{
			return null;
		}
		return activeBuildings.Find((Building e) => e != null && e.GUID == buildingData.GUID);
	}
}
