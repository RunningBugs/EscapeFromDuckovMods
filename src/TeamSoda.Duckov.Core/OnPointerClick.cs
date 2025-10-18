using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class OnPointerClick : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public UnityEvent<PointerEventData> onPointerClick;

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		onPointerClick?.Invoke(eventData);
	}
}
