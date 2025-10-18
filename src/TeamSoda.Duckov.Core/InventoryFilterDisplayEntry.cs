using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryFilterDisplayEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI nameDisplay;

	[SerializeField]
	private GameObject selectedIndicator;

	private Action<InventoryFilterDisplayEntry, PointerEventData> onPointerClick;

	public InventoryFilterProvider.FilterEntry Filter { get; private set; }

	public void OnPointerClick(PointerEventData eventData)
	{
		onPointerClick?.Invoke(this, eventData);
	}

	internal void Setup(Action<InventoryFilterDisplayEntry, PointerEventData> onPointerClick, InventoryFilterProvider.FilterEntry filter)
	{
		this.onPointerClick = onPointerClick;
		Filter = filter;
		if ((bool)icon)
		{
			icon.sprite = filter.icon;
		}
		if ((bool)nameDisplay)
		{
			nameDisplay.text = filter.DisplayName;
		}
	}

	internal void NotifySelectionChanged(bool isThisSelected)
	{
		selectedIndicator.SetActive(isThisSelected);
	}
}
