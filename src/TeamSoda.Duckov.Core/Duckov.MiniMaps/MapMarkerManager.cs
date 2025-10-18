using System;
using System.Collections.Generic;
using Duckov.Scenes;
using Saves;
using UnityEngine;

namespace Duckov.MiniMaps;

public class MapMarkerManager : MonoBehaviour
{
	[Serializable]
	private struct SaveData
	{
		public string mainSceneName;

		public List<MapMarkerPOI.RuntimeData> pois;
	}

	[SerializeField]
	private List<Sprite> icons = new List<Sprite>();

	[SerializeField]
	private MapMarkerPOI markerPrefab;

	[SerializeField]
	private int selectedIconIndex;

	[SerializeField]
	private Color selectedColor = Color.white;

	public static Action<int> OnIconChanged;

	public static Action<Color> OnColorChanged;

	private bool loaded;

	private List<MapMarkerPOI> pois = new List<MapMarkerPOI>();

	public static MapMarkerManager Instance { get; private set; }

	public static int SelectedIconIndex
	{
		get
		{
			if (Instance == null)
			{
				return 0;
			}
			return Instance.selectedIconIndex;
		}
	}

	public static Color SelectedColor
	{
		get
		{
			if (Instance == null)
			{
				return Color.white;
			}
			return Instance.selectedColor;
		}
	}

	public static Sprite SelectedIcon
	{
		get
		{
			if (Instance == null)
			{
				return null;
			}
			if (Instance.icons.Count <= SelectedIconIndex)
			{
				return null;
			}
			return Instance.icons[SelectedIconIndex];
		}
	}

	public static string SelectedIconName
	{
		get
		{
			if (Instance == null)
			{
				return null;
			}
			Sprite selectedIcon = SelectedIcon;
			if (selectedIcon == null)
			{
				return null;
			}
			return selectedIcon.name;
		}
	}

	public static List<Sprite> Icons
	{
		get
		{
			if (Instance == null)
			{
				return null;
			}
			return Instance.icons;
		}
	}

	private string SaveKey => "MapMarkerManager_" + MultiSceneCore.MainSceneID;

	private void Awake()
	{
		Instance = this;
		SavesSystem.OnCollectSaveData += OnCollectSaveData;
	}

	private void Start()
	{
		Load();
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= OnCollectSaveData;
	}

	private void Load()
	{
		loaded = true;
		SaveData saveData = SavesSystem.Load<SaveData>(SaveKey);
		if (saveData.pois == null)
		{
			return;
		}
		foreach (MapMarkerPOI.RuntimeData poi in saveData.pois)
		{
			Request(poi);
		}
	}

	private void OnCollectSaveData()
	{
		if (!loaded)
		{
			return;
		}
		SaveData value = new SaveData
		{
			pois = new List<MapMarkerPOI.RuntimeData>()
		};
		foreach (MapMarkerPOI poi in pois)
		{
			if (!(poi == null))
			{
				value.pois.Add(poi.Data);
			}
		}
		SavesSystem.Save(SaveKey, value);
	}

	public static void Request(MapMarkerPOI.RuntimeData data)
	{
		if (!(Instance == null))
		{
			MapMarkerPOI mapMarkerPOI = UnityEngine.Object.Instantiate(Instance.markerPrefab);
			mapMarkerPOI.Setup(data);
			Instance.pois.Add(mapMarkerPOI);
			MultiSceneCore.MoveToMainScene(mapMarkerPOI.gameObject);
		}
	}

	public static void Request(Vector3 worldPos)
	{
		if (!(Instance == null))
		{
			MapMarkerPOI mapMarkerPOI = UnityEngine.Object.Instantiate(Instance.markerPrefab);
			mapMarkerPOI.Setup(worldPos, SelectedIconName, MultiSceneCore.ActiveSubSceneID, SelectedColor);
			Instance.pois.Add(mapMarkerPOI);
			MultiSceneCore.MoveToMainScene(mapMarkerPOI.gameObject);
		}
	}

	public static void Release(MapMarkerPOI entry)
	{
		if (!(entry == null))
		{
			if (Instance != null)
			{
				Instance.pois.Remove(entry);
			}
			if (entry != null)
			{
				UnityEngine.Object.Destroy(entry.gameObject);
			}
		}
	}

	internal static Sprite GetIcon(string iconName)
	{
		if (Instance == null)
		{
			return null;
		}
		if (Instance.icons == null)
		{
			return null;
		}
		return Instance.icons.Find((Sprite e) => e != null && e.name == iconName);
	}

	internal static void SelectColor(Color color)
	{
		if (!(Instance == null))
		{
			Instance.selectedColor = color;
			OnColorChanged?.Invoke(color);
		}
	}

	internal static void SelectIcon(int index)
	{
		if (!(Instance == null))
		{
			Instance.selectedIconIndex = index;
			OnIconChanged?.Invoke(index);
		}
	}
}
