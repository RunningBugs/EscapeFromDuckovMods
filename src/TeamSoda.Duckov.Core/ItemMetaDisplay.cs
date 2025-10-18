using System;
using Duckov.Utilities;
using ItemStatsSystem;
using LeTai.TrueShadow;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemMetaDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IItemMetaDataProvider
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TrueShadow displayQualityShadow;

	private ItemMetaData data;

	public static event Action<ItemMetaDisplay> OnMouseEnter;

	public static event Action<ItemMetaDisplay> OnMouseExit;

	public ItemMetaData GetMetaData()
	{
		return data;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		ItemMetaDisplay.OnMouseEnter?.Invoke(this);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ItemMetaDisplay.OnMouseExit?.Invoke(this);
	}

	public void Setup(int typeID)
	{
		ItemMetaData metaData = ItemAssetsCollection.GetMetaData(typeID);
		Setup(metaData);
	}

	public void Setup(ItemMetaData data)
	{
		this.data = data;
		icon.sprite = data.icon;
		GameplayDataSettings.UIStyle.ApplyDisplayQualityShadow(data.displayQuality, displayQualityShadow);
	}

	internal void Setup(object rootTypeID)
	{
		throw new NotImplementedException();
	}
}
