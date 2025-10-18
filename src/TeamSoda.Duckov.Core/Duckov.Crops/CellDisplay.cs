using System;
using UnityEngine;

namespace Duckov.Crops;

public class CellDisplay : MonoBehaviour
{
	[Serializable]
	private struct GraphicsStyle
	{
		public Color color;

		public float smoothness;
	}

	[SerializeField]
	private Renderer renderer;

	[SerializeField]
	private GraphicsStyle styleDry;

	[SerializeField]
	private GraphicsStyle styleWatered;

	private Garden garden;

	private Vector2Int coord;

	private MaterialPropertyBlock propertyBlock;

	internal void Setup(Garden garden, int coordx, int coordy)
	{
		this.garden = garden;
		coord = new Vector2Int(coordx, coordy);
		bool watered = false;
		Crop crop = garden[coord];
		if (crop != null)
		{
			watered = crop.Watered;
		}
		RefreshGraphics(watered);
	}

	private void OnEnable()
	{
		Crop.onCropStatusChange += HandleCropEvent;
	}

	private void OnDisable()
	{
		Crop.onCropStatusChange -= HandleCropEvent;
	}

	private void HandleCropEvent(Crop crop, Crop.CropEvent e)
	{
		if (!(crop == null) && !(garden == null))
		{
			CropData data = crop.Data;
			if (!(data.gardenID != garden.GardenID) && !(data.coord != coord))
			{
				RefreshGraphics(crop.Watered && e != Crop.CropEvent.BeforeDestroy && e != Crop.CropEvent.Harvest);
			}
		}
	}

	private void RefreshGraphics(bool watered)
	{
		if (watered)
		{
			ApplyGraphicsStype(styleWatered);
		}
		else
		{
			ApplyGraphicsStype(styleDry);
		}
	}

	private void ApplyGraphicsStype(GraphicsStyle style)
	{
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		propertyBlock.Clear();
		string text = "_TintColor";
		string text2 = "_Smoothness";
		propertyBlock.SetColor(text, style.color);
		propertyBlock.SetFloat(text2, style.smoothness);
		renderer.SetPropertyBlock(propertyBlock);
	}
}
