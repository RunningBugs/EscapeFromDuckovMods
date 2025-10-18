using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DragHandler : MonoBehaviour, IDragHandler, IEventSystemHandler
{
	public UnityEvent<PointerEventData> onDrag;

	public void OnDrag(PointerEventData eventData)
	{
		onDrag?.Invoke(eventData);
	}
}
