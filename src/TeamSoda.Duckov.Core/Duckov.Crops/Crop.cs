using System;
using Cysharp.Threading.Tasks;
using Duckov.Economy;
using UnityEngine;

namespace Duckov.Crops;

public class Crop : MonoBehaviour
{
	public enum CropEvent
	{
		Plant,
		Water,
		Ripen,
		Harvest,
		BeforeDestroy
	}

	[SerializeField]
	private Transform displayParent;

	private Garden garden;

	private bool initialized;

	private CropData data;

	private CropInfo info;

	private GameObject displayInstance;

	public Action<Crop> onPlant;

	public Action<Crop> onWater;

	public Action<Crop> onRipen;

	public Action<Crop> onHarvest;

	public Action<Crop> onBeforeDestroy;

	public CropData Data => data;

	public CropInfo Info => info;

	public float Progress => (float)data.growTicks / (float)info.totalGrowTicks;

	public bool Ripen
	{
		get
		{
			if (!initialized)
			{
				return false;
			}
			return data.growTicks >= info.totalGrowTicks;
		}
	}

	public bool Watered => data.watered;

	public string DisplayName => Info.DisplayName;

	public TimeSpan RemainingTime
	{
		get
		{
			if (!initialized)
			{
				return TimeSpan.Zero;
			}
			long num = info.totalGrowTicks - data.growTicks;
			if (num < 0)
			{
				return TimeSpan.Zero;
			}
			return TimeSpan.FromTicks(num);
		}
	}

	public static event Action<Crop, CropEvent> onCropStatusChange;

	public bool Harvest()
	{
		if (!Ripen)
		{
			return false;
		}
		if (Watered)
		{
			data.score += 50;
		}
		int product = info.GetProduct(data.Ranking);
		if (product <= 0)
		{
			Debug.LogError("Crop product is invalid:\ncrop:" + info.id);
			return false;
		}
		Cost cost = new Cost((product, info.resultAmount));
		cost.Return().Forget();
		DestroyCrop();
		onHarvest?.Invoke(this);
		Crop.onCropStatusChange?.Invoke(this, CropEvent.Harvest);
		return true;
	}

	public void DestroyCrop()
	{
		onBeforeDestroy?.Invoke(this);
		Crop.onCropStatusChange?.Invoke(this, CropEvent.BeforeDestroy);
		garden.Release(this);
	}

	public void InitializeNew(Garden garden, string id, Vector2Int coord)
	{
		CropData cropData = new CropData
		{
			gardenID = garden.GardenID,
			cropID = id,
			coord = coord,
			LastUpdateDateTime = DateTime.Now
		};
		Initialize(garden, cropData);
		onPlant?.Invoke(this);
		Crop.onCropStatusChange?.Invoke(this, CropEvent.Plant);
	}

	public void Initialize(Garden garden, CropData data)
	{
		this.garden = garden;
		string cropID = data.cropID;
		CropInfo? cropInfo = CropDatabase.GetCropInfo(cropID);
		if (!cropInfo.HasValue)
		{
			Debug.LogError("找不到 corpInfo id: " + cropID);
			return;
		}
		info = cropInfo.Value;
		this.data = data;
		RefreshDisplayInstance();
		initialized = true;
		Vector3 localPosition = garden.CoordToLocalPosition(data.coord);
		base.transform.localPosition = localPosition;
	}

	private void RefreshDisplayInstance()
	{
		if (displayInstance != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(displayInstance.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(displayInstance.gameObject);
			}
		}
		if (info.displayPrefab == null)
		{
			Debug.LogError("找不到Display Prefab: " + info.DisplayName);
			return;
		}
		displayInstance = UnityEngine.Object.Instantiate(info.displayPrefab, displayParent);
		displayInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
	}

	public void Water()
	{
		if (!data.watered)
		{
			data.watered = true;
			onWater?.Invoke(this);
			Crop.onCropStatusChange?.Invoke(this, CropEvent.Water);
		}
	}

	private void FixedUpdate()
	{
		Tick();
	}

	private void Tick()
	{
		if (!initialized)
		{
			return;
		}
		TimeSpan timeSpan = DateTime.Now - data.LastUpdateDateTime;
		data.LastUpdateDateTime = DateTime.Now;
		if (data.watered && !Ripen)
		{
			long ticks = timeSpan.Ticks;
			data.growTicks += ticks;
			if (Ripen)
			{
				OnRipen();
			}
		}
	}

	private void OnRipen()
	{
		onRipen?.Invoke(this);
		Crop.onCropStatusChange?.Invoke(this, CropEvent.Ripen);
	}
}
