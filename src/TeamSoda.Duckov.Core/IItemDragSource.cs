using System;
using Duckov.UI;
using ItemStatsSystem;
using UnityEngine.EventSystems;

public interface IItemDragSource : IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler
{
	static event Action<Item> OnStartDragItem;

	static event Action<Item> OnEndDragItem;

	bool IsEditable();

	Item GetItem();

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		if (IsEditable() && eventData.button == PointerEventData.InputButton.Left)
		{
			Item item = GetItem();
			IItemDragSource.OnStartDragItem?.Invoke(item);
			if (!(item == null))
			{
				ItemUIUtilities.NotifyPutItem(item, pickup: true);
			}
		}
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			Item item = GetItem();
			IItemDragSource.OnEndDragItem?.Invoke(item);
		}
	}
}
