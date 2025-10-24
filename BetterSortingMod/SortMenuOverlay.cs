using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BetterSortingMod;

internal class SortMenuOverlay : MonoBehaviour, IPointerClickHandler
{
	private Action _onClick;

	internal void Initialize(Action onClick)
	{
		_onClick = onClick;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.pointerPressRaycast.gameObject == gameObject)
		{
			_onClick?.Invoke();
		}
		else if (eventData.pointerCurrentRaycast.gameObject == gameObject)
		{
			_onClick?.Invoke();
		}
	}
}
