using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Utilities;
using Saves;
using UnityEngine;

namespace Duckov.Crops;

public class Garden : MonoBehaviour
{
	[Serializable]
	private class SaveData
	{
		[SerializeField]
		public List<CropData> crops;

		public SaveData(Garden garden)
		{
			crops = new List<CropData>();
			foreach (Crop value in garden.dictioanry.Values)
			{
				if (!(value == null))
				{
					crops.Add(value.Data);
				}
			}
		}
	}

	[SerializeField]
	private string gardenID = "Default";

	public static List<IGardenSizeAdder> sizeAdders = new List<IGardenSizeAdder>();

	public static List<IGardenAutoWaterProvider> autoWaters = new List<IGardenAutoWaterProvider>();

	public static Dictionary<string, Garden> gardens = new Dictionary<string, Garden>();

	[SerializeField]
	private Grid grid;

	[SerializeField]
	private Crop cropTemplate;

	[SerializeField]
	private Transform border00;

	[SerializeField]
	private Transform border01;

	[SerializeField]
	private Transform border11;

	[SerializeField]
	private Transform border10;

	[SerializeField]
	private Transform corner00;

	[SerializeField]
	private Transform corner01;

	[SerializeField]
	private Transform corner11;

	[SerializeField]
	private Transform corner10;

	[SerializeField]
	private BoxCollider interactBox;

	[SerializeField]
	private Vector2Int size;

	[SerializeField]
	private bool autoWater;

	public Vector3 cameraRigCenter = new Vector3(3f, 0f, 3f);

	private bool sizeDirty;

	[SerializeField]
	private CellDisplay cellDisplayTemplate;

	private PrefabPool<CellDisplay> _cellPool;

	private Dictionary<Vector2Int, Crop> dictioanry = new Dictionary<Vector2Int, Crop>();

	public string GardenID => gardenID;

	public string SaveKey => "Garden_" + gardenID;

	public bool AutoWater
	{
		get
		{
			return autoWater;
		}
		set
		{
			autoWater = value;
			if (value)
			{
				WaterAll();
			}
		}
	}

	public Vector2Int Size
	{
		get
		{
			return size;
		}
		set
		{
			size = value;
			sizeDirty = true;
		}
	}

	public PrefabPool<CellDisplay> CellPool
	{
		get
		{
			if (_cellPool == null)
			{
				_cellPool = new PrefabPool<CellDisplay>(cellDisplayTemplate);
			}
			return _cellPool;
		}
	}

	public Crop this[Vector2Int coord]
	{
		get
		{
			if (dictioanry.TryGetValue(coord, out var value))
			{
				return value;
			}
			return null;
		}
		private set
		{
			dictioanry[coord] = value;
		}
	}

	public static event Action OnSizeAddersChanged;

	public static event Action OnAutoWatersChanged;

	private void WaterAll()
	{
		foreach (Crop value in dictioanry.Values)
		{
			if (!(value == null) && !value.Watered)
			{
				value.Water();
			}
		}
	}

	private void Awake()
	{
		gardens[gardenID] = this;
		SavesSystem.OnCollectSaveData += Save;
		OnSizeAddersChanged += RefreshSize;
		OnAutoWatersChanged += RefreshAutowater;
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= Save;
		OnSizeAddersChanged -= RefreshSize;
		OnAutoWatersChanged -= RefreshAutowater;
	}

	private void Start()
	{
		RegenerateCellDisplays();
		Load();
		RefreshSize();
		RefreshAutowater();
	}

	private void FixedUpdate()
	{
		if (sizeDirty)
		{
			RegenerateCellDisplays();
		}
	}

	private void RefreshAutowater()
	{
		bool flag = false;
		foreach (IGardenAutoWaterProvider autoWater in autoWaters)
		{
			if (autoWater.TakeEffect(gardenID))
			{
				flag = true;
				break;
			}
		}
		if (flag != AutoWater)
		{
			AutoWater = flag;
		}
	}

	private void RefreshSize()
	{
		Vector2Int zero = Vector2Int.zero;
		foreach (IGardenSizeAdder sizeAdder in sizeAdders)
		{
			if (sizeAdder != null)
			{
				zero += sizeAdder.GetValue(gardenID);
			}
		}
		Size = new Vector2Int(3 + zero.x, 3 + zero.y);
	}

	public void SetSize(int x, int y)
	{
		RegenerateCellDisplays();
	}

	private void RegenerateCellDisplays()
	{
		sizeDirty = false;
		CellPool.ReleaseAll();
		Vector2Int vector2Int = Size;
		for (int i = 0; i < vector2Int.y; i++)
		{
			for (int j = 0; j < vector2Int.x; j++)
			{
				Vector3 localPosition = CoordToLocalPosition(new Vector2Int(j, i));
				CellDisplay cellDisplay = CellPool.Get();
				cellDisplay.transform.localPosition = localPosition;
				cellDisplay.Setup(this, j, i);
			}
		}
		Vector3 vector = CoordToLocalPosition(new Vector2Int(0, 0)) - new Vector3(grid.cellSize.x, 0f, grid.cellSize.y) / 2f;
		Vector3 vector2 = CoordToLocalPosition(new Vector2Int(vector2Int.x, vector2Int.y)) - new Vector3(grid.cellSize.x, 0f, grid.cellSize.y) / 2f;
		float num = vector2.x - vector.x;
		float num2 = vector2.z - vector.z;
		Vector3 localPosition2 = vector;
		Vector3 localPosition3 = new Vector3(vector.x, 0f, vector2.z);
		Vector3 localPosition4 = vector2;
		Vector3 localPosition5 = new Vector3(vector2.x, 0f, vector.z);
		Vector3 localScale = new Vector3(1f, 1f, num2);
		Vector3 localScale2 = new Vector3(1f, 1f, num);
		Vector3 localScale3 = new Vector3(1f, 1f, num2);
		Vector3 localScale4 = new Vector3(1f, 1f, num);
		border00.localPosition = localPosition2;
		border01.localPosition = localPosition3;
		border11.localPosition = localPosition4;
		border10.localPosition = localPosition5;
		corner00.localPosition = localPosition2;
		corner01.localPosition = localPosition3;
		corner11.localPosition = localPosition4;
		corner10.localPosition = localPosition5;
		border00.localScale = localScale;
		border01.localScale = localScale2;
		border11.localScale = localScale3;
		border10.localScale = localScale4;
		border00.localRotation = Quaternion.Euler(0f, 0f, 0f);
		border01.localRotation = Quaternion.Euler(0f, 90f, 0f);
		border11.localRotation = Quaternion.Euler(0f, 180f, 0f);
		border10.localRotation = Quaternion.Euler(0f, 270f, 0f);
		Vector3 localPosition6 = (vector + vector2) / 2f;
		interactBox.transform.localPosition = localPosition6;
		interactBox.center = Vector3.zero;
		interactBox.size = new Vector3(num + 0.5f, 1f, num2 + 0.5f);
	}

	private Crop CreateCropInstance(string id)
	{
		return UnityEngine.Object.Instantiate(cropTemplate, base.transform);
	}

	public void Save()
	{
		if (LevelManager.LevelInited)
		{
			SaveData value = new SaveData(this);
			SavesSystem.Save(SaveKey, value);
		}
	}

	public void Load()
	{
		Clear();
		dictioanry.Clear();
		SaveData saveData = SavesSystem.Load<SaveData>(SaveKey);
		if (saveData == null)
		{
			return;
		}
		foreach (CropData crop2 in saveData.crops)
		{
			Crop crop = CreateCropInstance(crop2.cropID);
			crop.Initialize(this, crop2);
			this[crop2.coord] = crop;
		}
	}

	private void Clear()
	{
		foreach (Crop item in dictioanry.Values.ToList())
		{
			if (!(item == null))
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
	}

	public bool IsCoordValid(Vector2Int coord)
	{
		Vector2Int vector2Int = Size;
		if (vector2Int.x <= 0 || vector2Int.y <= 0)
		{
			return true;
		}
		if (coord.x < vector2Int.x && coord.y < vector2Int.y && coord.x >= 0)
		{
			return coord.y >= 0;
		}
		return false;
	}

	public bool IsCoordOccupied(Vector2Int coord)
	{
		return this[coord] != null;
	}

	public bool Plant(Vector2Int coord, string cropID)
	{
		if (!IsCoordValid(coord))
		{
			return false;
		}
		if (IsCoordOccupied(coord))
		{
			return false;
		}
		if (!CropDatabase.IsIdValid(cropID))
		{
			Debug.Log("[Garden] Invalid crop id " + cropID, this);
			return false;
		}
		Crop crop = CreateCropInstance(cropID);
		crop.InitializeNew(this, cropID, coord);
		this[coord] = crop;
		if (autoWater)
		{
			crop.Water();
		}
		return true;
	}

	public void Water(Vector2Int coord)
	{
		Crop crop = this[coord];
		if (!(crop == null))
		{
			crop.Water();
		}
	}

	public Vector3 CoordToWorldPosition(Vector2Int coord)
	{
		Vector3 position = CoordToLocalPosition(coord);
		return base.transform.TransformPoint(position);
	}

	public Vector3 CoordToLocalPosition(Vector2Int coord)
	{
		Vector3 cellCenterLocal = grid.GetCellCenterLocal((Vector3Int)coord);
		float z = grid.cellSize.z;
		float y = cellCenterLocal.y - z / 2f;
		Vector3 result = cellCenterLocal;
		result.y = y;
		return result;
	}

	public Vector2Int WorldPositionToCoord(Vector3 wPos)
	{
		Vector3 worldPosition = wPos + Vector3.up * 0.1f * grid.cellSize.z;
		return (Vector2Int)grid.WorldToCell(worldPosition);
	}

	internal void Release(Crop crop)
	{
		UnityEngine.Object.Destroy(crop.gameObject);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		float x = grid.cellSize.x;
		float y = grid.cellSize.y;
		Vector2Int vector2Int = Size;
		for (int i = 0; i <= vector2Int.x; i++)
		{
			Vector3 vector = Vector3.right * i * x;
			Vector3 to = vector + Vector3.forward * vector2Int.y * y;
			Gizmos.DrawLine(vector, to);
		}
		for (int j = 0; j <= vector2Int.y; j++)
		{
			Vector3 vector2 = Vector3.forward * j * y;
			Vector3 to2 = vector2 + Vector3.right * vector2Int.x * x;
			Gizmos.DrawLine(vector2, to2);
		}
	}

	internal static void Register(IGardenSizeAdder obj)
	{
		sizeAdders.Add(obj);
		Garden.OnSizeAddersChanged?.Invoke();
	}

	internal static void Register(IGardenAutoWaterProvider obj)
	{
		autoWaters.Add(obj);
		Garden.OnAutoWatersChanged?.Invoke();
	}

	internal static void Unregister(IGardenSizeAdder obj)
	{
		sizeAdders.Remove(obj);
		Garden.OnSizeAddersChanged?.Invoke();
	}

	internal static void Unregister(IGardenAutoWaterProvider obj)
	{
		autoWaters.Remove(obj);
		Garden.OnAutoWatersChanged?.Invoke();
	}
}
