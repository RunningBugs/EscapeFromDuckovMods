using System;
using System.Collections.Generic;
using Duckov.MiniMaps;
using Duckov.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class MapMarkerSettingsPanel : MonoBehaviour
{
	[SerializeField]
	private Color[] colors;

	[SerializeField]
	private MapMarkerPanelButton iconBtnTemplate;

	[SerializeField]
	private MapMarkerPanelButton colorBtnTemplate;

	private PrefabPool<MapMarkerPanelButton> _iconBtnPool;

	private PrefabPool<MapMarkerPanelButton> _colorBtnPool;

	private List<Sprite> Icons => MapMarkerManager.Icons;

	private PrefabPool<MapMarkerPanelButton> IconBtnPool
	{
		get
		{
			if (_iconBtnPool == null)
			{
				_iconBtnPool = new PrefabPool<MapMarkerPanelButton>(iconBtnTemplate);
			}
			return _iconBtnPool;
		}
	}

	private PrefabPool<MapMarkerPanelButton> ColorBtnPool
	{
		get
		{
			if (_colorBtnPool == null)
			{
				_colorBtnPool = new PrefabPool<MapMarkerPanelButton>(colorBtnTemplate);
			}
			return _colorBtnPool;
		}
	}

	private void OnEnable()
	{
		Setup();
		MapMarkerManager.OnColorChanged = (Action<Color>)Delegate.Combine(MapMarkerManager.OnColorChanged, new Action<Color>(OnColorChanged));
		MapMarkerManager.OnIconChanged = (Action<int>)Delegate.Combine(MapMarkerManager.OnIconChanged, new Action<int>(OnIconChanged));
	}

	private void OnDisable()
	{
		MapMarkerManager.OnColorChanged = (Action<Color>)Delegate.Remove(MapMarkerManager.OnColorChanged, new Action<Color>(OnColorChanged));
		MapMarkerManager.OnIconChanged = (Action<int>)Delegate.Remove(MapMarkerManager.OnIconChanged, new Action<int>(OnIconChanged));
	}

	private void OnIconChanged(int obj)
	{
		Setup();
	}

	private void OnColorChanged(Color color)
	{
		Setup();
	}

	private void Setup()
	{
		if (MapMarkerManager.Instance == null)
		{
			return;
		}
		IconBtnPool.ReleaseAll();
		ColorBtnPool.ReleaseAll();
		Color[] array = colors;
		foreach (Color cur in array)
		{
			MapMarkerPanelButton mapMarkerPanelButton = ColorBtnPool.Get();
			mapMarkerPanelButton.Image.color = cur;
			mapMarkerPanelButton.Setup(delegate
			{
				MapMarkerManager.SelectColor(cur);
			}, cur == MapMarkerManager.SelectedColor);
		}
		for (int num = 0; num < Icons.Count; num++)
		{
			Sprite sprite = Icons[num];
			if (!(sprite == null))
			{
				MapMarkerPanelButton mapMarkerPanelButton2 = IconBtnPool.Get();
				Image image = mapMarkerPanelButton2.Image;
				image.sprite = sprite;
				image.color = MapMarkerManager.SelectedColor;
				int index = num;
				mapMarkerPanelButton2.Setup(delegate
				{
					MapMarkerManager.SelectIcon(index);
				}, index == MapMarkerManager.SelectedIconIndex);
			}
		}
	}
}
