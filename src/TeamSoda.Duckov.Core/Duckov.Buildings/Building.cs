using Drawing;
using Duckov.Utilities;
using SodaCraft.Localizations;
using Unity.Mathematics;
using UnityEngine;

namespace Duckov.Buildings;

public class Building : MonoBehaviour, IDrawGizmos
{
	[SerializeField]
	private string id;

	[SerializeField]
	private Vector2Int dimensions;

	[SerializeField]
	private GameObject graphicsContainer;

	[SerializeField]
	private GameObject functionContainer;

	private BuildingManager.BuildingData data;

	public bool unlockAchievement;

	private GameObject areaMesh;

	private int guid => data.GUID;

	public int GUID => guid;

	public string ID => id;

	public Vector2Int Dimensions => dimensions;

	[LocalizationKey("Default")]
	public string DisplayNameKey
	{
		get
		{
			return "Building_" + ID;
		}
		set
		{
		}
	}

	public string DisplayName => DisplayNameKey.ToPlainText();

	[LocalizationKey("Default")]
	public string DescriptionKey
	{
		get
		{
			return "Building_" + ID + "_Desc";
		}
		set
		{
		}
	}

	public string Description => DescriptionKey.ToPlainText();

	public Vector3 GetOffset(BuildingRotation rotation = BuildingRotation.Zero)
	{
		bool num = (int)rotation % 2 != 0;
		float num2 = (num ? dimensions.y : dimensions.x) - 1;
		float num3 = (num ? dimensions.x : dimensions.y) - 1;
		return new Vector3(num2 / 2f, 0f, num3 / 2f);
	}

	public static string GetDisplayName(string id)
	{
		return ("Building_" + id).ToPlainText();
	}

	private void Awake()
	{
		if (graphicsContainer == null)
		{
			Debug.LogError("建筑" + DisplayName + "未配置 Graphics Container");
			graphicsContainer = base.transform.Find("Graphics")?.gameObject;
		}
		if (functionContainer == null)
		{
			Debug.LogError("建筑" + DisplayName + "未配置 Function Container");
			functionContainer = base.transform.Find("Function")?.gameObject;
		}
		CreateAreaMesh();
	}

	private void CreateAreaMesh()
	{
		if (areaMesh == null)
		{
			areaMesh = Object.Instantiate(GameplayDataSettings.Prefabs.BuildingBlockAreaMesh, base.transform);
			areaMesh.transform.localPosition = Vector3.zero;
			areaMesh.transform.localRotation = quaternion.identity;
			areaMesh.transform.localScale = new Vector3((float)dimensions.x - 0.02f, 1f, (float)dimensions.y - 0.02f);
			areaMesh.transform.SetParent(functionContainer.transform, worldPositionStays: true);
		}
	}

	private void RegisterEvents()
	{
		BuildingManager.OnBuildingDestroyed += OnBuildingDestroyed;
	}

	private void OnBuildingDestroyed(int guid)
	{
		if (guid == GUID)
		{
			Release();
		}
	}

	private void Release()
	{
		Object.Destroy(base.gameObject);
	}

	private void UnregisterEvents()
	{
		BuildingManager.OnBuildingDestroyed -= OnBuildingDestroyed;
	}

	public void DrawGizmos()
	{
		if (!GizmoContext.InSelection(this))
		{
			return;
		}
		using (Draw.WithColor(new Color(1f, 1f, 1f, 0.5f)))
		{
			using (Draw.InLocalSpace(base.transform))
			{
				float3 @float = GetOffset();
				float2 size = new float2(0.9f, 0.9f);
				for (int i = 0; i < Dimensions.y; i++)
				{
					for (int j = 0; j < Dimensions.x; j++)
					{
						Draw.SolidPlane(new float3(j, 0f, i) - @float, Vector3.up, size);
					}
				}
			}
		}
	}

	internal void Setup(BuildingManager.BuildingData data)
	{
		this.data = data;
		base.transform.localRotation = Quaternion.Euler(0f, (int)data.Rotation * 90, 0f);
		RegisterEvents();
	}

	private void OnDestroy()
	{
		UnregisterEvents();
	}

	internal void SetupPreview()
	{
		functionContainer.SetActive(value: false);
		Collider[] componentsInChildren = graphicsContainer.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}
}
